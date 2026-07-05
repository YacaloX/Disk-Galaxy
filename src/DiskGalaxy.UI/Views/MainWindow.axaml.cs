using Avalonia.Controls;
using DiskGalaxy.Rendering.Scene;
using DiskGalaxy.UI.ViewModels;
using Serilog;

namespace DiskGalaxy.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        GalaxyView.SelectionChanged += OnSelectionChanged;
        GalaxyView.NodeDoubleClicked += OnNodeDoubleClicked;
    }

    private void OnSelectionChanged(SceneNode? node)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.Galaxy.SelectedNode = node;
        }
    }

    private void OnNodeDoubleClicked(SceneNode? node)
    {
        if (node is null || DataContext is not MainWindowViewModel vm) return;

        if (node.IsFolder)
        {
            node.IsExpanded = true;
            if (vm.Galaxy.SceneGraph is not null)
            {
                vm.Galaxy.SceneGraph.RebuildVisible();
            }
            if (GalaxyView.Engine is not null)
            {
                if (vm.Galaxy.SceneGraph is not null)
                    GalaxyView.Engine.SetScene(vm.Galaxy.SceneGraph);
                GalaxyView.Engine.FlyToNode(node);
            }
        }
    }
}