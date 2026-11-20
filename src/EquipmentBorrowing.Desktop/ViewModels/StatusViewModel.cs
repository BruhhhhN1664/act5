using CommunityToolkit.Mvvm.ComponentModel;

namespace EquipmentBorrowing.Desktop.ViewModels;

/// <summary>Shared loading / error / info state shown in the banner at the top of the window.</summary>
public sealed partial class StatusViewModel : ObservableObject
{
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasError))]
    private string? _error;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasInfo))]
    private string? _info;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _busyText = "";

    public bool HasError => !string.IsNullOrEmpty(Error);
    public bool HasInfo => !string.IsNullOrEmpty(Info);

    private int _busyCount;

    public void BeginBusy(string text)
    {
        _busyCount++;
        BusyText = text;
        IsBusy = true;
    }

    public void EndBusy()
    {
        _busyCount = Math.Max(0, _busyCount - 1);
        if (_busyCount == 0)
        {
            IsBusy = false;
            BusyText = "";
        }
    }

    public void Clear()
    {
        Error = null;
        Info = null;
    }
}
