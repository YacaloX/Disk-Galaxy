using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiskGalaxy.Core.Models;
using DiskGalaxy.Rendering.Scene;

namespace DiskGalaxy.UI.ViewModels;

public sealed partial class InspectorViewModel : ViewModelBase
{
    [ObservableProperty]
    private SceneNode? _node;

    public string DisplayName => Node?.Label ?? string.Empty;
    public string FullPath => Node?.FullPath ?? string.Empty;
    public string Extension => Node?.IsFolder == true ? "Folder" : (Path.GetExtension(Node?.FullPath) ?? "Unknown");
    public string SizeFormatted => FileSizeFormatter.Format(Node?.ByteSize ?? 0);
    public string CategoryName => GetCategoryName();
    public string CategoryColor => GetCategoryColor();
    public string ParentName => Node?.Parent?.Label ?? "(root)";
    public bool IsFolder => Node?.IsFolder ?? false;

    partial void OnNodeChanged(SceneNode? value)
    {
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(FullPath));
        OnPropertyChanged(nameof(Extension));
        OnPropertyChanged(nameof(SizeFormatted));
        OnPropertyChanged(nameof(CategoryName));
        OnPropertyChanged(nameof(CategoryColor));
        OnPropertyChanged(nameof(ParentName));
        OnPropertyChanged(nameof(IsFolder));
    }

    [RelayCommand]
    private void RevealInFileManager()
    {
        if (Node is null) return;

        try
        {
            var path = Node.FullPath;
            if (OperatingSystem.IsWindows())
            {
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
            else if (OperatingSystem.IsLinux())
            {
                var dir = Directory.Exists(path) ? path : Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                    Process.Start("xdg-open", $"\"{dir}\"");
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", $"\"{path}\"");
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Failed to reveal path in file manager: {Path}", Node.FullPath);
        }
    }

    private string GetCategoryName()
    {
        if (Node?.Data is FileStar star) return star.Category.ToString();
        if (Node?.IsFolder == true) return "Folder";
        return "Unknown";
    }

    private string GetCategoryColor()
    {
        if (Node?.Data is FileStar star)
        {
            return star.Category switch
            {
                FileCategory.Image => "#B388FF",
                FileCategory.Video => "#FF5252",
                FileCategory.Document => "#448AFF",
                FileCategory.Audio => "#69F0AE",
                FileCategory.Archive => "#FFD740",
                FileCategory.Executable => "#FFFF00",
                FileCategory.Code => "#40C4FF",
                _ => "#9E9E9E",
            };
        }
        return Node?.IsFolder == true ? "#6C63FF" : "#9E9E9E";
    }

}
