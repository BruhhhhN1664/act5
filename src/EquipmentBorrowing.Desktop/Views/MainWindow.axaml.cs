using Avalonia.Controls;
using EquipmentBorrowing.Desktop.ViewModels;

namespace EquipmentBorrowing.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Initial load is asynchronous: the window appears immediately and shows a loading bar.
        Opened += async (_, _) =>
        {
            if (DataContext is MainWindowViewModel vm)
                await vm.RefreshAllAsync(quiet: false);
        };
    }
}
