using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Contracts;
using EquipmentBorrowing.Desktop.Services;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed partial class SelectableEquipment : ObservableObject
{
    public SelectableEquipment(EquipmentDto item) => Item = item;
    public EquipmentDto Item { get; }
    public string Label => $"{Item.EquipmentCode} — {Item.Name}";
    [ObservableProperty] private bool _isSelected;
}

/// <summary>UC-03 (single, API-10) and UC-06 (atomic multi-item, API-11).</summary>
public sealed partial class CheckoutViewModel : ObservableObject
{
    private readonly ApiClient _api;
    private readonly OperationRunner _run;
    private readonly RefreshCoordinator _refresh;

    public StatusViewModel Status { get; }
    public ObservableCollection<StudentDto> Students { get; } = new();
    public ObservableCollection<SelectableEquipment> Equipment { get; } = new();

    [ObservableProperty] private StudentDto? _selectedStudent;
    [ObservableProperty] private string _expectedReturnDate = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public CheckoutViewModel(ApiClient api, OperationRunner run, RefreshCoordinator refresh, StatusViewModel status)
    {
        _api = api;
        _run = run;
        _refresh = refresh;
        Status = status;
    }

    public Task<bool> LoadAsync(bool quiet) =>
        _run.RunAsync(async () =>
        {
            var keepStudent = SelectedStudent?.Id;
            var keepItems = Equipment.Where(e => e.IsSelected).Select(e => e.Item.Id).ToHashSet();

            var students = await _api.GetStudentsAsync(null);
            var available = await _api.GetEquipmentAsync(availableOnly: true);

            Students.Clear();
            foreach (var s in students) Students.Add(s);
            SelectedStudent = keepStudent is null ? null : Students.FirstOrDefault(x => x.Id == keepStudent);

            Equipment.Clear();
            foreach (var e in available)
                Equipment.Add(new SelectableEquipment(e) { IsSelected = keepItems.Contains(e.Id) });
        }, "Loading students and available equipment…", clearMessages: !quiet);

    [RelayCommand]
    private async Task RefreshAsync() => await LoadAsync(quiet: false);

    [RelayCommand]
    private async Task SubmitAsync()
    {
        Status.Clear();
        if (SelectedStudent is null) { Status.Error = "Choose a student first."; return; }
        if (!DateOnly.TryParseExact(ExpectedReturnDate.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var due))
        {
            Status.Error = "Expected return date must look like 2026-11-12 (YYYY-MM-DD).";
            return;
        }
        var chosen = Equipment.Where(e => e.IsSelected).Select(e => e.Item).ToList();
        if (chosen.Count == 0) { Status.Error = "Tick at least one available item."; return; }

        var student = SelectedStudent;
        var ok = await _run.RunAsync(async () =>
        {
            if (chosen.Count == 1)
            {
                var b = await _api.BorrowAsync(new CreateBorrowingRequest(student.Id, chosen[0].Id, due));
                Status.Info = $"Issued {b.EquipmentCode} to {b.StudentNumber}, due {b.ExpectedReturnDate:yyyy-MM-dd}.";
            }
            else
            {
                var result = await _api.CheckoutAsync(new CheckoutRequest(student.Id, chosen.Select(c => c.Id).ToList(), due));
                Status.Info = $"Checked out {result.Borrowings.Count} items to {student.StudentNumber} in one atomic request, due {due:yyyy-MM-dd}.";
            }
        }, "Checking out…", clearMessages: false);

        if (ok) await _refresh.RefreshAsync();
    }
}
