using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.VobSub.Ocr.Service;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Ocr;
using SkiaSharp;
using SubtitleAlchemist.Logic.Ocr;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    [ObservableProperty] private string _googleVisionApiKey;
    [ObservableProperty] private ObservableCollection<OcrLanguage> _googleVisionLanguages;
    [ObservableProperty] private OcrLanguage? _selectedGoogleVisionLanguage;
    [ObservableProperty] private ObservableCollection<OcrLanguage2> _paddleOcrLanguages;
    [ObservableProperty] private OcrLanguage2? _selectedPaddleOcrLanguage;

    public Window? Window { get; set; }
    public DataGrid SubtitleGrid { get; set; }
    public MatroskaTrackInfo? SelectedMatroskaTrack { get; set; }
    public bool OkPressed { get; private set; }
    public string WindowTitle { get; private set; }
    public List<SubtitleLineViewModel> OcredSubtitle;

    private IOcrSubtitle? _ocrSubtitle;
    private readonly INOcrCaseFixer _nOcrCaseFixer;
    private readonly IWindowService _windowService;
    private CancellationTokenSource _cancellationTokenSource;
    private NOcrDb? _nOcrDb;
    private readonly List<SkipOnceChar> _runOnceChars;
    private readonly List<SkipOnceChar> _skipOnceChars;

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
        _cancellationTokenSource = new CancellationTokenSource();
        OcredSubtitle = new List<SubtitleLineViewModel>();
        LoadSettings();
        EngineSelectionChanged();
    }

    private void LoadSettings()
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
        _cancellationTokenSource?.Cancel();
    }

    [RelayCommand]
    private void Export()
    {
    }

    [RelayCommand]
    private void PickOllamaModel()
    {
    }

    [RelayCommand]
    private async Task PickTesseractModel()
    {
        await TesseractModelDownload();
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
        OkPressed = true;

        OcredSubtitle.Clear();
        for (var i = 0; i < OcrSubtitleItems.Count; i++)
        {
            OcrSubtitleItem? item = OcrSubtitleItems[i];
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
        _cancellationTokenSource?.Cancel();
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
        ProgressText = "Running OCR...";
        ProgressValue = 0d;

        if (ocrEngine.EngineType == OcrEngineType.nOcr)
        {
            RunNOcr(startFromIndex);
        }
        else if (ocrEngine.EngineType == OcrEngineType.Tesseract)
        {
            RunTesseractOcr(startFromIndex);
        }
        else if (ocrEngine.EngineType == OcrEngineType.PaddleOcr)
        {
            //if (!Directory.Exists(Se.PaddleOcrFolder))
            //{
            //    var answer = await Page.DisplayAlert(
            //        "Download Paddle OCR models?",
            //        $"{Environment.NewLine}\"Paddle OCR\" requires AI model files.{Environment.NewLine}{Environment.NewLine}Download and use Paddle OCR models?",
            //        "Yes",
            //        "No");

            //    if (!answer)
            //    {
            //        await PauseOcr();
            //        return;
            //    }

            //    var result = await _popupService.ShowPopupAsync<DownloadPaddleOcrModelsPopupModel>(CancellationToken.None);
            //    if (result is not string)
            //    {
            //        await PauseOcr();
            //        return;
            //    }
            //}

            //RunPaddleOcrBatch(startFromIndex, 10);
        }
        else if (ocrEngine.EngineType == OcrEngineType.Ollama)
        {
            RunOllamaOcr();
        }
        else if (ocrEngine.EngineType == OcrEngineType.Ollama)
        {
            //   RunGoogleVisionOcr(startFromIndex);
        }
    }

    private void RunNOcr(int startFromIndex)
    {

        if (!InitNOcrDb())
        {
            return;
        }

        _ = Task.Run(() =>
        {
            for (var i = startFromIndex; i < OcrSubtitleItems.Count; i++)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }

                ProgressValue = i * 100.0 / (double)OcrSubtitleItems.Count;
                ProgressText = $"Running OCR... {i + 1}/{OcrSubtitleItems.Count}";

                var item = OcrSubtitleItems[i];
                var bitmap = item.GetSkBitmap();
                var nBmp = new NikseBitmap2(bitmap);
                nBmp.MakeTwoColor(200);
                nBmp.CropTop(0, new SKColor(0, 0, 0, 0));
                var list = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(nBmp, SelectedNOcrPixelsAreSpace, false, true, 20, true);
                var sb = new StringBuilder();
                SelectedOcrSubtitleItem = item;

                foreach (var splitterItem in list)
                {
                    if (splitterItem.NikseBitmap == null)
                    {
                        if (splitterItem.SpecialCharacter != null)
                        {
                            sb.Append(splitterItem.SpecialCharacter);
                        }
                    }
                    else
                    {
                        var match = _nOcrDb!.GetMatch(nBmp, list, splitterItem, splitterItem.Top, true, SelectedNOcrMaxWrongPixels);

                        if (NOcrDrawUnknownText && match == null)
                        {
                            var letterIndex = list.IndexOf(splitterItem);

                            if (_skipOnceChars.Any(p => p.LetterIndex == letterIndex && p.LineIndex == i))
                            {
                                sb.Append("*");
                                continue;
                            }

                            var runOnceChar = _runOnceChars.FirstOrDefault(p => p.LetterIndex == letterIndex && p.LineIndex == i);
                            if (runOnceChar != null)
                            {
                                sb.Append(runOnceChar.Text);
                                continue;
                            }

                            //Dispatcher.UIThread.Invoke(() =>
                            //{
                            //    //await PauseOcr();
                            //    //await Shell.Current.GoToAsync(nameof(NOcrCharacterAddPage), new Dictionary<string, object>
                            //    //    {
                            //    //    { "Page", nameof(OcrPage) },
                            //    //    { "Bitmap", nBmp.GetBitmap() },
                            //    //    { "Letters", list },
                            //    //    { "Item", splitterItem },
                            //    //    { "OcrSubtitleItems", OcrSubtitleItems.ToList() },
                            //    //    { "StartFromNumber", SelectedStartFromNumber },
                            //    //    { "ItalicOn", _toolsItalicOn },
                            //    //    { "nOcrDb", _nOcrDb },
                            //    //    { "MaxWrongPixels", SelectedNOcrMaxWrongPixels },
                            //    //    });
                            //});
                            //return;
                        }

                        sb.Append(match != null ? _nOcrCaseFixer.FixUppercaseLowercaseIssues(splitterItem, match) : "*");
                    }
                }

                item.Text = sb.ToString().Trim();

                SelectAndScrollToRow(i);

                _runOnceChars.Clear();
                _skipOnceChars.Clear();
            }

            IsOcrRunning = false;
        });
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
                ProgressText = $"Running OCR... {i + 1}/{OcrSubtitleItems.Count}";

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
                ProgressText = $"Running OCR... {i + 1}/{OcrSubtitleItems.Count}";

                var item = OcrSubtitleItems[i];
                var bitmap = item.GetSkBitmap();

                SelectAndScrollToRow(i);

                var text = await ollamaOcr.Ocr(bitmap, OllamaModel, SelectedOllamaLanguage ?? "English", _cancellationTokenSource.Token);
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
        var tesseractFolder = Se.TesseractFolder;
        if (!string.IsNullOrEmpty(tesseractFolder))
        {
            return true;
        }
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            await MessageBox.Show(
                Window!,
                "Please install Tesseract",
                $"{Environment.NewLine}\"Tesseract\" was not detected. Please install Tesseract - with e.g. ´brew install tesseract´.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            
            return false;
        }
        
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await MessageBox.Show(
                Window!,
                "Please install Tesseract",
                $"{Environment.NewLine}\"Tesseract\" was not detected. Please install Tesseract.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            
            return false;
        }

        var tesseractExe = Path.Combine(Se.TesseractFolder, "tesseract.exe");
        if (!File.Exists(tesseractExe)) //TODO: check for mac/Linux executable on mac/Linux
        {
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

            var result = await _windowService.ShowDialogAsync<DownloadTesseractWindow, DownloadTesseractViewModel>(Window!, vm => { });
            return result.OkPressed;
        }

        return true;
    }

    private async Task<bool> TesseractModelDownload()
    {
        var result = await _windowService.ShowDialogAsync<DownloadTesseractModelWindow, DownloadTesseractModelViewModel>(Window!, vm => { });

        LoadActiveTesseractDictionaries();
        if (result.OkPressed)
        {
            var item = TesseractDictionaryItems.FirstOrDefault(p => p.Code == result.SelectedTesseractDictionaryItem?.Code);
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

    public static Bitmap ConvertSKBitmapToAvaloniaBitmap(SKBitmap skBitmap)
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
        }, DispatcherPriority.Background);
    }

    public void Initialize(List<BluRaySupParser.PcsData> subtitles, string fileName)
    {
        _ocrSubtitle = new BluRayPcsDataList(subtitles);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    internal void EngineSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        EngineSelectionChanged();
    }

    internal void EngineSelectionChanged()
    {
        IsNOcrVisible = SelectedOcrEngine?.EngineType == OcrEngineType.nOcr;
        IsOllamaVisible = SelectedOcrEngine?.EngineType == OcrEngineType.Ollama;
        IsTesseractVisible = SelectedOcrEngine?.EngineType == OcrEngineType.Tesseract;
        IsPaddleOcrVisible = SelectedOcrEngine?.EngineType == OcrEngineType.PaddleOcr;
        IsGoogleVisionVisible = SelectedOcrEngine?.EngineType == OcrEngineType.GoogleVision;

        if (IsNOcrVisible && NOcrDatabases.Count == 0)
        {
            foreach (var s in NOcrDb.GetDatabases().OrderBy(p => p))
            {
                NOcrDatabases.Add(s);
            }
            SelectedNOcrDatabase = NOcrDb.GetDatabases().FirstOrDefault();
        }

        if (IsTesseractVisible)
        {
            LoadActiveTesseractDictionaries();
            if (SelectedTesseractDictionaryItem == null)
            {
                SelectedTesseractDictionaryItem = TesseractDictionaryItems.FirstOrDefault(p => p.Code == "eng") ?? TesseractDictionaryItems.FirstOrDefault();
            }
        }

        if (IsPaddleOcrVisible)
        {
            if (SelectedPaddleOcrLanguage == null)
            {
                SelectedPaddleOcrLanguage = PaddleOcrLanguages.FirstOrDefault(p => p.Code == "eng") ?? PaddleOcrLanguages.FirstOrDefault();
            }
        }

        if (IsGoogleVisionVisible)
        {
            if (SelectedGoogleVisionLanguage == null)
            {
                SelectedGoogleVisionLanguage = GoogleVisionLanguages.FirstOrDefault(p => p.Code == "eng") ?? GoogleVisionLanguages.FirstOrDefault();
            }
        }
    }
}