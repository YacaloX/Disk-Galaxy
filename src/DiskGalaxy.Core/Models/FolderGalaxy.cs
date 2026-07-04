namespace DiskGalaxy.Core.Models;

public sealed class FolderGalaxy
{
    public required string Name { get; init; }
    public required string FullPath { get; init; }
    public long TotalSize { get; set; }
    public int FileCount { get; set; }
    public int FolderCount { get; set; }
    public List<FolderGalaxy> Children { get; set; } = [];
    public List<FileStar> Stars { get; set; } = [];
}
