using Avalonia;
using DiskGalaxy.Core.Scanning;
using DiskGalaxy.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DiskGalaxy.UI;

sealed class Program
{
    public static ServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/diskgalaxy.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(Log.Logger);

        services.AddSingleton<IFileSystemScanner, FileSystemScanner>();

        services.AddSingleton<GalaxyViewModel>();
        services.AddTransient<ScanViewModel>(sp =>
        {
            var scanner = sp.GetRequiredService<IFileSystemScanner>();
            var galaxy = sp.GetRequiredService<GalaxyViewModel>();
            var vm = new ScanViewModel(scanner);
            vm.OnScanCompleted += result => galaxy.LoadScanResult(result);
            return vm;
        });
        services.AddTransient<MainWindowViewModel>();
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
