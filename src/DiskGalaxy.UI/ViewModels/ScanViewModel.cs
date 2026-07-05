using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiskGalaxy.Core.Models;
using DiskGalaxy.Core.Scanning;

namespace DiskGalaxy.UI.ViewModels;

public sealed partial class ScanViewModel : ViewModelBase
{
    private readonly IFileSystemScanner _scanner;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private string _selectedPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private string _currentPath = string.Empty;

    [ObservableProperty]
    private int _filesScanned;

    [ObservableProperty]
    private int _foldersScanned;

    [ObservableProperty]
    private long _totalSize;

    [ObservableProperty]
    private double _progressValue;

    [ObservableProperty]
    private bool _isIndeterminate;

    [ObservableProperty]
    private ScanResult? _lastResult;

    public ObservableCollection<string> Errors { get; } = [];

    public Action<ScanResult>? OnScanCompleted { get; set; }

    public ScanViewModel(IFileSystemScanner scanner)
    {
        _scanner = scanner;
    }

    [RelayCommand]
    private async Task StartScanAsync()
    {
        if (IsScanning) return;

        _cts = new CancellationTokenSource();
        IsScanning = true;
        Errors.Clear();
        LastResult = null;
        FilesScanned = 0;
        FoldersScanned = 0;
        TotalSize = 0;
        ProgressValue = 0;
        IsIndeterminate = true;

        try
        {
            var progress = new Progress<ScanProgress>(OnScanProgress);

            var result = await Task.Run(
                () => _scanner.ScanAsync(SelectedPath, progress, _cts.Token),
                _cts.Token);

            LastResult = result;
            ProgressValue = 100;

            foreach (var error in result.Errors)
            {
                Errors.Add(error);
            }

            OnScanCompleted?.Invoke(result);
        }
        catch (OperationCanceledException)
        {
            CurrentPath = "Scan cancelled.";
        }
        catch (Exception ex)
        {
            Errors.Add($"Scan failed: {ex.Message}");
        }
        finally
        {
            IsScanning = false;
            IsIndeterminate = false;
        }
    }

    [RelayCommand]
    private void CancelScan()
    {
        _cts?.Cancel();
    }

    private void OnScanProgress(ScanProgress progress)
    {
        CurrentPath = progress.CurrentPath;
        FilesScanned = progress.FilesScanned;
        FoldersScanned = progress.FoldersScanned;
        TotalSize = progress.TotalSize;
        IsIndeterminate = true;
    }
}
