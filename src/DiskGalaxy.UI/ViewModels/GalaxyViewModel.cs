using CommunityToolkit.Mvvm.ComponentModel;
using DiskGalaxy.Core.Models;
using DiskGalaxy.Rendering.Layout;
using DiskGalaxy.Rendering.Scene;

namespace DiskGalaxy.UI.ViewModels;

public sealed partial class GalaxyViewModel : ViewModelBase
{
    [ObservableProperty]
    private SceneGraph? _sceneGraph;

    [ObservableProperty]
    private SceneNode? _selectedNode;

    public void LoadScanResult(ScanResult result)
    {
        var graph = SceneLoader.LoadFromScanResult(result);

        var layout = new HierarchyLayout();
        layout.Layout(graph);

        Serilog.Log.Information("SceneGraph loaded: {NodeCount} nodes, {EdgeCount} edges, total size: {Size}",
            graph.VisibleNodes.Count, graph.Edges.Count, result.TotalSize);

        SceneGraph = graph;
    }

    public void Clear()
    {
        SceneGraph = null;
        SelectedNode = null;
    }
}
