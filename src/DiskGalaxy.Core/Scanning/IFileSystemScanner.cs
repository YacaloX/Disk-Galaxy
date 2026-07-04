using DiskGalaxy.Core.Models;

namespace DiskGalaxy.Core.Scanning;

public interface IFileSystemScanner
{
    Task<ScanResult> ScanAsync(
        string path,
        IProgress<ScanProgress> progress,
        CancellationToken cancellationToken,
        ScanOptions? options = null);
}
