using CommunityToolkit.Mvvm.ComponentModel;

namespace DiskGalaxy.UI.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    public ScanViewModel Scan { get; }
    public GalaxyViewModel Galaxy { get; }

    public MainWindowViewModel(ScanViewModel scanViewModel, GalaxyViewModel galaxyViewModel)
    {
        Scan = scanViewModel;
        Galaxy = galaxyViewModel;
    }
}
