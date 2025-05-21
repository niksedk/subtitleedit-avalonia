using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.RestoreAutoBackup;

public partial class RestoreAutoBackupViewModel : ObservableObject
{
    [ObservableProperty] private bool _isOkButtonEnabled;
    [ObservableProperty] private DisplayFile? _selectedFile;
    [ObservableProperty] private ObservableCollection<DisplayFile> _files;

    public RestoreAutoBackupWindow? Window { get; set; }
    public string? RestoreFileName { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IAutoBackupService _autoBackupService;
    
    public RestoreAutoBackupViewModel(IAutoBackupService autoBackupService)
    {
        _autoBackupService = autoBackupService;
        _files  = new ObservableCollection<DisplayFile>();
        Initialize();
    }

    private void Initialize()
    {
        foreach (var fileName in _autoBackupService.GetAutoBackupFiles())
        {
            var fileInfo = new FileInfo(fileName);

            var path = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            var displayDate = path[..19].Replace('_', ' ');
            displayDate = displayDate.Remove(13, 1).Insert(13, ":");
            displayDate = displayDate.Remove(16, 1).Insert(16, ":");

            Files.Add(new DisplayFile(fileName, displayDate, Utilities.FormatBytesToDisplayFileSize(fileInfo.Length)));
        }

        Files = new ObservableCollection<DisplayFile>(Files.OrderByDescending(f => f.DateAndTime));
        if (Files.Count > 0)
        {
            SelectedFile = Files[0];
        }
    }

    [RelayCommand]
    private async Task RestoreFile()
    {
        if (SelectedFile is not { } file || Window == null)
        {
            return;
        }
        var answer = await MessageBox.Show(
            Window,
            "Restore auto-backup file?",
            $"Do you want to restore \"{file.FileName}\" from {file.DateAndTime}?",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }
        
        OkPressed = true;
        RestoreFileName = file.FullPath;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }
    
    [RelayCommand]
    private async Task OpenFolder()
    {
        var folder = Se.AutoBackupFolder;
        if (!Directory.Exists(folder))
        {
            try
            {
                Directory.CreateDirectory(folder);
            }
            catch 
            {
                return;
            }
        }
        
        var dirInfo = new DirectoryInfo(folder);
        await Window!.Launcher.LaunchDirectoryInfoAsync(dirInfo);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    public void DataGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        IsOkButtonEnabled = e.AddedItems.Count > 0;
    }
}