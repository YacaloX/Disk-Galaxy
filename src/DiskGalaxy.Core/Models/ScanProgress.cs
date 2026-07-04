namespace DiskGalaxy.Core.Models;

public sealed record ScanProgress(
    string CurrentPath,
    int FilesScanned,
    int FoldersScanned,
    long TotalSize,
    TimeSpan Elapsed,
    bool IsIndeterminate)
{
    public static ScanProgress Indeterminate(string path) =>
        new(path, 0, 0, 0, TimeSpan.Zero, true);
}
