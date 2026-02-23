using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.Settings.SettingsImportExport;

public partial class SettingsImportExportViewModel : ObservableObject
{
    public string TitleText { get; set; }
    [ObservableProperty] private ObservableCollection<SettingsAreaItem> _settingsAreas = new();
    [ObservableProperty] private string _filePath = string.Empty;

    private bool _isExport;
    public bool OkPressed { get; set; }
    public Window? Window { get; set; }

    public SettingsImportExportViewModel()
    {
        TitleText = Se.Language.General.ImportDotDotDot;
        InitializeSettingsAreas();
    }

    public void SetIsExport(bool isExport)
    {
        _isExport = isExport;
        TitleText = isExport ? Se.Language.General.ExportDotDotDot : Se.Language.General.ImportDotDotDot;
    }

    private void InitializeSettingsAreas()
    {
        SettingsAreas = new ObservableCollection<SettingsAreaItem>
        {
            new() { Name = Se.Language.General.Rules, IsSelected = true, Key = "Rules" },
            new() { Name = Se.Language.General.General, IsSelected = true, Key = "General" },
            new() { Name = Se.Language.General.SubtitleFormats, IsSelected = true, Key = "SubtitleFormats" },
            new() { Name = Se.Language.Options.Settings.SyntaxColoring, IsSelected = true, Key = "SyntaxColoring" },
            new() { Name = Se.Language.General.VideoPlayer, IsSelected = true, Key = "VideoPlayer" },
            new() { Name = Se.Language.Options.Settings.WaveformSpectrogram, IsSelected = true, Key = "WaveformSpectrogram" },
            new() { Name = Se.Language.General.Tools, IsSelected = true, Key = "Tools" },
            new() { Name = Se.Language.General.Appearance, IsSelected = true, Key = "Appearance" },
            new() { Name = Se.Language.General.Toolbar, IsSelected = true, Key = "Toolbar" },
            new() { Name = Se.Language.Options.Settings.Network, IsSelected = true, Key = "Network" },
            new() { Name = Se.Language.Options.Settings.FilesAndLogs, IsSelected = false, Key = "FilesAndLogs" },
            new() { Name = "Shortcuts", IsSelected = true, Key = "Shortcuts" },
            new() { Name = "Auto Translate", IsSelected = true, Key = "AutoTranslate" },
            new() { Name = "Spell Check", IsSelected = true, Key = "SpellCheck" },
        };
    }

    [RelayCommand]
    private async Task BrowseFile()
    {
        if (Window == null)
        {
            return;
        }

        var storageProvider = Window.StorageProvider;

        if (_isExport)
        {
            var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = Se.Language.General.ExportDotDotDot,
                DefaultExtension = "json",
                SuggestedFileName = "Settings",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("JSON files")
                    {
                        Patterns = new[] { "*.json" }
                    },
                    new FilePickerFileType("All files")
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
            });

            if (file != null)
            {
                FilePath = file.Path.LocalPath;
            }
        }
        else
        {
            var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = Se.Language.General.ImportDotDotDot,
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("JSON files")
                    {
                        Patterns = new[] { "*.json" }
                    },
                    new FilePickerFileType("All files")
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
            });

            if (files.Count > 0)
            {
                FilePath = files[0].Path.LocalPath;
            }
        }
    }

    [RelayCommand]
    private void Ok()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
        {
            return;
        }

        if (_isExport)
        {
            ExportSettings();
        }
        else
        {
            ImportSettings();
        }

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    private void ExportSettings()
    {
        var exportData = new Se();
        var selectedAreas = SettingsAreas.Where(a => a.IsSelected).Select(a => a.Key).ToList();
        var currentSettings = Se.Settings;

        // For simplicity, we'll export entire sections
        if (selectedAreas.Contains("Rules") || selectedAreas.Contains("General") || selectedAreas.Contains("SubtitleFormats") ||
            selectedAreas.Contains("SyntaxColoring") || selectedAreas.Contains("Toolbar"))
        {
            exportData.General = currentSettings.General;
        }

        if (selectedAreas.Contains("VideoPlayer"))
        {
            exportData.Video = currentSettings.Video;
        }

        if (selectedAreas.Contains("WaveformSpectrogram"))
        {
            exportData.Waveform = currentSettings.Waveform;
        }

        if (selectedAreas.Contains("Tools"))
        {
            exportData.Tools = currentSettings.Tools;
        }

        if (selectedAreas.Contains("Appearance"))
        {
            exportData.Appearance = currentSettings.Appearance;
        }

        if (selectedAreas.Contains("Network"))
        {
            exportData.Options = currentSettings.Options;
        }

        if (selectedAreas.Contains("Shortcuts"))
        {
            exportData.Shortcuts = currentSettings.Shortcuts;
        }

        if (selectedAreas.Contains("AutoTranslate"))
        {
            exportData.AutoTranslate = currentSettings.AutoTranslate;
        }

        if (selectedAreas.Contains("SpellCheck"))
        {
            exportData.SpellCheck = currentSettings.SpellCheck;
        }

        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }

    private void ImportSettings()
    {
        if (!File.Exists(FilePath))
        {
            return;
        }

        var json = File.ReadAllText(FilePath);
        var importData = JsonSerializer.Deserialize<Se>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        });

        if (importData == null)
        {
            return;
        }

        var selectedAreas = SettingsAreas.Where(a => a.IsSelected).Select(a => a.Key).ToList();

        // For simplicity, we'll import entire sections
        if (selectedAreas.Contains("Rules") || selectedAreas.Contains("General") || selectedAreas.Contains("SubtitleFormats") ||
            selectedAreas.Contains("SyntaxColoring") || selectedAreas.Contains("Toolbar"))
        {
            Se.Settings.General = importData.General;
        }

        if (selectedAreas.Contains("VideoPlayer") && importData.Video != null)
        {
            Se.Settings.Video = importData.Video;
        }

        if (selectedAreas.Contains("WaveformSpectrogram") && importData.Waveform != null)
        {
            Se.Settings.Waveform = importData.Waveform;
        }

        if (selectedAreas.Contains("Tools") && importData.Tools != null)
        {
            Se.Settings.Tools = importData.Tools;
        }

        if (selectedAreas.Contains("Appearance") && importData.Appearance != null)
        {
            Se.Settings.Appearance = importData.Appearance;
        }

        if (selectedAreas.Contains("Network") && importData.Options != null)
        {
            Se.Settings.Options = importData.Options;
        }

        if (selectedAreas.Contains("Shortcuts") && importData.Shortcuts != null)
        {
            Se.Settings.Shortcuts = importData.Shortcuts;
        }

        if (selectedAreas.Contains("AutoTranslate") && importData.AutoTranslate != null)
        {
            Se.Settings.AutoTranslate = importData.AutoTranslate;
        }

        if (selectedAreas.Contains("SpellCheck") && importData.SpellCheck != null)
        {
            Se.Settings.SpellCheck = importData.SpellCheck;
        }

        Se.SaveSettings();
    }

    public void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (Window != null)
        {
            UiUtil.RestoreWindowPosition(Window);
        }
    }
}

public partial class SettingsAreaItem : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private string _key = string.Empty;
}
