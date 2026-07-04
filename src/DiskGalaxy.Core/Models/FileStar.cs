namespace DiskGalaxy.Core.Models;

public sealed class FileStar
{
    public required string Name { get; init; }
    public required string FullPath { get; init; }
    public string Extension => Path.GetExtension(FullPath);
    public long Size { get; init; }
    public FileCategory Category { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime ModifiedAt { get; init; }
    public required string ParentFolder { get; init; }
    public int Depth { get; init; }

    public string SizeFormatted => Size switch
    {
        < 1024 => $"{Size} B",
        < 1024 * 1024 => $"{Size / 1024.0:F1} KB",
        < 1024L * 1024 * 1024 => $"{Size / (1024.0 * 1024):F1} MB",
        _ => $"{Size / (1024.0 * 1024 * 1024):F2} GB"
    };
}
