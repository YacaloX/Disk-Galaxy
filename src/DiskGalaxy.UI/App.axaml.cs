using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DiskGalaxy.Core.Scanning;
using DiskGalaxy.UI.ViewModels;
using DiskGalaxy.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DiskGalaxy.UI;

public sealed partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = Program.ServiceProvider.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel,
            };

            desktop.MainWindow.Loaded += async (_, _) =>
            {
                if (!string.IsNullOrEmpty(mainViewModel.Scan.SelectedPath))
                    await mainViewModel.Scan.StartScanCommand.ExecuteAsync(null);
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
