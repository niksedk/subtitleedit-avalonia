using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Core.VobSub.Ocr.Service;
using Nikse.SubtitleEdit.Features.Edit.GoToLineNumber;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Ocr.Download;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.Ocr.NOcr;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.ShowImage;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.Ocr;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr;

public partial class OcrViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<OcrEngineItem> _ocrEngines;
    [ObservableProperty] private OcrEngineItem? _selectedOcrEngine;
    [ObservableProperty] private ObservableCollection<OcrSubtitleItem> _ocrSubtitleItems;
    [ObservableProperty] private OcrSubtitleItem? _selectedOcrSubtitleItem;
    [ObservableProperty] private ObservableCollection<string> _nOcrDatabases;
    [ObservableProperty] private string? _selectedNOcrDatabase;
    [ObservableProperty] private ObservableCollection<int> _nOcrMaxWrongPixelsList;
    [ObservableProperty] private int _selectedNOcrMaxWrongPixels;
    [ObservableProperty] private ObservableCollection<int> _nOcrPixelsAreSpaceList;
    [ObservableProperty] private int _selectedNOcrPixelsAreSpace;
    [ObservableProperty] private ObservableCollection<string> _ollamaLanguages;
    [ObservableProperty] private string? _selectedOllamaLanguage;
    [ObservableProperty] private ObservableCollection<TesseractDictionary> _tesseractDictionaryItems;
    [ObservableProperty] private TesseractDictionary? _selectedTesseractDictionaryItem;
    [ObservableProperty] private string _ollamaModel;
    [ObservableProperty] private string _ollamaUrl;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private Bitmap? _currentImageSource;
    [ObservableProperty] private string _currentBitmapInfo;
    [ObservableProperty] private string _currentText;
    [ObservableProperty] private bool _isOcrRunning;
    [ObservableProperty] private bool _isNOcrVisible;
    [ObservableProperty] private bool _isOllamaVisible;
    [ObservableProperty] private bool _isTesseractVisible;
    [ObservableProperty] private bool _isPaddleOcrVisible;
    [ObservableProperty] private bool _isGoogleVisionVisible;
    [ObservableProperty] private bool _isMistralOcrVisible;
    [ObservableProperty] private bool _nOcrDrawUnknownText;
    [ObservableProperty] private bool _isInspectLineVisible;
    [ObservableProperty] private bool _isInspectAdditionsVisible;
    [ObservableProperty] private string _googleVisionApiKey;
    [ObservableProperty] private string _mistralApiKey;
    [ObservableProperty] private ObservableCollection<OcrLanguage> _googleVisionLanguages;
    [ObservableProperty] private OcrLanguage? _selectedGoogleVisionLanguage;
    [ObservableProperty] private ObservableCollection<OcrLanguage2> _paddleOcrLanguages;
    [ObservableProperty] private OcrLanguage2? _selectedPaddleOcrLanguage;
    [ObservableProperty] private bool _paddleUseGpu;
    [ObservableProperty] private bool _showContextMenu;
    [ObservableProperty] private ObservableCollection<SpellCheckDictionaryDisplay> _dictionaries;
    [ObservableProperty] private SpellCheckDictionaryDisplay? _selectedDictionary;
    [ObservableProperty] private bool _doFixOcrErrors;
    [ObservableProperty] private bool _doPromptForUnknownWords;
    [ObservableProperty] private bool _doTryToGuessUnknownWords;
    [ObservableProperty] private bool _doAutoBreak;
    [ObservableProperty] private bool _isDictionaryLoaded;
    [ObservableProperty] private ObservableCollection<UnknownWordItem> _unknownWords;
    [ObservableProperty] private UnknownWordItem? _selectedUnknownWord;
    [ObservableProperty] private bool _isUnknownWordSelected;
    [ObservableProperty] private ObservableCollection<ReplacementUsedItem> _allFixes;
    [ObservableProperty] private ReplacementUsedItem? _selectedAllFix;
    [ObservableProperty] private ObservableCollection<string> _allGuesses;
    [ObservableProperty] private string? _selectedAllGuess;

    public Window? Window { get; set; }
    public DataGrid SubtitleGrid { get; set; }
    public MatroskaTrackInfo? SelectedMatroskaTrack { get; set; }
    public bool OkPressed { get; private set; }

    public readonly List<SubtitleLineViewModel> OcredSubtitle;

    private IOcrSubtitle? _ocrSubtitle;
    private readonly INOcrCaseFixer _nOcrCaseFixer;
    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;
    private readonly ISpellCheckManager _spellCheckManager;
    private readonly IOcrFixEngine2 _ocrFixEngine;

    private CancellationTokenSource _cancellationTokenSource;
    private NOcrDb? _nOcrDb;
    private readonly List<SkipOnceChar> _runOnceChars;
    private readonly List<SkipOnceChar> _skipOnceChars;
    private readonly NOcrAddHistoryManager _nOcrAddHistoryManager;

    public OcrViewModel(
        INOcrCaseFixer nOcrCaseFixer,
        IWindowService windowService,
        IFileHelper fileHelper,
        ISpellCheckManager spellCheckManager,
        IOcrFixEngine2 ocrFixEngine)
    {
        _nOcrCaseFixer = nOcrCaseFixer;
        _windowService = windowService;
        _fileHelper = fileHelper;
        _spellCheckManager = spellCheckManager;
        _ocrFixEngine = ocrFixEngine;

        Title = Se.Language.Ocr.Ocr;
        OcrEngines = new ObservableCollection<OcrEngineItem>(OcrEngineItem.GetOcrEngines());
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>();
        NOcrDatabases = new ObservableCollection<string>();
        NOcrMaxWrongPixelsList = new ObservableCollection<int>(Enumerable.Range(1, 500));
        NOcrPixelsAreSpaceList = new ObservableCollection<int>(Enumerable.Range(1, 50));
        OllamaLanguages = new ObservableCollection<string>(Iso639Dash2LanguageCode.List
            .Select(p => p.EnglishName)
            .OrderBy(p => p));
        SelectedOllamaLanguage = "English";
        SubtitleGrid = new DataGrid();
        CurrentBitmapInfo = string.Empty;
        CurrentText = string.Empty;
        ProgressText = string.Empty;
        OllamaModel = string.Empty;
        OllamaUrl = string.Empty;
        TesseractDictionaryItems = new ObservableCollection<TesseractDictionary>();
        GoogleVisionApiKey = string.Empty;
        MistralApiKey = string.Empty;
        GoogleVisionLanguages = new ObservableCollection<OcrLanguage>(GoogleVisionOcr.GetLanguages().OrderBy(p => p.ToString()));
        PaddleOcrLanguages = new ObservableCollection<OcrLanguage2>(PaddleOcr.GetLanguages().OrderBy(p => p.ToString()));
        OcredSubtitle = new List<SubtitleLineViewModel>();
        Dictionaries = new ObservableCollection<SpellCheck.SpellCheckDictionaryDisplay>();
        UnknownWords = new ObservableCollection<UnknownWordItem>();
        AllFixes = new ObservableCollection<ReplacementUsedItem>();
        AllGuesses = new ObservableCollection<string>();
        _runOnceChars = new List<SkipOnceChar>();
        _skipOnceChars = new List<SkipOnceChar>();
        _nOcrAddHistoryManager = new NOcrAddHistoryManager();
        _cancellationTokenSource = new CancellationTokenSource();
        LoadSettings();
        EngineSelectionChanged();
        LoadDictionaries();
    }

    private void LoadSettings()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var ocr = Se.Settings.Ocr;
            if (!string.IsNullOrEmpty(ocr.Engine) && OcrEngines.Any(p => p.Name == ocr.Engine))
            {
                SelectedOcrEngine = OcrEngines.First(p => p.Name == ocr.Engine);
            }

            if (!string.IsNullOrEmpty(ocr.NOcrDatabase) && NOcrDatabases.Contains(ocr.NOcrDatabase))
            {
                SelectedNOcrDatabase = ocr.NOcrDatabase;
            }

            SelectedNOcrMaxWrongPixels = ocr.NOcrMaxWrongPixels;
            NOcrDrawUnknownText = ocr.NOcrDrawUnknownText;
            SelectedNOcrPixelsAreSpace = ocr.NOcrPixelsAreSpace;
            OllamaModel = ocr.OllamaModel;
            OllamaUrl = ocr.OllamaUrl;
            SelectedOllamaLanguage = ocr.OllamaLanguage;
            GoogleVisionApiKey = ocr.GoogleVisionApiKey;
            MistralApiKey = ocr.MistralApiKey;
            SelectedGoogleVisionLanguage = GoogleVisionLanguages.FirstOrDefault(p => p.Code == ocr.GoogleVisionLanguage);
            SelectedPaddleOcrLanguage = PaddleOcrLanguages.FirstOrDefault(p => p.Code == Se.Settings.Ocr.PaddleOcrLastLanguage) ?? PaddleOcrLanguages.First();
            DoFixOcrErrors = ocr.DoFixOcrErrors;
            DoPromptForUnknownWords = ocr.DoPromptForUnknownWords;
            DoTryToGuessUnknownWords = ocr.DoTryToGuessUnknownWords;
            DoAutoBreak = ocr.DoAutoBreak;
        });
    }

    private void SaveSettings()
    {
        var ocr = Se.Settings.Ocr;
        ocr.Engine = SelectedOcrEngine?.Name ?? "nOCR";
        ocr.NOcrDatabase = SelectedNOcrDatabase ?? "Latin";
        ocr.NOcrMaxWrongPixels = SelectedNOcrMaxWrongPixels;
        ocr.NOcrDrawUnknownText = NOcrDrawUnknownText;
        ocr.NOcrPixelsAreSpace = SelectedNOcrPixelsAreSpace;
        ocr.OllamaModel = OllamaModel;
        ocr.OllamaUrl = OllamaUrl;
        ocr.OllamaLanguage = SelectedOllamaLanguage ?? "English";
        ocr.GoogleVisionApiKey = GoogleVisionApiKey;
        ocr.MistralApiKey = MistralApiKey;
        ocr.GoogleVisionLanguage = SelectedGoogleVisionLanguage?.Code ?? "en";
        ocr.DoFixOcrErrors = DoFixOcrErrors;
        ocr.DoPromptForUnknownWords = DoPromptForUnknownWords;
        ocr.DoTryToGuessUnknownWords = DoTryToGuessUnknownWords;
        ocr.DoAutoBreak = DoAutoBreak;
        
        if (SelectedDictionary != null)
        {
            Se.Settings.Ocr.LastLanguageDictionaryFile = SelectedDictionary.DictionaryFileName;
        }

        Se.SaveSettings();
    }
    
    private void LoadDictionaries()
    {
        var spellCheckLanguages = _spellCheckManager.GetDictionaryLanguages(Se.DictionariesFolder);
        Dictionaries.Clear();
        Dictionaries.Add(new SpellCheckDictionaryDisplay
        {
            Name = GetDictionaryNameNone(),
            DictionaryFileName = string.Empty,
        });
        Dictionaries.AddRange(spellCheckLanguages);
        if (Dictionaries.Count > 0)
        {
            if (!string.IsNullOrEmpty(Se.Settings.Ocr.LastLanguageDictionaryFile))
            {
                SelectedDictionary = Dictionaries.FirstOrDefault(l => l.DictionaryFileName == Se.Settings.Ocr.LastLanguageDictionaryFile);
            }
            
            if (SelectedDictionary == null && !string.IsNullOrEmpty(Se.Settings.SpellCheck.LastLanguageDictionaryFile))
            {
                SelectedDictionary = Dictionaries.FirstOrDefault(l => l.DictionaryFileName == Se.Settings.SpellCheck.LastLanguageDictionaryFile);
            }

            if (SelectedDictionary == null)
            {
                SelectedDictionary = Dictionaries.FirstOrDefault(l => l.Name.Contains("English", StringComparison.OrdinalIgnoreCase));
            }

            if (SelectedDictionary == null)
            {
                SelectedDictionary = Dictionaries[0];
            }

            _spellCheckManager.Initialize(SelectedDictionary.DictionaryFileName, SpellCheckDictionaryDisplay.GetTwoLetterLanguageCode(SelectedDictionary));
        }
    }

    private static string GetDictionaryNameNone()
    {
        return "[" + Se.Language.General.None + "]";
    }

    private string? GetNOcrLanguageFileName()
    {
        if (SelectedNOcrDatabase == null)
        {
            return null;
        }

        return Path.Combine(Se.OcrFolder, $"{SelectedNOcrDatabase}.nocr");
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    [RelayCommand]
    private async Task PickDictionary()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<GetDictionariesWindow, GetDictionariesViewModel>(Window!);
        if (result.OkPressed && result.SelectedDictionary != null)
        {
            LoadDictionaries();
            SelectedDictionary = Dictionaries
                .FirstOrDefault(d =>
                    d.Name.Contains(result.SelectedDictionary.EnglishName, StringComparison.OrdinalIgnoreCase) ||
                    d.Name.Contains(result.SelectedDictionary.NativeName, StringComparison.OrdinalIgnoreCase));

            if (SelectedDictionary == null)
            {
                SelectedDictionary = Dictionaries.FirstOrDefault();
            }
        }
    }

    [RelayCommand]
    private void PauseOcr()
    {
        IsOcrRunning = false;
        _cancellationTokenSource.Cancel();
    }

    [RelayCommand]
    private void AddUnknownWordToNames()
    {
    }

    [RelayCommand]
    private void AddUnknownWordToUserDictionary()
    {
    }


    [RelayCommand]
    private void AddUnknownWordToOcrPair()
    {
    }

    [RelayCommand]
    private async Task GoogleUnknowWord()
    {
        var selectedWord = SelectedUnknownWord;
        if (selectedWord == null)
        {
            return;
        }

        await Window!.Launcher.LaunchUriAsync(new Uri("https://www.google.com/search?q=" + Utilities.UrlEncode(selectedWord.ToString())));
    }


    [RelayCommand]
    private void Export()
    {
    }

    [RelayCommand]
    private async Task InspectLine()
    {
        var item = SelectedOcrSubtitleItem;
        if (item == null)
        {
            return;
        }

        if (!InitNOcrDb())
        {
            return;
        }

        var bitmap = item.GetSkBitmap();
        var nBmp = new NikseBitmap2(bitmap);
        nBmp.MakeTwoColor(200);
        nBmp.CropTop(0, new SKColor(0, 0, 0, 0));
        var letters =
            NikseBitmapImageSplitter2.SplitBitmapToLettersNew(nBmp, SelectedNOcrPixelsAreSpace, false, true, 20, true);
        var matches = new List<NOcrChar?>();
        foreach (var splitterItem in letters)
        {
            if (splitterItem.NikseBitmap == null)
            {
                var match = new NOcrChar { Text = splitterItem.SpecialCharacter ?? string.Empty };
                matches.Add(match);
            }
            else
            {
                var match = _nOcrDb!.GetMatch(nBmp, letters, splitterItem, splitterItem.Top, true,
                    SelectedNOcrMaxWrongPixels);
                matches.Add(match);
            }
        }

        var result = await _windowService.ShowDialogAsync<NOcrInspectWindow, NOcrInspectViewModel>(Window!,
            vm =>
            {
                vm.Initialize(nBmp.GetBitmap(), SelectedOcrSubtitleItem, _nOcrDb, SelectedNOcrMaxWrongPixels, letters,
                    matches);
            });

        if (result.AddBetterMatchPressed)
        {
            var characterAddResult =
                await _windowService.ShowDialogAsync<NOcrCharacterAddWindow, NOcrCharacterAddViewModel>(Window!,
                    vm =>
                    {
                        vm.Initialize(nBmp, item, letters, result.LetterIndex, _nOcrDb!, SelectedNOcrMaxWrongPixels,
                            _nOcrAddHistoryManager, false, false);
                    });

            if (characterAddResult.OkPressed)
            {
                var letterBitmap = letters[result.LetterIndex].NikseBitmap;
                _nOcrAddHistoryManager.Add(characterAddResult.NOcrChar, letterBitmap, OcrSubtitleItems.IndexOf(item));
                _nOcrDb!.Add(characterAddResult.NOcrChar);
                _ = Task.Run(_nOcrDb.Save);
            }
            else if (characterAddResult.InspectHistoryPressed)
            {
                await _windowService.ShowDialogAsync<NOcrCharacterHistoryWindow, NOcrCharacterHistoryViewModel>(Window!,
                    vm => { vm.Initialize(_nOcrDb!, _nOcrAddHistoryManager); });
            }
        }
    }

    [RelayCommand]
    private async Task InspectAdditions()
    {
        await _windowService.ShowDialogAsync<NOcrCharacterHistoryWindow, NOcrCharacterHistoryViewModel>(Window!,
            vm => { vm.Initialize(_nOcrDb!, _nOcrAddHistoryManager); });
    }

    [RelayCommand]
    private async Task ViewSelectedImage()
    {
        var item = SelectedOcrSubtitleItem;
        if (item == null)
        {
            return;
        }

        await _windowService.ShowDialogAsync<ShowImageWindow, ShowImageViewModel>(Window!, vm => { vm.Initialize("OCR image", item.GetBitmap()); });
    }

    [RelayCommand]
    private async Task SaveImageAs()
    {
        var item = SelectedOcrSubtitleItem;
        if (item == null)
        {
            return;
        }

        var imageIndex = OcrSubtitleItems.IndexOf(item) + 1;
        var fileName = await _fileHelper.PickSaveSubtitleFile(Window!, ".png", $"image{imageIndex}", Se.Language.General.SaveImageAs);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var bitmap = item.GetBitmap();
        bitmap.Save(fileName, 100);
    }

    [RelayCommand]
    private async Task PickOllamaModel()
    {
        var result = await _windowService.ShowDialogAsync<PickOllamaModelWindow, PickOllamaModelViewModel>(Window!,
            vm => { vm.Initialize(Se.Language.General.PickOllamaModel, OllamaModel, OllamaUrl); });

        if (result.OkPressed && result.SelectedModel != null)
        {
            OllamaModel = result.SelectedModel;
        }
    }

    [RelayCommand]
    private async Task ShowNOcrSettings()
    {
        InitNOcrDb();
        var result =
            await _windowService.ShowDialogAsync<NOcrSettingsWindow, NOcrSettingsViewModel>(Window!,
                vm => { vm.Initialize(_nOcrDb!); });

        if (result.EditPressed)
        {
            await _windowService.ShowDialogAsync<NOcrDbEditWindow, NOcrDbEditViewModel>(Window!,
                vm => { vm.Initialize(_nOcrDb!); });

            return;
        }

        if (result.DeletePressed)
        {
            try
            {
                File.Delete(_nOcrDb!.FileName);
                NOcrDatabases.Remove(SelectedNOcrDatabase!);
                SelectedNOcrDatabase = NOcrDatabases.FirstOrDefault();

                if (SelectedNOcrDatabase == null)
                {
                    _nOcrDb = new NOcrDb(Path.Combine(Se.OcrFolder, "Default.nocr"));
                    _nOcrDb.Save();
                    NOcrDatabases.Add("Default");
                    SelectedNOcrDatabase = NOcrDatabases.FirstOrDefault();
                }
            }
            catch
            {
                await MessageBox.Show(
                    Window!,
                    "Error deleting file",
                    $"Could not delete the file {_nOcrDb!.FileName}.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return;
        }

        if (result.NewPressed)
        {
            var newResult = await _windowService.ShowDialogAsync<NOcrDbNewWindow, NOcrDbNewViewModel>(Window!,
                vm => { vm.Initialize(Se.Language.Ocr.NewNOcrDatabase, string.Empty); });
            if (newResult.OkPressed)
            {
                if (!Directory.Exists(Se.OcrFolder))
                {
                    Directory.CreateDirectory(Se.OcrFolder);
                }

                var newFileName = Path.Combine(Se.OcrFolder, newResult.DatabaseName + ".nocr");
                if (File.Exists(newFileName))
                {
                    await MessageBox.Show(
                        Window!,
                        Se.Language.General.FileAlreadyExists,
                        string.Format(Se.Language.General.FileXAlreadyExists, newFileName),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                _nOcrDb = new NOcrDb(newFileName);
                _nOcrDb.Save();
                NOcrDatabases.Add(newResult.DatabaseName);
                var sortedList = NOcrDatabases.OrderBy(p => p).ToList();
                NOcrDatabases.Clear();
                NOcrDatabases.AddRange(sortedList);
                SelectedNOcrDatabase = newResult.DatabaseName;
            }

            return;
        }

        if (result.RenamePressed)
        {
            var newResult = await _windowService.ShowDialogAsync<NOcrDbNewWindow, NOcrDbNewViewModel>(Window!,
                vm =>
                {
                    vm.Initialize(Se.Language.Ocr.RenameNOcrDatabase,
                        Path.GetFileNameWithoutExtension(_nOcrDb!.FileName));
                });
            if (newResult.OkPressed)
            {
                if (!Directory.Exists(Se.OcrFolder))
                {
                    Directory.CreateDirectory(Se.OcrFolder);
                }

                var newFileName = Path.Combine(Se.OcrFolder, newResult.DatabaseName + ".nocr");
                if (File.Exists(newFileName))
                {
                    await MessageBox.Show(
                        Window!,
                        Se.Language.General.FileAlreadyExists,
                        string.Format(Se.Language.General.FileXAlreadyExists, newFileName),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                File.Move(_nOcrDb!.FileName, newFileName);
                NOcrDatabases.Clear();
                foreach (var s in NOcrDb.GetDatabases().OrderBy(p => p))
                {
                    NOcrDatabases.Add(s);
                }

                SelectedNOcrDatabase = newResult.DatabaseName;
            }
        }
    }

    [RelayCommand]
    private async Task PickTesseractModel()
    {
        await TesseractModelDownload();
    }

    [RelayCommand]
    private void ToggleItalic()
    {
        var selectedItems = SubtitleGrid.SelectedItems;
        if (selectedItems == null || selectedItems.Count == 0)
        {
            return;
        }

        var first = true;
        var makeItalic = true;
        foreach (var item in selectedItems)
        {
            if (item is OcrSubtitleItem ocrItem)
            {
                if (first)
                {
                    first = false;
                    makeItalic = !ocrItem.Text.Contains("<i>");
                }

                ocrItem.Text = ocrItem.Text.Replace("<i>", string.Empty).Replace("</i>", string.Empty);
                ocrItem.Text = ocrItem.Text.Replace("<I>", string.Empty).Replace("</I>", string.Empty);
                if (makeItalic)
                {
                    if (!string.IsNullOrEmpty(ocrItem.Text))
                    {
                        ocrItem.Text = $"<i>{ocrItem.Text}</i>";
                    }
                }

                var idx = OcrSubtitleItems.IndexOf(ocrItem);
                if (_ocrFixEngine.IsLoaded())
                {
                    ocrItem.FixResult = new OcrFixLineResult
                    {
                        LineIndex = idx,

                        //TODO: spell check
                        Words = new List<OcrFixLinePartResult> { new() { Word = ocrItem.Text, IsSpellCheckedOk = null } },
                    };
                }
                else
                {
                    ocrItem.FixResult = new OcrFixLineResult(idx, ocrItem.Text);
                }
            }
        }
    }

    [RelayCommand]
    private void ToggleBold()
    {
        var selectedItems = SubtitleGrid.SelectedItems;
        if (selectedItems == null || selectedItems.Count == 0)
        {
            return;
        }

        var first = true;
        var makeBold = true;
        foreach (var item in selectedItems)
        {
            if (item is OcrSubtitleItem ocrItem)
            {
                if (first)
                {
                    first = false;
                    makeBold = !ocrItem.Text.Contains("<b>");
                }

                ocrItem.Text = ocrItem.Text.Replace("<b>", string.Empty).Replace("</b>", string.Empty);
                ocrItem.Text = ocrItem.Text.Replace("<B>", string.Empty).Replace("</B>", string.Empty);
                if (makeBold)
                {
                    if (!string.IsNullOrEmpty(ocrItem.Text))
                    {
                        ocrItem.Text = $"<b>{ocrItem.Text}</b>";
                    }
                }

                var idx = OcrSubtitleItems.IndexOf(ocrItem);
                if (_ocrFixEngine.IsLoaded())
                {
                    ocrItem.FixResult = new OcrFixLineResult
                    {
                        LineIndex = idx,
                        // TODO: spell check
                        Words = new List<OcrFixLinePartResult> { new() { Word = ocrItem.Text, IsSpellCheckedOk = null } },
                    };
                }
                else
                {
                    ocrItem.FixResult = new OcrFixLineResult(idx, ocrItem.Text);
                }
            }
        }
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;

        OcredSubtitle.Clear();
        for (var i = 0; i < OcrSubtitleItems.Count; i++)
        {
            var item = OcrSubtitleItems[i];
            var subtitleLine = new SubtitleLineViewModel
            {
                Number = i + 1,
                Text = item.Text,
                StartTime = item.StartTime,
                EndTime = item.EndTime,
            };
            OcredSubtitle.Add(subtitleLine);
        }

        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        _cancellationTokenSource.Cancel();
        Close();
    }

    [RelayCommand]
    private void DeleteSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems;
        if (selectedItems == null || selectedItems.Count == 0)
        {
            return;
        }

        var selectedIndices = new List<int>();
        foreach (var selectedItem in selectedItems)
        {
            if (selectedItem is OcrSubtitleItem item)
            {
                var idx = OcrSubtitleItems.IndexOf(item);
                if (idx >= 0)
                {
                    selectedIndices.Add(idx);

                    var remov = UnknownWords.Where(uw => uw.Item == item).ToList();
                    foreach (var unknownWord in remov)
                    {
                        UnknownWords.Remove(unknownWord);
                    }
                }
            }
        }

        foreach (var index in selectedIndices.OrderByDescending(p => p))
        {
            OcrSubtitleItems.RemoveAt(index);
            _ocrSubtitle?.Delete(index);
        }

        Renumber();

        var toRemove = new List<UnknownWordItem>();
        foreach (var unknownWord in UnknownWords)
        {
            if (!OcrSubtitleItems.Contains(unknownWord.Item))
            {
                toRemove.Add(unknownWord);
            }
        }
        foreach (var item in toRemove)
        {
            UnknownWords.Remove(item);
        }
    }

    private void Renumber()
    {
        for (var i = 0; i < OcrSubtitleItems.Count; i++)
        {
            OcrSubtitleItems[i].Number = i + 1;
        }
    }

    [RelayCommand]
    private async Task StartOcrSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems;
        if (selectedItems == null || selectedItems.Count == 0)
        {
            return;
        }

        var selectedIndices = new List<int>();
        foreach (var selectedItem in selectedItems)
        {
            if (selectedItem is OcrSubtitleItem item)
            {
                var index = OcrSubtitleItems.IndexOf(item);
                if (index >= 0 && !selectedIndices.Contains(index))
                {
                    selectedIndices.Add(index);
                }
            }
        }

        await StartOcr(selectedIndices);
    }

    [RelayCommand]
    private async Task StartOcr(List<int>? selectedIndices)
    {
        if (IsOcrRunning)
        {
            return;
        }

        if (!(SelectedOcrEngine is { } ocrEngine))
        {
            return;
        }

        if (ocrEngine.EngineType == OcrEngineType.Tesseract)
        {
            var tesseractOk = await CheckAndDownloadTesseract();
            if (!tesseractOk)
            {
                return;
            }

            if (SelectedTesseractDictionaryItem == null)
            {
                var tesseractModelOk = await TesseractModelDownload();
                if (!tesseractModelOk)
                {
                    return;
                }
            }
        }

        if (SelectedDictionary != null && DoFixOcrErrors && SelectedDictionary.Name != GetDictionaryNameNone())
        {
            var threeLetterCode = SelectedDictionary.GetThreeLetterCode();
            _ocrFixEngine.Initialize(OcrSubtitleItems.ToList(), threeLetterCode, SelectedDictionary);
        }
        else
        {
            _ocrFixEngine.Unload();
        }

        SaveSettings();
        _cancellationTokenSource = new CancellationTokenSource();
        IsOcrRunning = true;

        var startFromIndex = SelectedOcrSubtitleItem == null ? 0 : OcrSubtitleItems.IndexOf(SelectedOcrSubtitleItem);
        if (selectedIndices == null)
        {
            selectedIndices = new List<int>();
            for (var i = startFromIndex; i < OcrSubtitleItems.Count; i++)
            {
                selectedIndices.Add(i);
            }
        }

        ProgressText = Se.Language.Ocr.RunningOcrDotDotDot;
        ProgressValue = 0d;

        if (ocrEngine.EngineType == OcrEngineType.nOcr)
        {
            RunNOcr(selectedIndices);
        }
        else if (ocrEngine.EngineType == OcrEngineType.Tesseract)
        {
            RunTesseractOcr(selectedIndices);
        }
        else if (ocrEngine.EngineType == OcrEngineType.PaddleOcrStandalone)
        {
            if (Configuration.IsRunningOnWindows && !File.Exists(Path.Combine(Se.PaddleOcrFolder, "paddleocr.exe")))
            {
                var answer = await MessageBox.Show(
                    Window!,
                    "Download Paddle OCR?",
                    $"{Environment.NewLine}\"Paddle OCR\" requires downloading Paddle OCR.{Environment.NewLine}{Environment.NewLine}Download and use Paddle OCR?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    PauseOcr();
                    return;
                }

                var result = await _windowService.ShowDialogAsync<DownloadPaddleOcrWindow, DownloadPaddleOcrViewModel>(Window!,
                    vm => { vm.Initialize(PaddleOcrDownloadType.EngineCpu); });
                if (!result.OkPressed)
                {
                    PauseOcr();
                    return;
                }
            }

            var modelsDirectory = Se.PaddleOcrModelsFolder;
            if (!Directory.Exists(modelsDirectory))
            {
                var result = await _windowService.ShowDialogAsync<DownloadPaddleOcrWindow, DownloadPaddleOcrViewModel>(Window!,
                    vm => { vm.Initialize(PaddleOcrDownloadType.Models); });
                if (!result.OkPressed)
                {
                    PauseOcr();
                    return;
                }
            }

            RunPaddleOcr(selectedIndices, ocrEngine.EngineType);
        }
        else if (ocrEngine.EngineType == OcrEngineType.PaddleOcrPython)
        {
            var modelsDirectory = Se.PaddleOcrModelsFolder;
            if (!Directory.Exists(modelsDirectory))
            {
                var result = await _windowService.ShowDialogAsync<DownloadPaddleOcrWindow, DownloadPaddleOcrViewModel>(Window!,
                    vm => { vm.Initialize(PaddleOcrDownloadType.Models); });
                if (!result.OkPressed)
                {
                    PauseOcr();
                    return;
                }
            }

            RunPaddleOcr(selectedIndices, ocrEngine.EngineType);
        }
        else if (ocrEngine.EngineType == OcrEngineType.Ollama)
        {
            RunOllamaOcr(selectedIndices);
        }
        else if (ocrEngine.EngineType == OcrEngineType.Mistral)
        {
            if (string.IsNullOrEmpty(MistralApiKey))
            {
                await MessageBox.Show(
                    Window!,
                    "Mistral API key missing",
                    $"You must enter a valid Mistral API key.{Environment.NewLine}{Environment.NewLine}Get your API key from https://mistral.ai/",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                IsOcrRunning = false;
                return;
            }

            RunMistralOcr(selectedIndices);
        }
        else if (ocrEngine.EngineType == OcrEngineType.GoogleVision)
        {
            //   RunGoogleVisionOcr(startFromIndex);
        }
    }

    private Lock BatchLock = new Lock();

    private void RunPaddleOcr(List<int> selectedIndices, OcrEngineType engineType)
    {
        var numberOfImages = selectedIndices.Count;
        var ocrEngine = new PaddleOcr();
        var language = SelectedPaddleOcrLanguage?.Code ?? "en";
        var mode = Se.Settings.Ocr.PaddleOcrMode;
        Se.Settings.Ocr.PaddleOcrLastLanguage = language;

        var batchImages = new List<PaddleOcrBatchInput>(numberOfImages);
        var count = 0;
        ProgressText = $"Initializing Paddle OCR...";
        foreach (var i in selectedIndices)
        {
            count++;
            var ocrItem = OcrSubtitleItems[i];
            batchImages.Add(new PaddleOcrBatchInput
            {
                Bitmap = ocrItem.GetSkBitmap(),
                Index = i,
                Text = $"{count} / {numberOfImages}: {ocrItem.StartTime} - {ocrItem.EndTime}"
            });

            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                IsOcrRunning = false;
                return;
            }
        }

        var ocrProgress = new Progress<PaddleOcrBatchProgress>(p =>
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                return;
            }

            lock (BatchLock)
            {
                var number = p.Index;
                if (!selectedIndices.Contains(number))
                {
                    return;
                }

                var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
                var pctString = percentage.ToString(CultureInfo.InvariantCulture);
                ProgressValue = number / (double)OcrSubtitleItems.Count;
                ProgressText = $"Running OCR... {number + 1}/{OcrSubtitleItems.Count}";

                var scrollToIndex = number;
                var item = p.Item;
                if (item == null)
                {
                    item = OcrSubtitleItems[p.Index];
                }

                item.Text = p.Text;
                OcrFixLineAndSetText(number, item);
            }
        });

        _ = Task.Run(async () =>
        {
            await ocrEngine.OcrBatch(engineType, batchImages, language, PaddleUseGpu, mode, ocrProgress, _cancellationTokenSource.Token);
            IsOcrRunning = false;
        });
    }

    private void RunNOcr(List<int> selectedIndices)
    {
        if (!InitNOcrDb())
        {
            return;
        }

        _skipOnceChars.Clear();
        _ = Task.Run(() => { RunNOcrLoop(selectedIndices); });
    }

    private void RunNOcrLoop(List<int> selectedIndices)
    {
        foreach (var i in selectedIndices)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                IsOcrRunning = false;
                return;
            }

            ProgressValue = i * 100.0 / OcrSubtitleItems.Count;
            ProgressText = string.Format(Se.Language.Ocr.RunningOcrDotDotDotXY, i + 1, OcrSubtitleItems.Count);

            var item = OcrSubtitleItems[i];
            var bitmap = item.GetSkBitmap();
            var parentBitmap = new NikseBitmap2(bitmap);
            parentBitmap.MakeTwoColor(200);
            parentBitmap.CropTop(0, new SKColor(0, 0, 0, 0));
            var letters = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(parentBitmap, SelectedNOcrPixelsAreSpace,
                false, true, 20, true);
            var sb = new StringBuilder();
            SelectedOcrSubtitleItem = item;
            int index = 0;
            while (index < letters.Count)
            {
                var splitterItem = letters[index];
                if (splitterItem.NikseBitmap == null)
                {
                    if (splitterItem.SpecialCharacter != null)
                    {
                        sb.Append(splitterItem.SpecialCharacter);
                    }
                }
                else
                {
                    var match = _nOcrDb!.GetMatch(parentBitmap, letters, splitterItem, splitterItem.Top, true,
                        SelectedNOcrMaxWrongPixels);

                    if (NOcrDrawUnknownText && match == null)
                    {
                        var letterIndex = letters.IndexOf(splitterItem);

                        if (_skipOnceChars.Any(p => p.LetterIndex == letterIndex && p.LineIndex == i))
                        {
                            sb.Append("*");
                            index++;
                            continue;
                        }

                        var runOnceChar =
                            _runOnceChars.FirstOrDefault(p => p.LetterIndex == letterIndex && p.LineIndex == i);
                        if (runOnceChar != null)
                        {
                            sb.Append(runOnceChar.Text);
                            _runOnceChars.Clear();
                            index++;
                            continue;
                        }

                        Dispatcher.UIThread.Post(async void () =>
                        {
                            var result =
                                await _windowService.ShowDialogAsync<NOcrCharacterAddWindow, NOcrCharacterAddViewModel>(
                                    Window!,
                                    vm =>
                                    {
                                        vm.Initialize(parentBitmap, item, letters, letterIndex, _nOcrDb,
                                            SelectedNOcrMaxWrongPixels, _nOcrAddHistoryManager, true, true);
                                    });

                            if (result.OkPressed)
                            {
                                var letterBitmap = letters[letterIndex].NikseBitmap;
                                _nOcrAddHistoryManager.Add(result.NOcrChar, letterBitmap,
                                    OcrSubtitleItems.IndexOf(item));
                                IsInspectAdditionsVisible = true;
                                _nOcrDb.Add(result.NOcrChar);
                                _ = Task.Run(() => _nOcrDb.Save());
                                _ = Task.Run(() => RunNOcrLoop(selectedIndices.Where(p => p >= i).ToList()));
                            }
                            else if (result.AbortPressed)
                            {
                                IsOcrRunning = false;
                            }
                            else if (result.UseOncePressed)
                            {
                                _runOnceChars.Add(new SkipOnceChar(i, letterIndex, result.NewText));
                                _ = Task.Run(() => RunNOcrLoop(selectedIndices.Where(p => p >= i).ToList()));
                            }
                            else if (result.SkipPressed)
                            {
                                _skipOnceChars.Add(new SkipOnceChar(i, letterIndex));
                                _ = Task.Run(() => RunNOcrLoop(selectedIndices.Where(p => p >= i).ToList()));
                            }
                            else if (result.InspectHistoryPressed)
                            {
                                IsOcrRunning = false;
                                await _windowService
                                    .ShowDialogAsync<NOcrCharacterHistoryWindow, NOcrCharacterHistoryViewModel>(Window!,
                                        vm => { vm.Initialize(_nOcrDb!, _nOcrAddHistoryManager); });
                            }
                        });

                        return;
                    }

                    if (match is { ExpandCount: > 0 })
                    {
                        index += match.ExpandCount - 1;
                    }

                    sb.Append(match != null ? _nOcrCaseFixer.FixUppercaseLowercaseIssues(splitterItem, match) : "*");
                }

                index++;
            }

            item.Text = sb.ToString().Trim();
            OcrFixLineAndSetText(i, item);
            _runOnceChars.Clear();
            _skipOnceChars.Clear();
        }

        IsOcrRunning = false;
    }

    private void OcrFixLineAndSetText(int i, OcrSubtitleItem item)
    {
        if (SelectedDictionary != null &&
            SelectedDictionary.Name != GetDictionaryNameNone() &&
            _ocrFixEngine.IsLoaded() && DoFixOcrErrors)
        {
            var result = _ocrFixEngine.FixOcrErrors(i, DoTryToGuessUnknownWords);
            var resultText = result.GetText();
            Dispatcher.UIThread.Post(() =>
            {
                item.FixResult = result;
                if (resultText != item.Text)
                {
                    item.Text = resultText;
                }

                CurrentText = item.Text;
            });

            if (!string.IsNullOrEmpty(result.ReplacementUsed.From))
            {
                AllFixes.Add(result.ReplacementUsed);
            }

            foreach (var word in result.Words)
            {
                if (!string.IsNullOrEmpty(word.ReplacementUsed.From))
                {
                    AllFixes.Add(word.ReplacementUsed);
                }

                if (word.GuessUsed)
                {
                    AllGuesses.Add($"{word.Word} -> {word.FixedWord}");
                }

                if (word.IsSpellCheckedOk == false)
                {
                    var unknownWordItem = new UnknownWordItem(item, result, word);
                    UnknownWords.Add(unknownWordItem);
                }
            }
        }
        else
        {
            item.FixResult = new OcrFixLineResult
            {
                LineIndex = i,
                Words = new List<OcrFixLinePartResult> { new() { Word = item.Text, IsSpellCheckedOk = null } },
            };
        }

        SelectAndScrollToRow(i);
    }

    private bool InitNOcrDb()
    {
        var fileName = GetNOcrLanguageFileName();
        if (_nOcrDb != null && _nOcrDb.FileName == fileName)
        {
            return true;
        }

        if (fileName == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(fileName) && (_nOcrDb == null || _nOcrDb.FileName != fileName))
        {
            _nOcrDb = new NOcrDb(fileName);
        }

        return true;
    }

    private void RunTesseractOcr(List<int> selectedIndices)
    {
        var tesseractOcr = new TesseractOcr();
        var language = SelectedTesseractDictionaryItem?.Code ?? "eng";

        _ = Task.Run(async () =>
        {
            foreach (var i in selectedIndices)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }

                ProgressValue = i * 100.0 / OcrSubtitleItems.Count;
                ProgressText = string.Format(Se.Language.Ocr.RunningOcrDotDotDotXY, i + 1, OcrSubtitleItems.Count);

                var item = OcrSubtitleItems[i];
                var bitmap = item.GetSkBitmap();

                var text = await tesseractOcr.Ocr(bitmap, language, _cancellationTokenSource.Token);
                item.Text = text;

                OcrFixLineAndSetText(i, item);

                if (SelectedOcrSubtitleItem == item)
                {
                    CurrentText = text;
                }
            }

            PauseOcr();
        });
    }

    private void RunOllamaOcr(List<int> selectedIndices)
    {
        var ollamaOcr = new OllamaOcr();

        _ = Task.Run(async () =>
        {
            foreach (var i in selectedIndices)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }

                ProgressValue = i * 100.0 / OcrSubtitleItems.Count;
                ProgressText = string.Format(Se.Language.Ocr.RunningOcrDotDotDotXY, i + 1, OcrSubtitleItems.Count);

                var item = OcrSubtitleItems[i];
                var bitmap = item.GetSkBitmap();

                SelectAndScrollToRow(i);

                var text = await ollamaOcr.Ocr(bitmap, OllamaModel, OllamaUrl, SelectedOllamaLanguage ?? "English", _cancellationTokenSource.Token);
                item.Text = text;

                if (SelectedOcrSubtitleItem == item)
                {
                    CurrentText = text;
                }
            }

            PauseOcr();
        });
    }

    private void RunMistralOcr(List<int> selectedIndices)
    {
        var mistralOcr = new MistralOcr(MistralApiKey);

        _ = Task.Run(async () =>
        {
            foreach (var i in selectedIndices)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }

                ProgressValue = i * 100.0 / OcrSubtitleItems.Count;
                ProgressText = string.Format(Se.Language.Ocr.RunningOcrDotDotDotXY, i + 1, OcrSubtitleItems.Count);

                var item = OcrSubtitleItems[i];
                var bitmap = item.GetSkBitmap();

                SelectAndScrollToRow(i);

                var text = await mistralOcr.Ocr(bitmap, SelectedOllamaLanguage ?? "English", _cancellationTokenSource.Token);
                item.Text = text;

                if (SelectedOcrSubtitleItem == item)
                {
                    CurrentText = text;
                }
            }

            PauseOcr();
        });
    }

    private async Task<bool> CheckAndDownloadTesseract()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var tesseractExe = Path.Combine(Se.TesseractFolder, "tesseract.exe");
            if (File.Exists(tesseractExe))
            {
                return true;
            }

            var answer = await MessageBox.Show(
                Window!,
                "Download Tesseract OCR?",
                $"{Environment.NewLine}\"Tesseract\" requires downloading Tesseract OCR.{Environment.NewLine}{Environment.NewLine}Download and use Tesseract OCR?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }

            await _windowService.ShowDialogAsync<DownloadTesseractWindow, DownloadTesseractViewModel>(Window!);

            return File.Exists(tesseractExe);
        }

        try
        {
            var fileName = TesseractOcr.GetExecutablePath();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            process.WaitForExit(2000); // Wait max 2 seconds

            if (process.ExitCode == 0)
            {
                return true;
            }
        }
        catch
        {
            // ignore
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            await MessageBox.Show(
                Window!,
                "Please install Tesseract",
                $"{Environment.NewLine}\"Tesseract\" was not detected. Please install Tesseract." +
                Environment.NewLine + "" +
                "E.g. ´brew install tesseract´.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            await MessageBox.Show(
                Window!,
                "Please install Tesseract",
                $"{Environment.NewLine}\"Tesseract\" was not detected. Please install Tesseract." +
                Environment.NewLine +
                $"E.g. ´sudo apt install tesseract-ocr´ or ´sudo pacman -S tesseract´.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        return false;
    }

    private async Task<bool> TesseractModelDownload()
    {
        var result =
            await _windowService.ShowDialogAsync<DownloadTesseractModelWindow, DownloadTesseractModelViewModel>(Window!);

        LoadActiveTesseractDictionaries();
        if (result.OkPressed)
        {
            var item = TesseractDictionaryItems.FirstOrDefault(p =>
                p.Code == result.SelectedTesseractDictionaryItem?.Code);
            SelectedTesseractDictionaryItem = item ?? TesseractDictionaryItems.FirstOrDefault();
            return true;
        }

        return false;
    }

    private void LoadActiveTesseractDictionaries()
    {
        TesseractDictionaryItems.Clear();

        var folder = Se.TesseractModelFolder;
        if (!Directory.Exists(folder))
        {
            return;
        }

        var allDictionaries = TesseractDictionary.List();
        var items = new List<TesseractDictionary>();
        foreach (var file in Directory.GetFiles(folder, "*.traineddata"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (name == "osd")
            {
                continue;
            }

            var dictionary = allDictionaries.FirstOrDefault(p => p.Code == name);
            if (dictionary != null)
            {
                items.Add(dictionary);
            }
            else
            {
                items.Add(new TesseractDictionary { Code = name, Name = name, Url = string.Empty });
            }
        }

        TesseractDictionaryItems.AddRange(items.OrderBy(p => p.ToString()));
    }

    internal void SubtitleGridKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.I && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            ToggleItalic();
            e.Handled = true; // prevent further handling if needed
        }
        else if (e.Key == Key.P && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true; // prevent further handling if needed
            Dispatcher.UIThread.Post(async void () => { await ViewSelectedImage(); });
        }
        else if (e.Key == Key.Delete)
        {
            e.Handled = true; // prevent further handling if needed
            DeleteSelectedLines();
        }
    }

    internal void SubtitleGridDoubelTapped()
    {
        var engine = SelectedOcrEngine;
        if (engine == null)
        {
            return;
        }

        if (engine != null && engine.EngineType == OcrEngineType.nOcr)
        {
            Dispatcher.UIThread.Post(async void () =>
            {
                await InspectAdditions();
            });
            return;
        }
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Cancel();
        }
        else if (e.Key == Key.G && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true; // prevent further handling if needed
            Dispatcher.UIThread.Post(async void () => { await ShowGoToLine(); });
        }
    }

    internal void DataGridTracksSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        bool flowControl = TrackChanged();
        if (!flowControl)
        {
            return;
        }
    }

    private bool TrackChanged()
    {
        return true;
    }

    public static Bitmap ConvertSkBitmapToAvaloniaBitmap(SKBitmap skBitmap)
    {
        using var image = SKImage.FromBitmap(skBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new MemoryStream(data.ToArray());

        return new Bitmap(stream);
    }


    internal void SelectAndScrollToRow(int index)
    {
        if (index < 0 || index >= OcrSubtitleItems.Count)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            SelectedOcrSubtitleItem = OcrSubtitleItems[index];
            SubtitleGrid.SelectedIndex = index;
            SubtitleGrid.ScrollIntoView(SelectedOcrSubtitleItem, null);
            TrackChanged();
        });
    }

    public void Initialize(List<BluRaySupParser.PcsData> subtitles, string fileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, fileName);
        _ocrSubtitle = new OcrSubtitleBluRay(subtitles);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    public void Initialize(List<VobSubMergedPack> vobSubMergedPackList, List<SKColor> palette, string vobSubFileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, vobSubFileName);
        _ocrSubtitle = new OcrSubtitleVobSub(vobSubMergedPackList);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    public void Initialize(Trak mp4SubtitleTrack, List<Paragraph> paragraphs, string fileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, fileName);
        _ocrSubtitle = new OcrSubtitleMp4VobSub(mp4SubtitleTrack, paragraphs);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    public void Initialize(List<VobSubMergedPack> mergedVobSubPacks, MatroskaTrackInfo matroskaSubtitleInfo, string fileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, fileName);
        _ocrSubtitle = new OcrSubtitleVobSub(mergedVobSubPacks);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    public void Initialize(MatroskaTrackInfo matroskaSubtitleInfo, Subtitle subtitle, List<DvbSubPes> subtitleImages, string fileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, fileName);
        _ocrSubtitle = new OcrSubtitleMkvDvb(matroskaSubtitleInfo, subtitle, subtitleImages);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    public void Initialize(MatroskaTrackInfo matroskaSubtitleInfo, List<BluRaySupParser.PcsData> pcsDataList, string fileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, fileName);
        _ocrSubtitle = new OcrSubtitleMkvBluRay(matroskaSubtitleInfo, pcsDataList);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    public void Initialize(IList<IBinaryParagraphWithPosition> list, string fileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, fileName);
        _ocrSubtitle = new OcrSubtitleIBinaryParagrap(list);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    public void InitializeBdn(Subtitle subtitle, string fileName, bool isSon)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, fileName);
        _ocrSubtitle = new OcrSubtitleBdn(subtitle, fileName, isSon);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    internal void Initialize(TransportStreamParser tsParser, List<TransportStreamSubtitle> subtitles, string fileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, fileName);
        _ocrSubtitle = new OcrSubtitleTransportStream(tsParser, subtitles, fileName);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    internal void EngineSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        EngineSelectionChanged();
    }

    private void EngineSelectionChanged()
    {
        if (SelectedOcrEngine == null)
        {
            SelectedOcrEngine = OcrEngines.FirstOrDefault();
        }

        IsNOcrVisible = SelectedOcrEngine?.EngineType == OcrEngineType.nOcr;
        IsInspectLineVisible = SelectedOcrEngine?.EngineType == OcrEngineType.nOcr;
        IsOllamaVisible = SelectedOcrEngine?.EngineType == OcrEngineType.Ollama;
        IsTesseractVisible = SelectedOcrEngine?.EngineType == OcrEngineType.Tesseract;
        IsPaddleOcrVisible = SelectedOcrEngine?.EngineType == OcrEngineType.PaddleOcrStandalone || SelectedOcrEngine?.EngineType == OcrEngineType.PaddleOcrPython;
        IsGoogleVisionVisible = SelectedOcrEngine?.EngineType == OcrEngineType.GoogleVision;
        IsMistralOcrVisible = SelectedOcrEngine?.EngineType == OcrEngineType.Mistral;

        if (IsNOcrVisible && NOcrDatabases.Count == 0)
        {
            foreach (var s in NOcrDb.GetDatabases().OrderBy(p => p))
            {
                NOcrDatabases.Add(s);
            }

            if (!string.IsNullOrEmpty(Se.Settings.Ocr.NOcrDatabase) &&
                NOcrDatabases.Contains(Se.Settings.Ocr.NOcrDatabase))
            {
                SelectedNOcrDatabase = Se.Settings.Ocr.NOcrDatabase;
            }

            if (SelectedNOcrDatabase == null)
            {
                SelectedNOcrDatabase = NOcrDb.GetDatabases().FirstOrDefault();
            }
        }

        if (IsTesseractVisible)
        {
            LoadActiveTesseractDictionaries();
            if (SelectedTesseractDictionaryItem == null)
            {
                SelectedTesseractDictionaryItem = TesseractDictionaryItems.FirstOrDefault(p => p.Code == "eng") ??
                                                  TesseractDictionaryItems.FirstOrDefault();
            }
        }

        if (IsPaddleOcrVisible)
        {
            if (SelectedPaddleOcrLanguage == null)
            {
                SelectedPaddleOcrLanguage = PaddleOcrLanguages.FirstOrDefault(p => p.Code == "eng") ??
                                            PaddleOcrLanguages.FirstOrDefault();
            }
        }

        if (IsGoogleVisionVisible)
        {
            if (SelectedGoogleVisionLanguage == null)
            {
                SelectedGoogleVisionLanguage = GoogleVisionLanguages.FirstOrDefault(p => p.Code == "eng") ??
                                               GoogleVisionLanguages.FirstOrDefault();
            }
        }
    }

    internal void OnClosing(WindowClosingEventArgs e)
    {
        SaveSettings();
    }

    internal void SubtitleGridContextOpening(object? sender, EventArgs e)
    {
        ShowContextMenu = OcrSubtitleItems.Count > 0;
    }

    public void DictionaryChanged()
    {
        IsDictionaryLoaded = Dictionaries.IndexOf(SelectedDictionary ?? Dictionaries.First()) > 0;
    }

    internal void OnLoaded()
    {
        DictionaryChanged();
    }

    internal void TextBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        var selected = SelectedOcrSubtitleItem;
        if (selected == null)
        {
            return;
        }

        if (selected.FixResult == null)
        {
            return;
        }

        if (selected.FixResult.GetText() == selected.Text)
        {
            return;
        }

        var idx = OcrSubtitleItems.IndexOf(selected);
        selected.FixResult = new OcrFixLineResult(idx, selected.Text);
        //TODO: spell check?
    }

    private async Task ShowGoToLine()
    {
        if (OcrSubtitleItems.Count == 0)
        {
            return;
        }

        var viewModel = await _windowService.ShowDialogAsync<GoToLineNumberWindow, GoToLineNumberViewModel>(Window!,
            vm => { vm.MaxLineNumber = OcrSubtitleItems.Count; });

        if (viewModel is { OkPressed: true, LineNumber: >= 0 } && viewModel.LineNumber <= OcrSubtitleItems.Count)
        {
            SelectAndScrollToRow(viewModel.LineNumber - 1);
        }
    }

    internal void UnknownWordSelectionChanged()
    {
        IsUnknownWordSelected = SelectedUnknownWord != null;
    }

    internal void UnknownWordSelectionTapped()
    {
        if (SelectedUnknownWord == null)
        {
            return;
        }

        SelectAndScrollToRow(OcrSubtitleItems.IndexOf(SelectedUnknownWord.Item));
    }
}