using CommunityToolkit.Mvvm.ComponentModel;

namespace DiskGalaxy.UI.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    public ScanViewModel Scan { get; }
    public GalaxyViewModel Galaxy { get; }
    public SearchViewModel Search { get; }
    public FilterViewModel Filter { get; }

    public MainWindowViewModel(ScanViewModel scanViewModel, GalaxyViewModel galaxyViewModel,
        SearchViewModel searchViewModel, FilterViewModel filterViewModel)
    {
        Scan = scanViewModel;
        Galaxy = galaxyViewModel;
        Search = searchViewModel;
        Filter = filterViewModel;
    }
}
