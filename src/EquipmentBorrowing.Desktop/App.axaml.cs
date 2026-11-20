using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Desktop.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowing.Desktop;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var provider = BuildServices();
            desktop.MainWindow = new MainWindow { DataContext = provider.GetRequiredService<MainWindowViewModel>() };
        }
        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider BuildServices()
    {
        // API base URL: appsettings.json "Api:BaseUrl", overridable with the env var EQUIPMENT_Api__BaseUrl.
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables("EQUIPMENT_")
            .Build();

        var baseUrl = config["Api:BaseUrl"] ?? "http://localhost:5080/";
        if (!baseUrl.EndsWith('/')) baseUrl += "/";

        var services = new ServiceCollection();
        services.AddSingleton(new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(15) });
        services.AddSingleton<ApiClient>();
        services.AddSingleton<StatusViewModel>();
        services.AddSingleton<RefreshCoordinator>();
        services.AddSingleton<OperationRunner>();
        services.AddSingleton<EquipmentViewModel>();
        services.AddSingleton<StudentsViewModel>();
        services.AddSingleton<BorrowingsViewModel>();
        services.AddSingleton<CheckoutViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        return services.BuildServiceProvider();
    }
}
