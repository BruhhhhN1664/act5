using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Desktop.Services;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    public StatusViewModel Status { get; }
    public EquipmentViewModel Equipment { get; }
    public StudentsViewModel Students { get; }
    public BorrowingsViewModel Borrowings { get; }
    public CheckoutViewModel Checkout { get; }
    public string ApiBaseUrl { get; }

    public MainWindowViewModel(
        StatusViewModel status, EquipmentViewModel equipment, StudentsViewModel students,
        BorrowingsViewModel borrowings, CheckoutViewModel checkout, RefreshCoordinator coordinator, ApiClient api)
    {
        Status = status;
        Equipment = equipment;
        Students = students;
        Borrowings = borrowings;
        Checkout = checkout;
        ApiBaseUrl = api.BaseAddress.ToString();
        coordinator.RefreshAll = RefreshAllAsync;
    }

    /// <summary>Reloads every tab from the API. Stops at the first failure so an unreachable API is reported once.</summary>
    public async Task RefreshAllAsync(bool quiet)
    {
        if (!await Borrowings.LoadAsync(quiet)) return;
        if (!await Students.LoadAsync(true)) return;
        if (!await Equipment.LoadAsync(true)) return;
        await Checkout.LoadAsync(true);
    }

    [RelayCommand]
    private async Task ReloadAllAsync() => await RefreshAllAsync(quiet: false);
}
