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

    [ObservableProperty]
    private float _fps;

    [ObservableProperty]
    private int _visibleNodes;

    [ObservableProperty]
    private int _totalNodes;

    [ObservableProperty]
    private long _totalSize;

    public InspectorViewModel Inspector { get; }

    public GalaxyViewModel()
    {
        Inspector = new InspectorViewModel();
    }

    partial void OnSelectedNodeChanged(SceneNode? value)
    {
        Inspector.Node = value;
    }

    public void UpdateStats(float fps, int visibleCount, int totalCount, long size)
    {
        Fps = fps;
        VisibleNodes = visibleCount;
        TotalNodes = totalCount;
        TotalSize = size;
    }

    public void LoadScanResult(ScanResult result)
    {
        var graph = SceneLoader.LoadFromScanResult(result);

        var layout = new HierarchyLayout();
        layout.Layout(graph);

        Serilog.Log.Information("SceneGraph loaded: {NodeCount} nodes, {EdgeCount} edges, total size: {Size}",
            graph.VisibleNodes.Count, graph.Edges.Count, result.TotalSize);

        SceneGraph = graph;
        TotalSize = result.TotalSize;
        TotalNodes = graph.AllNodes.Count;
    }

    public void Clear()
    {
        SceneGraph = null;
        SelectedNode = null;
    }
}
