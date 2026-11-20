using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Contracts;
using EquipmentBorrowing.Desktop.Services;

namespace EquipmentBorrowing.Desktop.ViewModels;

/// <summary>UC-04, 05, 09 over API-12, 14 (borrowing list, search/filter/sort, return).</summary>
public sealed partial class BorrowingsViewModel : ObservableObject
{
    private readonly ApiClient _api;
    private readonly OperationRunner _run;
    private readonly RefreshCoordinator _refresh;

    public StatusViewModel Status { get; }
    public ObservableCollection<BorrowingDto> Items { get; } = new();

    public string[] StatusOptions { get; } = { "All", "Active", "Returned" };
    public string[] SortOptions { get; } = { "borrowedDate", "expectedReturnDate" };
    public string[] DirectionOptions { get; } = { "asc", "desc" };

    [ObservableProperty] private BorrowingDto? _selected;
    [ObservableProperty] private string _search = "";
    [ObservableProperty] private string _selectedStatus = "All";
    [ObservableProperty] private string _selectedSort = "borrowedDate";
    [ObservableProperty] private string _selectedDirection = "asc";
    [ObservableProperty] private string _summary = "";

    public BorrowingsViewModel(ApiClient api, OperationRunner run, RefreshCoordinator refresh, StatusViewModel status)
    {
        _api = api;
        _run = run;
        _refresh = refresh;
        Status = status;
    }

    public Task<bool> LoadAsync(bool quiet) =>
        _run.RunAsync(async () =>
        {
            var keep = Selected?.Id;
            var list = await _api.GetBorrowingsAsync(Search, SelectedStatus, SelectedSort, SelectedDirection);
            Items.Clear();
            foreach (var b in list) Items.Add(b);
            Selected = keep is null ? null : Items.FirstOrDefault(x => x.Id == keep);
            Summary = $"{list.Count} borrowing(s) shown";
        }, "Loading borrowings…", clearMessages: !quiet);

    [RelayCommand]
    private async Task RefreshAsync() => await LoadAsync(quiet: false);

    [RelayCommand]
    private async Task ShowActiveAsync()
    {
        SelectedStatus = "Active"; // UC-04
        await LoadAsync(quiet: false);
    }

    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        Search = "";
        SelectedStatus = "All";
        SelectedSort = "borrowedDate";
        SelectedDirection = "asc";
        await LoadAsync(quiet: false);
    }

    [RelayCommand]
    private async Task ReturnAsync()
    {
        if (Selected is null || Selected.Status != BorrowingStatus.Active)
        {
            Status.Clear();
            Status.Error = "Select an ACTIVE borrowing first, then click Return selected.";
            return;
        }
        var b = Selected;
        var ok = await _run.RunAsync(async () =>
        {
            var returned = await _api.ReturnAsync(b.Id);
            Status.Info = $"{returned.EquipmentCode} ({returned.EquipmentName}) returned on {returned.ReturnedDate:yyyy-MM-dd}.";
        }, "Returning item…");
        if (ok) await _refresh.RefreshAsync();
    }
}
