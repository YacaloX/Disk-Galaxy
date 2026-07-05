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

    public string SizeFormatted => FileSizeFormatter.Format(Size);
}
