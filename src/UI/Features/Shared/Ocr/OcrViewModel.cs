using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Core.VobSub.Ocr.Service;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
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

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public partial class OcrViewModel : ObservableObject
{
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
    [ObservableProperty] private bool _nOcrDrawUnknownText;
    [ObservableProperty] private bool _isInspectLineVisible;
    [ObservableProperty] private bool _isInspectAdditionsVisible;
    [ObservableProperty] private string _googleVisionApiKey;
    [ObservableProperty] private ObservableCollection<OcrLanguage> _googleVisionLanguages;
    [ObservableProperty] private OcrLanguage? _selectedGoogleVisionLanguage;
    [ObservableProperty] private ObservableCollection<OcrLanguage2> _paddleOcrLanguages;
    [ObservableProperty] private OcrLanguage2? _selectedPaddleOcrLanguage;
    [ObservableProperty] private bool _paddleUseGpu;

    public Window? Window { get; set; }
    public DataGrid SubtitleGrid { get; set; }
    public MatroskaTrackInfo? SelectedMatroskaTrack { get; set; }
    public bool OkPressed { get; private set; }
    public string WindowTitle { get; private set; }
    public readonly List<SubtitleLineViewModel> OcredSubtitle;

    private IOcrSubtitle? _ocrSubtitle;
    private readonly INOcrCaseFixer _nOcrCaseFixer;
    private readonly IWindowService _windowService;
    private CancellationTokenSource _cancellationTokenSource;
    private NOcrDb? _nOcrDb;
    private readonly List<SkipOnceChar> _runOnceChars;
    private readonly List<SkipOnceChar> _skipOnceChars;
    private readonly NOcrAddHistoryManager _nOcrAddHistoryManager;

    public OcrViewModel(INOcrCaseFixer nOcrCaseFixer, IWindowService windowService)
    {
        _nOcrCaseFixer = nOcrCaseFixer;
        _windowService = windowService;

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
        WindowTitle = string.Empty;
        CurrentBitmapInfo = string.Empty;
        CurrentText = string.Empty;
        ProgressText = string.Empty;
        OllamaModel = string.Empty;
        TesseractDictionaryItems = new ObservableCollection<TesseractDictionary>();
        GoogleVisionApiKey = string.Empty;
        GoogleVisionLanguages = new ObservableCollection<OcrLanguage>(GoogleVisionOcr.GetLanguages().OrderBy(p => p.ToString()));
        PaddleOcrLanguages = new ObservableCollection<OcrLanguage2>(PaddleOcr.GetLanguages().OrderBy(p => p.ToString()));
        _runOnceChars = new List<SkipOnceChar>();
        _skipOnceChars = new List<SkipOnceChar>();
        _nOcrAddHistoryManager = new NOcrAddHistoryManager();
        _cancellationTokenSource = new CancellationTokenSource();
        OcredSubtitle = new List<SubtitleLineViewModel>();
        LoadSettings();
        EngineSelectionChanged();
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
            SelectedOllamaLanguage = ocr.OllamaLanguage;
            GoogleVisionApiKey = ocr.GoogleVisionApiKey;
            SelectedGoogleVisionLanguage = GoogleVisionLanguages.FirstOrDefault(p => p.Code == ocr.GoogleVisionLanguage);
            SelectedPaddleOcrLanguage = PaddleOcrLanguages.FirstOrDefault(p => p.Code == Se.Settings.Ocr.PaddleOcrLastLanguage) ?? PaddleOcrLanguages.First();
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
        ocr.OllamaLanguage = SelectedOllamaLanguage ?? "English";
        ocr.GoogleVisionApiKey = GoogleVisionApiKey;
        ocr.GoogleVisionLanguage = SelectedGoogleVisionLanguage?.Code ?? "en";
        Se.SaveSettings();
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
    private void PauseOcr()
    {
        IsOcrRunning = false;
        _cancellationTokenSource.Cancel();
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
    private void PickOllamaModel()
    {
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
    private async Task StartOcr()
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

        SaveSettings();
        _cancellationTokenSource = new CancellationTokenSource();
        IsOcrRunning = true;
        var startFromIndex = SelectedOcrSubtitleItem == null ? 0 : OcrSubtitleItems.IndexOf(SelectedOcrSubtitleItem);
        ProgressText = Se.Language.Ocr.RunningOcrDotDotDot;
        ProgressValue = 0d;

        if (ocrEngine.EngineType == OcrEngineType.nOcr)
        {
            RunNOcr(startFromIndex);
        }
        else if (ocrEngine.EngineType == OcrEngineType.Tesseract)
        {
            RunTesseractOcr(startFromIndex);
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

                var result = await _windowService.ShowDialogAsync<DownloadPaddleOcrWindow, DownloadPaddleOcrViewModel>(Window!, vm =>
                {
                    vm.Initialize(PaddleOcrDownloadType.EngineCpu);
                });
                if (!result.OkPressed)
                {
                    PauseOcr();
                    return;
                }
            }

            var modelsDirectory = Se.PaddleOcrModelsFolder;
            if (!Directory.Exists(modelsDirectory))
            {
                var result = await _windowService.ShowDialogAsync<DownloadPaddleOcrWindow, DownloadPaddleOcrViewModel>(Window!, vm =>
                {
                    vm.Initialize(PaddleOcrDownloadType.Models);
                });
                if (!result.OkPressed)
                {
                    PauseOcr();
                    return;
                }
            }

            RunPaddleOcr(startFromIndex, _ocrSubtitle!.Count - startFromIndex, ocrEngine.EngineType);
        }
        else if (ocrEngine.EngineType == OcrEngineType.PaddleOcrPython)
        {
            var modelsDirectory = Se.PaddleOcrModelsFolder;
            if (!Directory.Exists(modelsDirectory))
            {
                var result = await _windowService.ShowDialogAsync<DownloadPaddleOcrWindow, DownloadPaddleOcrViewModel>(Window!, vm =>
                {
                    vm.Initialize(PaddleOcrDownloadType.Models);
                });
                if (!result.OkPressed)
                {
                    PauseOcr();
                    return;
                }
            }

            RunPaddleOcr(startFromIndex, _ocrSubtitle!.Count - startFromIndex, ocrEngine.EngineType);
        }
        else if (ocrEngine.EngineType == OcrEngineType.Ollama)
        {
            RunOllamaOcr();
        }
        else if (ocrEngine.EngineType == OcrEngineType.GoogleVision)
        {
            //   RunGoogleVisionOcr(startFromIndex);
        }
    }

    private Lock BatchLock = new Lock();

    private void RunPaddleOcr(int startFromIndex, int numberOfImages, OcrEngineType engineType)
    {
        var ocrEngine = new PaddleOcr();
        var language = SelectedPaddleOcrLanguage?.Code ?? "en";
        var mode = Se.Settings.Ocr.PaddleOcrMode;
        Se.Settings.Ocr.PaddleOcrLastLanguage = language;

        var batchImages = new List<PaddleOcrBatchInput>(numberOfImages);
        var max = startFromIndex + numberOfImages;
        ProgressText = $"Initializing Paddle OCR...";
        for (var i = startFromIndex; i < max; i++)
        {
            var ocrItem = OcrSubtitleItems[i];
            batchImages.Add(new PaddleOcrBatchInput
            {
                Bitmap = ocrItem.GetSkBitmap(),
                Index = i,
                Text = $"{i} / {max}: {ocrItem.StartTime} - {ocrItem.EndTime}"
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
                if (number > max)
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
                Dispatcher.UIThread.Post(() =>
                {
                    SelectAndScrollToRow(scrollToIndex);
                });
            }
        });

        _ = Task.Run(async () =>
        {
            await ocrEngine.OcrBatch(engineType, batchImages, language, PaddleUseGpu, mode, ocrProgress, _cancellationTokenSource.Token);
            IsOcrRunning = false;
        });
    }

    private void RunNOcr(int startFromIndex)
    {
        if (!InitNOcrDb())
        {
            return;
        }

        _skipOnceChars.Clear();
        _ = Task.Run(() => { RunNOcrLoop(startFromIndex); });
    }

    private void RunNOcrLoop(int startFromIndex)
    {
        for (var i = startFromIndex; i < OcrSubtitleItems.Count; i++)
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
                                _ = Task.Run(() => RunNOcrLoop(i));
                            }
                            else if (result.AbortPressed)
                            {
                                IsOcrRunning = false;
                            }
                            else if (result.UseOncePressed)
                            {
                                _runOnceChars.Add(new SkipOnceChar(i, letterIndex, result.NewText));
                                _ = Task.Run(() => RunNOcrLoop(i));
                            }
                            else if (result.SkipPressed)
                            {
                                _skipOnceChars.Add(new SkipOnceChar(i, letterIndex));
                                _ = Task.Run(() => RunNOcrLoop(i));
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

            SelectAndScrollToRow(i);

            _runOnceChars.Clear();
            _skipOnceChars.Clear();
        }

        IsOcrRunning = false;
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

    private void RunTesseractOcr(int startFromIndex)
    {
        var tesseractOcr = new TesseractOcr();
        var language = SelectedTesseractDictionaryItem?.Code ?? "eng";

        _ = Task.Run(async () =>
        {
            for (var i = startFromIndex; i < OcrSubtitleItems.Count; i++)
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

                var text = await tesseractOcr.Ocr(bitmap, language, _cancellationTokenSource.Token);
                item.Text = text;

                if (SelectedOcrSubtitleItem == item)
                {
                    CurrentText = text;
                }
            }

            PauseOcr();
        });
    }

    private void RunOllamaOcr()
    {
        var selectedOcrSubtitleItem = SelectedOcrSubtitleItem;
        if (selectedOcrSubtitleItem == null)
        {
            return;
        }

        var ollamaOcr = new OllamaOcr();
        var startFromIndex = OcrSubtitleItems.IndexOf(selectedOcrSubtitleItem);

        _ = Task.Run(async () =>
        {
            for (var i = startFromIndex; i < OcrSubtitleItems.Count; i++)
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

                var text = await ollamaOcr.Ocr(bitmap, OllamaModel, SelectedOllamaLanguage ?? "English",
                    _cancellationTokenSource.Token);
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


    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Cancel();
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
        _ocrSubtitle = new OcrSubtitleBluRay(subtitles);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    public void Initialize(List<VobSubMergedPack> vobSubMergedPackList, List<SKColor> palette, string vobSubFileName)
    {
        _ocrSubtitle = new OcrSubtitleVobSub(vobSubMergedPackList);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    public void Initialize(Trak mp4SubtitleTrack,  List<Paragraph> paragraphs, string fileName)
    {
        _ocrSubtitle = new OcrSubtitleMp4VobSub(mp4SubtitleTrack, paragraphs);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    internal void EngineSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        EngineSelectionChanged();
    }

    private void EngineSelectionChanged()
    {
        IsNOcrVisible = SelectedOcrEngine?.EngineType == OcrEngineType.nOcr;
        IsInspectLineVisible = SelectedOcrEngine?.EngineType == OcrEngineType.nOcr;
        IsOllamaVisible = SelectedOcrEngine?.EngineType == OcrEngineType.Ollama;
        IsTesseractVisible = SelectedOcrEngine?.EngineType == OcrEngineType.Tesseract;
        IsPaddleOcrVisible = SelectedOcrEngine?.EngineType == OcrEngineType.PaddleOcrStandalone || SelectedOcrEngine?.EngineType == OcrEngineType.PaddleOcrPython;
        IsGoogleVisionVisible = SelectedOcrEngine?.EngineType == OcrEngineType.GoogleVision;

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
}