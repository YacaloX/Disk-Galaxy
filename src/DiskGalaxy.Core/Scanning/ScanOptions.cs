namespace DiskGalaxy.Core.Scanning;

public sealed record ScanOptions
{
    public int MaxDepth { get; init; } = -1;
    public bool FollowSymlinks { get; init; }
    public long? MinSize { get; init; }
    public long? MaxSize { get; init; }
    public int ProgressInterval { get; init; } = 100;

    public static readonly ScanOptions Default = new();
}
