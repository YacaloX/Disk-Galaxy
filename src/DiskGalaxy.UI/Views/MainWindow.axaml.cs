using Avalonia.Controls;
using Avalonia.Threading;
using DiskGalaxy.Rendering.Scene;
using DiskGalaxy.UI.ViewModels;
using Serilog;

namespace DiskGalaxy.UI.Views;

public partial class MainWindow : Window
{
    private DispatcherTimer? _statsTimer;

    public MainWindow()
    {
        InitializeComponent();

        GalaxyView.SelectionChanged += OnSelectionChanged;
        GalaxyView.NodeDoubleClicked += OnNodeDoubleClicked;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.Search.SearchApplied += RefreshScene;
            vm.Filter.FiltersApplied += RefreshScene;
            vm.Galaxy.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(GalaxyViewModel.SceneGraph))
                    RefreshScene();
            };
        }

        _statsTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(0.5),
        };
        _statsTimer.Tick += OnStatsTick;
        _statsTimer.Start();
    }

    private void OnUnloaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _statsTimer?.Stop();
    }

    private void OnStatsTick(object? sender, EventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm || GalaxyView.Engine is null) return;

        var engine = GalaxyView.Engine;
        vm.Galaxy.UpdateStats(
            engine.CurrentFps,
            engine.NodeCount,
            vm.Galaxy.TotalNodes,
            vm.Galaxy.TotalSize);
    }

    private void RefreshScene()
    {
        if (DataContext is not MainWindowViewModel vm) return;

        vm.Galaxy.SceneGraph?.RebuildVisible();

        if (GalaxyView.Engine is not null && vm.Galaxy.SceneGraph is not null)
        {
            GalaxyView.Engine.SetScene(vm.Galaxy.SceneGraph);
        }
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