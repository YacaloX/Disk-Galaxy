namespace DiskGalaxy.Core.Models;

public sealed record ScanResult(
    FolderGalaxy RootFolder,
    int TotalFiles,
    long TotalSize,
    TimeSpan Duration,
    IReadOnlyList<string> Errors)
{
    public bool HasErrors => Errors.Count > 0;
}
