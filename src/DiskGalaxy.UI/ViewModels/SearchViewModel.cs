using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiskGalaxy.Rendering.Scene;
using Silk.NET.Maths;

namespace DiskGalaxy.UI.ViewModels;

public sealed partial class SearchViewModel : ViewModelBase
{
    private CancellationTokenSource? _debounceCts;
    private SceneGraph? _sceneGraph;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isSearching;

    public ObservableCollection<SceneNode> Results { get; } = [];

    public SceneGraph? SceneGraph
    {
        set
        {
            _sceneGraph = value;
            ClearSearch();
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        _ = DebounceSearchAsync(value, token);
    }

    private async Task DebounceSearchAsync(string query, CancellationToken token)
    {
        try
        {
            await Task.Delay(150, token);
            if (token.IsCancellationRequested) return;

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => PerformSearch(query));
        }
        catch (TaskCanceledException)
        {
            // Debounce cancelled, ignore
        }
    }

    public event Action? SearchApplied;

    private void PerformSearch(string query)
    {
        ResetHighlights();
        Results.Clear();

        if (string.IsNullOrWhiteSpace(query) || _sceneGraph is null)
        {
            IsSearching = false;
            SearchApplied?.Invoke();
            return;
        }

        IsSearching = true;
        var lower = query.ToLowerInvariant();

        foreach (var node in _sceneGraph.AllNodes)
        {
            if (node.Label.Contains(lower, StringComparison.OrdinalIgnoreCase) ||
                node.FullPath.Contains(lower, StringComparison.OrdinalIgnoreCase))
            {
                node.IsHighlighted = true;
                node.Color = node.BaseColor * 1.6f + new Vector3D<float>(0.2f, 0.2f, 0.2f);
                Results.Add(node);
            }
        }

        IsSearching = false;
        SearchApplied?.Invoke();
    }

    public void ClearSearch()
    {
        ResetHighlights();
        SearchQuery = string.Empty;
        Results.Clear();
        IsSearching = false;
        SearchApplied?.Invoke();
    }

    private void ResetHighlights()
    {
        if (_sceneGraph is null) return;
        foreach (var node in _sceneGraph.AllNodes)
        {
            node.IsHighlighted = false;
            node.Color = node.BaseColor;
        }
    }
}
