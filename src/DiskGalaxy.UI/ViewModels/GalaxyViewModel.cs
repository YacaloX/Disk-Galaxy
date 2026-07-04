using CommunityToolkit.Mvvm.ComponentModel;
using DiskGalaxy.Core.Models;
using DiskGalaxy.Rendering.Layout;
using DiskGalaxy.Rendering.Scene;

namespace DiskGalaxy.UI.ViewModels;

public sealed partial class GalaxyViewModel : ViewModelBase
{
    [ObservableProperty]
    private SceneGraph? _sceneGraph;

    public void LoadScanResult(ScanResult result)
    {
        var graph = SceneLoader.LoadFromScanResult(result);

        var layout = new HierarchyLayout();
        layout.Layout(graph);

        SceneGraph = graph;
    }

    public void Clear()
    {
        SceneGraph = null;
    }
}
