using System;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public partial class BatchConvertSettingsViewModel : ObservableObject
{
    [ObservableProperty] private bool _useSourceFolder;
    [ObservableProperty] private bool _useOutputFolder;
    [ObservableProperty] private string _outputFolder;
    [ObservableProperty] private bool _overwrite;
    [ObservableProperty] private ObservableCollection<string> _targetEncodings;
    [ObservableProperty] private string? _selectedTargetEncoding;

    [ObservableProperty] private ObservableCollection<string> _ocrEngines;
    [ObservableProperty] private string? _selectedOcrEngine;

    [ObservableProperty] private ObservableCollection<string> _languagePostFixes;
    [ObservableProperty] private string? _selectedLanguagePostFix;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IFolderHelper _folderHelper;

    public BatchConvertSettingsViewModel(IFolderHelper folderHelper)
    {
        var encodings = EncodingHelper.GetEncodings().Select(p => p.DisplayName).ToList();
        TargetEncodings = new ObservableCollection<string>(encodings);

        OcrEngines = new ObservableCollection<string> { "nOcr", "Tesseract" };
        if (OperatingSystem.IsWindows() && !File.Exists(Path.Combine(Se.PaddleOcrFolder, "paddleocr.exe")))
        {
            OcrEngines.Add("PaddleOCR");
        }
        
        SelectedOcrEngine = Se.Settings.Ocr.Engine == "nOcr" ? OcrEngines.First() : OcrEngines.Last();

        LanguagePostFixes = new ObservableCollection<string>()
        {
            Se.Language.General.NoLanguageCode,
            Se.Language.General.TwoLetterLanguageCode,
            Se.Language.General.ThreeLetterLanguageCode,
        };
        SelectedLanguagePostFix = Se.Settings.Tools.BatchConvert.LanguagePostFix;
        if (SelectedLanguagePostFix == null)
        {
            SelectedLanguagePostFix = LanguagePostFixes[1];
        }

        _folderHelper = folderHelper;

        OutputFolder = string.Empty;
        LoadSettings();
    }

    private void LoadSettings()
    {
        UseSourceFolder = Se.Settings.Tools.BatchConvert.SaveInSourceFolder; ;
        UseOutputFolder = !UseSourceFolder;
        OutputFolder = Se.Settings.Tools.BatchConvert.OutputFolder;
        Overwrite = Se.Settings.Tools.BatchConvert.Overwrite;
        SelectedTargetEncoding = Se.Settings.Tools.BatchConvert.TargetEncoding;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.BatchConvert.SaveInSourceFolder = !UseOutputFolder;
        Se.Settings.Tools.BatchConvert.OutputFolder = OutputFolder;
        Se.Settings.Tools.BatchConvert.Overwrite = Overwrite;
        Se.Settings.Tools.BatchConvert.TargetEncoding = SelectedTargetEncoding ?? TargetEncodings.First();

        Se.Settings.Tools.BatchConvert.LanguagePostFix = SelectedLanguagePostFix ?? Se.Language.General.TwoLetterLanguageCode;
        Se.Settings.Ocr.Engine = SelectedOcrEngine ?? "nOcr";

        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (UseOutputFolder && string.IsNullOrWhiteSpace(OutputFolder))
        {
            await MessageBox.Show(Window!, "Error",
                "Please select output folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private async Task BrowseOutputFolder()
    {
        var folder = await _folderHelper.PickFolderAsync(Window!, "Select output folder");
        if (!string.IsNullOrEmpty(folder))
        {
            OutputFolder = folder;
            UseOutputFolder = true;
            UseSourceFolder = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}