using DiskGalaxy.Core.Models;
using Silk.NET.Maths;

namespace DiskGalaxy.Rendering.Scene;

public static class SceneLoader
{
    public static SceneGraph LoadFromScanResult(ScanResult result)
    {
        var root = BuildNode(result.RootFolder, null);
        return new SceneGraph(root);
    }

    private static SceneNode BuildNode(FolderGalaxy folder, SceneNode? parent, int depth = 0)
    {
        var color = GetFolderColor(folder);
        var node = new SceneNode
        {
            Label = folder.Name,
            FullPath = folder.FullPath,
            Color = color,
            BaseColor = color,
            IsFolder = true,
            IsExpanded = depth <= 1,
            ByteSize = folder.TotalSize,
            Parent = parent,
            Data = folder,
        };

        foreach (var childFolder in folder.Children)
        {
            var childNode = BuildNode(childFolder, node, depth + 1);
            node.Children.Add(childNode);
        }

        foreach (var star in folder.Stars)
        {
            var starColor = GetFileColor(star);
            var fileNode = new SceneNode
            {
                Label = star.Name,
                FullPath = star.FullPath,
                Color = starColor,
                BaseColor = starColor,
                IsFolder = false,
                IsExpanded = false,
                ByteSize = star.Size,
                Parent = node,
                Data = star,
            };
            node.Children.Add(fileNode);
        }

        return node;
    }

    private static Vector3D<float> GetFolderColor(FolderGalaxy folder)
    {
        var dominant = folder.Stars
            .GroupBy(s => s.Category)
            .OrderByDescending(g => g.Sum(s => s.Size))
            .FirstOrDefault();

        return dominant is not null
            ? CategoryToColor(dominant.Key) * 0.7f + new Vector3D<float>(0.3f, 0.3f, 0.3f)
            : new Vector3D<float>(0.4f, 0.4f, 0.6f);
    }

    private static Vector3D<float> GetFileColor(FileStar star)
    {
        return CategoryToColor(star.Category);
    }

    private static Vector3D<float> CategoryToColor(FileCategory category)
    {
        return category switch
        {
            FileCategory.Image => new Vector3D<float>(0.7f, 0.3f, 0.8f),
            FileCategory.Video => new Vector3D<float>(0.9f, 0.2f, 0.2f),
            FileCategory.Document => new Vector3D<float>(0.2f, 0.5f, 0.9f),
            FileCategory.Audio => new Vector3D<float>(0.2f, 0.8f, 0.3f),
            FileCategory.Archive => new Vector3D<float>(0.9f, 0.6f, 0.1f),
            FileCategory.Executable => new Vector3D<float>(0.9f, 0.9f, 0.2f),
            FileCategory.Code => new Vector3D<float>(0.3f, 0.8f, 0.7f),
            FileCategory.Font => new Vector3D<float>(0.6f, 0.4f, 0.8f),
            FileCategory.Database => new Vector3D<float>(0.5f, 0.3f, 0.7f),
            FileCategory.Config => new Vector3D<float>(0.6f, 0.6f, 0.6f),
            FileCategory.Temporary => new Vector3D<float>(0.3f, 0.3f, 0.3f),
            _ => new Vector3D<float>(0.5f, 0.5f, 0.5f),
        };
    }
}
