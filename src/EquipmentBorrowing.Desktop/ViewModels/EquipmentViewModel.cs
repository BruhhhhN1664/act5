using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Contracts;
using EquipmentBorrowing.Desktop.Services;

namespace EquipmentBorrowing.Desktop.ViewModels;

/// <summary>UC-01, 02, 08, 15, 16 over API-01, 07, 08, 09, 16.</summary>
public sealed partial class EquipmentViewModel : ObservableObject
{
    private readonly ApiClient _api;
    private readonly OperationRunner _run;
    private readonly RefreshCoordinator _refresh;

    public StatusViewModel Status { get; }
    public ObservableCollection<EquipmentDto> Items { get; } = new();
    public ObservableCollection<BorrowingDto> History { get; } = new();

    [ObservableProperty] private EquipmentDto? _selected;
    [ObservableProperty] private bool _availableOnly;
    [ObservableProperty] private string _code = "";
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private string _historyTitle = "History (select an item, then click Show history)";

    public EquipmentViewModel(ApiClient api, OperationRunner run, RefreshCoordinator refresh, StatusViewModel status)
    {
        _api = api;
        _run = run;
        _refresh = refresh;
        Status = status;
    }

    partial void OnSelectedChanged(EquipmentDto? value)
    {
        if (value is null) return;
        Code = value.EquipmentCode;
        Name = value.Name;
        Description = value.Description ?? "";
    }

    partial void OnAvailableOnlyChanged(bool value) => _ = LoadAsync(quiet: false);

    public Task<bool> LoadAsync(bool quiet) =>
        _run.RunAsync(async () =>
        {
            var keep = Selected?.Id;
            var list = await _api.GetEquipmentAsync(AvailableOnly);
            Items.Clear();
            foreach (var e in list) Items.Add(e);
            Selected = keep is null ? null : Items.FirstOrDefault(x => x.Id == keep);
        }, "Loading equipment…", clearMessages: !quiet);

    [RelayCommand]
    private async Task RefreshAsync() => await LoadAsync(quiet: false);

    [RelayCommand]
    private async Task AddAsync()
    {
        if (!Valid()) return;
        var ok = await _run.RunAsync(async () =>
        {
            var created = await _api.CreateEquipmentAsync(new CreateEquipmentRequest(Code, Name, NullIfBlank(Description)));
            Status.Info = $"Registered {created.EquipmentCode} — {created.Name} (available).";
        }, "Saving equipment…");
        if (ok)
        {
            ClearForm();
            await _refresh.RefreshAsync();
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Selected is null)
        {
            Fail("Select an equipment row first, then edit its fields and click Save changes.");
            return;
        }
        if (!Valid()) return;
        var id = Selected.Id;
        var ok = await _run.RunAsync(async () =>
        {
            var updated = await _api.UpdateEquipmentAsync(id, new UpdateEquipmentRequest(Code, Name, NullIfBlank(Description)));
            Status.Info = $"Saved {updated.EquipmentCode}.";
        }, "Saving equipment…");
        if (ok) await _refresh.RefreshAsync();
    }

    [RelayCommand]
    private async Task ShowHistoryAsync()
    {
        if (Selected is null) { Fail("Select an equipment row first."); return; }
        var item = Selected;
        await _run.RunAsync(async () =>
        {
            var rows = await _api.GetEquipmentBorrowingsAsync(item.Id);
            History.Clear();
            foreach (var b in rows) History.Add(b);
            HistoryTitle = $"History of {item.EquipmentCode} — {item.Name} ({rows.Count} record(s))";
        }, "Loading history…");
    }

    [RelayCommand]
    private void ClearForm()
    {
        Selected = null;
        Code = "";
        Name = "";
        Description = "";
    }

    private bool Valid()
    {
        if (string.IsNullOrWhiteSpace(Code) || string.IsNullOrWhiteSpace(Name))
        {
            Fail("Equipment code and name are required.");
            return false;
        }
        return true; // the server re-validates everything (uniqueness, lengths)
    }

    private void Fail(string message)
    {
        Status.Clear();
        Status.Error = message;
    }

    private static string? NullIfBlank(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
