using CommunityToolkit.Mvvm.ComponentModel;
using DiskGalaxy.Core.Models;
using DiskGalaxy.Rendering.Scene;
using Silk.NET.Maths;

namespace DiskGalaxy.UI.ViewModels;

public sealed partial class FilterViewModel : ViewModelBase
{
    private SceneGraph? _sceneGraph;

    [ObservableProperty]
    private bool _showImages = true;

    [ObservableProperty]
    private bool _showVideos = true;

    [ObservableProperty]
    private bool _showDocuments = true;

    [ObservableProperty]
    private bool _showAudio = true;

    [ObservableProperty]
    private bool _showArchives = true;

    [ObservableProperty]
    private bool _showExecutables = true;

    [ObservableProperty]
    private bool _showCode = true;

    [ObservableProperty]
    private bool _showOther = true;

    [ObservableProperty]
    private double _minSizeMb;

    [ObservableProperty]
    private double _maxSizeMb = 10240;

    [ObservableProperty]
    private bool _recentFilesOnly;

    [ObservableProperty]
    private bool _largeFilesOnly;

    [ObservableProperty]
    private bool _hideEmptyFolders;

    [ObservableProperty]
    private int _selectedPresetIndex;

    public SceneGraph? SceneGraph
    {
        set
        {
            _sceneGraph = value;
            if (value is not null)
                ApplyFilters();
        }
    }

    public string[] Presets { get; } = ["All", "Large files only", "Recent only", "Media files"];

    partial void OnShowImagesChanged(bool value) => ApplyFilters();
    partial void OnShowVideosChanged(bool value) => ApplyFilters();
    partial void OnShowDocumentsChanged(bool value) => ApplyFilters();
    partial void OnShowAudioChanged(bool value) => ApplyFilters();
    partial void OnShowArchivesChanged(bool value) => ApplyFilters();
    partial void OnShowExecutablesChanged(bool value) => ApplyFilters();
    partial void OnShowCodeChanged(bool value) => ApplyFilters();
    partial void OnShowOtherChanged(bool value) => ApplyFilters();
    partial void OnMinSizeMbChanged(double value) => ApplyFilters();
    partial void OnMaxSizeMbChanged(double value) => ApplyFilters();
    partial void OnRecentFilesOnlyChanged(bool value) => ApplyFilters();
    partial void OnLargeFilesOnlyChanged(bool value) => ApplyFilters();
    partial void OnHideEmptyFoldersChanged(bool value) => ApplyFilters();

    partial void OnSelectedPresetIndexChanged(int value)
    {
        ApplyPreset(value);
    }

    private void ApplyPreset(int index)
    {
        switch (index)
        {
            case 1: // Large files only
                ShowImages = ShowVideos = ShowDocuments = ShowAudio = ShowArchives = ShowExecutables = ShowCode = ShowOther = true;
                MinSizeMb = 100;
                MaxSizeMb = 10240;
                RecentFilesOnly = false;
                LargeFilesOnly = true;
                HideEmptyFolders = false;
                break;
            case 2: // Recent only
                ShowImages = ShowVideos = ShowDocuments = ShowAudio = ShowArchives = ShowExecutables = ShowCode = ShowOther = true;
                MinSizeMb = 0;
                MaxSizeMb = 10240;
                RecentFilesOnly = true;
                LargeFilesOnly = false;
                HideEmptyFolders = false;
                break;
            case 3: // Media files
                ShowImages = ShowVideos = true;
                ShowDocuments = ShowAudio = ShowArchives = ShowExecutables = ShowCode = ShowOther = false;
                MinSizeMb = 0;
                MaxSizeMb = 10240;
                RecentFilesOnly = false;
                LargeFilesOnly = false;
                HideEmptyFolders = true;
                break;
            default: // All
                ShowImages = ShowVideos = ShowDocuments = ShowAudio = ShowArchives = ShowExecutables = ShowCode = ShowOther = true;
                MinSizeMb = 0;
                MaxSizeMb = 10240;
                RecentFilesOnly = false;
                LargeFilesOnly = false;
                HideEmptyFolders = false;
                break;
        }
    }

    public event Action? FiltersApplied;

    private void ApplyFilters()
    {
        if (_sceneGraph is null) return;

        var minBytes = (long)(MinSizeMb * 1024 * 1024);
        var maxBytes = (long)(MaxSizeMb * 1024 * 1024);
        var cutoffDate = DateTime.UtcNow.AddDays(-7);

        foreach (var node in _sceneGraph.AllNodes)
        {
            if (node.IsFolder)
            {
                node.Color = node.BaseColor;
                continue;
            }

            var category = GetNodeCategory(node);
            var visible = IsCategoryVisible(category)
                && node.ByteSize >= minBytes
                && node.ByteSize <= maxBytes
                && (!RecentFilesOnly || GetNodeModified(node) >= cutoffDate)
                && (!LargeFilesOnly || node.ByteSize >= 1024L * 1024 * 1024);

            node.Color = visible
                ? node.BaseColor
                : node.BaseColor * new Vector3D<float>(0.15f, 0.15f, 0.15f);
        }

        FiltersApplied?.Invoke();
    }

    private static FileCategory GetNodeCategory(SceneNode node)
    {
        if (node.Data is Core.Models.FileStar star) return star.Category;
        return Core.Models.FileCategory.Unknown;
    }

    private static DateTime GetNodeModified(SceneNode node)
    {
        if (node.Data is Core.Models.FileStar star) return star.ModifiedAt;
        return DateTime.MinValue;
    }

    private bool IsCategoryVisible(FileCategory category) => category switch
    {
        FileCategory.Image => ShowImages,
        FileCategory.Video => ShowVideos,
        FileCategory.Document => ShowDocuments,
        FileCategory.Audio => ShowAudio,
        FileCategory.Archive => ShowArchives,
        FileCategory.Executable => ShowExecutables,
        FileCategory.Code => ShowCode,
        _ => ShowOther,
    };
}
