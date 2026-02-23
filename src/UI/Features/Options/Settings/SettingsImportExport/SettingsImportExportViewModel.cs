using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.Settings.SettingsImportExport;

public partial class SettingsImportExportViewModel : ObservableObject
{
    public string TitleText { get; set; }
    [ObservableProperty] private bool _exportImportAll;
    [ObservableProperty] private bool _exportImportRecentFiles;
    [ObservableProperty] private bool _exportImportRules;
    [ObservableProperty] private bool _exportImportAppearance;
    [ObservableProperty] private bool _exportImportAutoTranslate;
    [ObservableProperty] private bool _exportImportWaveform;
    [ObservableProperty] private bool _exportImportSyntaxColoring;
    [ObservableProperty] private bool _exportImportShortcuts;
    [ObservableProperty] private string _filePath = string.Empty;

    private bool _isExport;
    public bool OkPressed { get; set; }
    public Window? Window { get; set; }
    private readonly IFileHelper _fileHelper;

    public SettingsImportExportViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
        TitleText = Se.Language.General.ImportDotDotDot;
        ExportImportAll = true;
    }

    public void SetIsExport(bool isExport)
    {
        _isExport = isExport;
        TitleText = isExport ? Se.Language.General.ExportDotDotDot : Se.Language.General.ImportDotDotDot;
    }


    [RelayCommand]
    private async Task BrowseFile()
    {
        if (Window == null)
        {
            return;
        }

        if (_isExport)
        {
            var fileName = await _fileHelper.PickSaveFile(
                Window,
                ".json",
                "Settings.json",
                Se.Language.General.ExportDotDotDot);

            if (!string.IsNullOrEmpty(fileName))
            {
                FilePath = fileName;
            }
        }
        else
        {
            var fileName = await _fileHelper.PickOpenFile(
                Window,
                Se.Language.General.ImportDotDotDot,
                "JSON files",
                ".json");

            if (!string.IsNullOrEmpty(fileName))
            {
                FilePath = fileName;
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

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    private void ExportSettings()
    {
        var exportData = new Se();
        var currentSettings = Se.Settings;

        if (ExportImportAll || ExportImportRules)
        {
            exportData.General = currentSettings.General;
        }

        if (ExportImportAll || ExportImportWaveform)
        {
            exportData.Waveform = currentSettings.Waveform;
        }

        if (ExportImportAll)
        {
            exportData.Tools = currentSettings.Tools;
        }

        if (ExportImportAll || ExportImportAppearance)
        {
            exportData.Appearance = currentSettings.Appearance;
        }

        if (ExportImportAll)
        {
            exportData.Options = currentSettings.Options;
        }

        if (ExportImportAll || ExportImportShortcuts)
        {
            exportData.Shortcuts = currentSettings.Shortcuts;
        }

        if (ExportImportAll || ExportImportAutoTranslate)
        {
            exportData.AutoTranslate = currentSettings.AutoTranslate;
        }

        if (ExportImportAll)
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

        if (ExportImportAll || ExportImportRules)
        {
            if (importData.General != null)
            {
                Se.Settings.General = importData.General;
            }
        }

        if (ExportImportAll || ExportImportWaveform)
        {
            if (importData.Waveform != null)
            {
                Se.Settings.Waveform = importData.Waveform;
            }
        }

        if (ExportImportAll)
        {
            if (importData.Video != null)
            {
                Se.Settings.Video = importData.Video;
            }

            if (importData.Tools != null)
            {
                Se.Settings.Tools = importData.Tools;
            }

            if (importData.Options != null)
            {
                Se.Settings.Options = importData.Options;
            }

            if (importData.SpellCheck != null)
            {
                Se.Settings.SpellCheck = importData.SpellCheck;
            }
        }

        if (ExportImportAll || ExportImportAppearance)
        {
            if (importData.Appearance != null)
            {
                Se.Settings.Appearance = importData.Appearance;
            }
        }

        if (ExportImportAll || ExportImportShortcuts)
        {
            if (importData.Shortcuts != null)
            {
                Se.Settings.Shortcuts = importData.Shortcuts;
            }
        }

        if (ExportImportAll || ExportImportAutoTranslate)
        {
            if (importData.AutoTranslate != null)
            {
                Se.Settings.AutoTranslate = importData.AutoTranslate;
            }
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

