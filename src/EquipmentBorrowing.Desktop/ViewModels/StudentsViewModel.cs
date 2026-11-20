using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Contracts;
using EquipmentBorrowing.Desktop.Services;

namespace EquipmentBorrowing.Desktop.ViewModels;

/// <summary>UC-07, 12, 13, 14 over API-02..06, 15.</summary>
public sealed partial class StudentsViewModel : ObservableObject
{
    private readonly ApiClient _api;
    private readonly OperationRunner _run;
    private readonly RefreshCoordinator _refresh;

    public StatusViewModel Status { get; }
    public ObservableCollection<StudentDto> Items { get; } = new();
    public ObservableCollection<BorrowingDto> History { get; } = new();

    [ObservableProperty] private StudentDto? _selected;
    [ObservableProperty] private string _search = "";
    [ObservableProperty] private string _number = "";
    [ObservableProperty] private string _fullName = "";
    [ObservableProperty] private string _contact = "";
    [ObservableProperty] private string _historyTitle = "History (select a student, then click Show history)";

    public StudentsViewModel(ApiClient api, OperationRunner run, RefreshCoordinator refresh, StatusViewModel status)
    {
        _api = api;
        _run = run;
        _refresh = refresh;
        Status = status;
    }

    partial void OnSelectedChanged(StudentDto? value)
    {
        if (value is null) return;
        Number = value.StudentNumber;
        FullName = value.FullName;
        Contact = value.Contact ?? "";
    }

    public Task<bool> LoadAsync(bool quiet) =>
        _run.RunAsync(async () =>
        {
            var keep = Selected?.Id;
            var list = await _api.GetStudentsAsync(Search);
            Items.Clear();
            foreach (var s in list) Items.Add(s);
            Selected = keep is null ? null : Items.FirstOrDefault(x => x.Id == keep);
        }, "Loading students…", clearMessages: !quiet);

    [RelayCommand]
    private async Task RefreshAsync() => await LoadAsync(quiet: false);

    [RelayCommand]
    private async Task AddAsync()
    {
        if (!Valid()) return;
        var ok = await _run.RunAsync(async () =>
        {
            var created = await _api.CreateStudentAsync(new CreateStudentRequest(Number, FullName, NullIfBlank(Contact)));
            Status.Info = $"Registered student {created.StudentNumber} — {created.FullName} (eligible).";
        }, "Saving student…");
        if (ok)
        {
            ClearForm();
            await _refresh.RefreshAsync();
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Selected is null) { Fail("Select a student row first, then edit its fields and click Save changes."); return; }
        if (!Valid()) return;
        var id = Selected.Id;
        var ok = await _run.RunAsync(async () =>
        {
            var updated = await _api.UpdateStudentAsync(id, new UpdateStudentRequest(Number, FullName, NullIfBlank(Contact)));
            Status.Info = $"Saved student {updated.StudentNumber}.";
        }, "Saving student…");
        if (ok) await _refresh.RefreshAsync();
    }

    [RelayCommand]
    private async Task ToggleEligibilityAsync()
    {
        if (Selected is null) { Fail("Select a student row first."); return; }
        var s = Selected;
        var ok = await _run.RunAsync(async () =>
        {
            var updated = await _api.SetEligibilityAsync(s.Id, !s.IsEligible);
            Status.Info = $"{updated.StudentNumber} is now {(updated.IsEligible ? "eligible" : "restricted")}.";
        }, "Updating eligibility…");
        if (ok) await _refresh.RefreshAsync();
    }

    [RelayCommand]
    private async Task ShowHistoryAsync()
    {
        if (Selected is null) { Fail("Select a student row first."); return; }
        var s = Selected;
        await _run.RunAsync(async () =>
        {
            var rows = await _api.GetStudentBorrowingsAsync(s.Id);
            History.Clear();
            foreach (var b in rows) History.Add(b);
            HistoryTitle = $"History of {s.StudentNumber} — {s.FullName} ({rows.Count} record(s))";
        }, "Loading history…");
    }

    [RelayCommand]
    private void ClearForm()
    {
        Selected = null;
        Number = "";
        FullName = "";
        Contact = "";
    }

    private bool Valid()
    {
        if (string.IsNullOrWhiteSpace(Number) || string.IsNullOrWhiteSpace(FullName))
        {
            Fail("Student number and full name are required.");
            return false;
        }
        return true;
    }

    private void Fail(string message)
    {
        Status.Clear();
        Status.Error = message;
    }

    private static string? NullIfBlank(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
