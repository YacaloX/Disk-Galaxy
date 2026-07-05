using System.Runtime.CompilerServices;
using DiskGalaxy.Core.Models;
using Serilog;

namespace DiskGalaxy.Core.Scanning;

public sealed class FileSystemScanner : IFileSystemScanner
{
    private readonly ILogger _logger;

    public FileSystemScanner(ILogger logger)
    {
        _logger = logger.ForContext<FileSystemScanner>();
    }

    public async Task<ScanResult> ScanAsync(
        string path,
        IProgress<ScanProgress> progress,
        CancellationToken cancellationToken,
        ScanOptions? options = null)
    {
        options ??= ScanOptions.Default;

        var startTime = DateTime.UtcNow;
        var errors = new List<string>();
        var scannedPaths = new HashSet<string>(StringComparer.Ordinal);

        var normalizedPath = NormalizePath(path);

        var rootFolder = await ScanDirectoryAsync(
            normalizedPath, 0, options, scannedPaths, errors, progress, startTime, cancellationToken);

        var duration = DateTime.UtcNow - startTime;

        return new ScanResult(
            rootFolder,
            rootFolder.FileCount,
            rootFolder.TotalSize,
            duration,
            errors.AsReadOnly());
    }

    private async Task<FolderGalaxy> ScanDirectoryAsync(
        string dirPath,
        int depth,
        ScanOptions options,
        HashSet<string> scannedPaths,
        List<string> errors,
        IProgress<ScanProgress> progress,
        DateTime startTime,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (options.MaxDepth >= 0 && depth > options.MaxDepth)
        {
            return new FolderGalaxy
            {
                Name = Path.GetFileName(dirPath),
                FullPath = dirPath,
            };
        }

        var realPath = ResolveRealPath(dirPath);
        if (realPath is not null && !scannedPaths.Add(realPath))
        {
            _logger.Information("Skipping symlink loop: {Path}", dirPath);
            return new FolderGalaxy
            {
                Name = Path.GetFileName(dirPath),
                FullPath = dirPath,
            };
        }

        var folder = new FolderGalaxy
        {
            Name = Path.GetFileName(dirPath) ?? dirPath,
            FullPath = dirPath,
        };

        try
        {
            var entries = await Task.Run(() => Directory.EnumerateFileSystemEntries(dirPath).ToList(), cancellationToken);

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var attr = File.GetAttributes(entry);

                    if (attr.HasFlag(FileAttributes.ReparsePoint) && !options.FollowSymlinks)
                    {
                        continue;
                    }

                    if (attr.HasFlag(FileAttributes.Directory))
                    {
                        var subFolder = await ScanDirectoryAsync(
                            entry, depth + 1, options, scannedPaths, errors, progress, startTime, cancellationToken);

                        folder.Children.Add(subFolder);
                        folder.TotalSize += subFolder.TotalSize;
                        folder.FileCount += subFolder.FileCount;
                        folder.FolderCount += subFolder.FolderCount + 1;
                    }
                    else
                    {
                        var fileInfo = new FileInfo(entry);
                        var size = fileInfo.Exists ? fileInfo.Length : 0;

                        if (options.MinSize.HasValue && size < options.MinSize.Value) continue;
                        if (options.MaxSize.HasValue && size > options.MaxSize.Value) continue;

                        var parentDir = Path.GetDirectoryName(entry) ?? string.Empty;

                        var star = new FileStar
                        {
                            Name = fileInfo.Name,
                            FullPath = fileInfo.FullName,
                            Size = size,
                            Category = FileCategoryExtensions.FromPath(entry),
                            CreatedAt = fileInfo.CreationTimeUtc,
                            ModifiedAt = fileInfo.LastWriteTimeUtc,
                            ParentFolder = parentDir,
                            Depth = depth + 1,
                        };

                        folder.Stars.Add(star);
                        folder.TotalSize += size;
                        folder.FileCount++;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    errors.Add($"Access denied: {entry} ({ex.Message})");
                }
                catch (PathTooLongException ex)
                {
                    errors.Add($"Path too long: {entry} ({ex.Message})");
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            errors.Add($"Access denied: {dirPath} ({ex.Message})");
        }
        catch (DirectoryNotFoundException ex)
        {
            errors.Add($"Directory not found: {dirPath} ({ex.Message})");
        }

        folder.Children = folder.Children.OrderByDescending(c => c.TotalSize).ToList();
        folder.Stars = folder.Stars.OrderByDescending(s => s.Size).ToList();

        if (folder.FileCount % options.ProgressInterval == 0)
        {
            ReportProgress(progress, folder.FullPath, folder.FileCount, folder.FolderCount, folder.TotalSize, startTime);
        }

        return folder;
    }

    private static string NormalizePath(string path)
    {
        if (OperatingSystem.IsWindows() && path.Length > 240 && !path.StartsWith(@"\\?\"))
        {
            return @"\\?\" + path;
        }
        return path;
    }

    private static string? ResolveRealPath(string path)
    {
        try
        {
            var info = new DirectoryInfo(path);
            return info.LinkTarget is not null
                ? info.LinkTarget
                : info.FullName;
        }
        catch
        {
            return null;
        }
    }

    private static void ReportProgress(
        IProgress<ScanProgress> progress,
        string currentPath,
        int filesScanned,
        int foldersScanned,
        long totalSize,
        DateTime startTime)
    {
        var elapsed = DateTime.UtcNow - startTime;
        progress.Report(new ScanProgress(currentPath, filesScanned, foldersScanned, totalSize, elapsed, false));
    }
}
