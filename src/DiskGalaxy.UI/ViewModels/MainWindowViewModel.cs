using CommunityToolkit.Mvvm.ComponentModel;

namespace DiskGalaxy.UI.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    public ScanViewModel Scan { get; }

    public MainWindowViewModel(ScanViewModel scanViewModel)
    {
        Scan = scanViewModel;
    }
}
