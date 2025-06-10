using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.EncodingSettings;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public partial class BatchConvertViewModel : ObservableObject
{
    [ObservableProperty] private string _targetFormatName;
    [ObservableProperty] private string _targetEncoding;
    [ObservableProperty] private ObservableCollection<BatchConvertItem> _batchItems;
    [ObservableProperty] private BatchConvertItem? _selectedBatchItem;
    [ObservableProperty] private string _batchItemsInfo;
    [ObservableProperty] private ObservableCollection<string> _targetFormats;
    [ObservableProperty] private string? _selectedTargetFormat;
    [ObservableProperty] private ObservableCollection<string> _targetEncodings;
    [ObservableProperty] private string? _selectedTargetEncoding;
    [ObservableProperty] private ObservableCollection<BatchConvertFunction> _batchFunctions;
    [ObservableProperty] private BatchConvertFunction? _selectedBatchFunction;
    [ObservableProperty] private bool _isProgressVisible;
    [ObservableProperty] private bool _isConverting;
    [ObservableProperty] private bool _areControlsEnabled;
    [ObservableProperty] private string _outputPropertiesText;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;

    // Remove formatting
    [ObservableProperty] private bool _formattingRemoveAll;
    [ObservableProperty] private bool _formattingRemoveItalic;
    [ObservableProperty] private bool _formattingRemoveBold;
    [ObservableProperty] private bool _formattingRemoveUnderline;
    [ObservableProperty] private bool _formattingRemoveFontTags;
    [ObservableProperty] private bool _formattingRemoveAlignmentTags;
    [ObservableProperty] private bool _formattingRemoveColors;

    // Offset time codes
    [ObservableProperty] private bool _offsetTimeCodesForward;
    [ObservableProperty] private bool _offsetTimeCodesBack;
    [ObservableProperty] private TimeSpan _offsetTimeCodesTime;

    // Adjust display duration
    [ObservableProperty] private ObservableCollection<AdjustDurationDisplay> _adjustTypes;
    [ObservableProperty] private AdjustDurationDisplay _selectedAdjustType;
    [ObservableProperty] private double _adjustSeconds;
    [ObservableProperty] private int _adjustPercent;
    [ObservableProperty] private double _adjustFixed;
    [ObservableProperty] private double _adjustRecalculateMaxCharacterPerSecond;
    [ObservableProperty] private double _adjustRecalculateOptimalCharacterPerSecond;
    [ObservableProperty] private bool _adjustIsSecondsVisible;
    [ObservableProperty] private bool _adjustIsPercentVisible;
    [ObservableProperty] private bool _adjustIsFixedVisible;
    [ObservableProperty] private bool _adjustIsRecalculateVisible;

    // Delete lines
    [ObservableProperty] private ObservableCollection<int> _deleteLineNumbers;
    [ObservableProperty] private int _deleteXFirstLines;
    [ObservableProperty] private int _deleteXLastLines;
    [ObservableProperty] private string _deleteLinesContains;

    // Change frame rate
    [ObservableProperty] private ObservableCollection<double> _frameRates;
    [ObservableProperty] private double _selectedFromFrameRate;
    [ObservableProperty] private double _selectedToFrameRate;

    public BatchConvertWindow? Window { get; set; }

    // View for functions
    public Control ViewRemoveFormatting { get; set; } = new Control();
    public Control ViewOffsetTimeCodes { get; set; } = new Control();
    public Control ViewAdjustDuration { get; set; } = new Control();
    public Control ViewDeleteLines { get; set; } = new Control();
    public Control ViewChangeFrameRate { get; set; } = new Control();

    public bool OkPressed { get; private set; }
    public ScrollViewer FunctionContainer { get; internal set; }

    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;
    private readonly IBatchConverter _batchConverter;
    private CancellationToken _cancellationToken;
    private CancellationTokenSource _cancellationTokenSource;

    public BatchConvertViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;

        BatchItems = new ObservableCollection<BatchConvertItem>();
        BatchFunctions = new ObservableCollection<BatchConvertFunction>();
        TargetFormats = new ObservableCollection<string>(SubtitleFormat.AllSubtitleFormats.Select(p => p.Name));
        TargetEncodings =
            new ObservableCollection<string>(EncodingHelper.GetEncodings().Select(p => p.DisplayName).ToList());
        DeleteLineNumbers = new ObservableCollection<int>();
        TargetFormatName = string.Empty;
        TargetEncoding = string.Empty;
        BatchItemsInfo = string.Empty;
        ProgressText = string.Empty;
        DeleteLinesContains = string.Empty;
        OutputPropertiesText = string.Empty;
        FunctionContainer = new ScrollViewer();
        FrameRates = new ObservableCollection<double>
        {
            23.976,
            24,
            25,
            29.97,
            30,
            48,
            59.94,
            60,
            120,
        };
        AdjustTypes = new ObservableCollection<AdjustDurationDisplay>(AdjustDurationDisplay.ListAll());
        SelectedAdjustType = AdjustTypes.First();

        var activeFunctions = Se.Settings.Tools.BatchConvert.ActiveFunctions;
        BatchFunctions = new ObservableCollection<BatchConvertFunction>(BatchConvertFunction.List(this));
    }

    private void SaveSettings()
    {
        Se.SaveSettings();
    }

    [RelayCommand]
    private void Done()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Convert()
    {
        if (BatchItems.Count == 0)
        {
            ShowStatus("No files to convert");
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;

        foreach (var batchItem in BatchItems)
        {
            batchItem.Status = "-";
        }

        SaveSettings();

        var config = MakeBatchConvertConfig();
        _batchConverter.Initialize(config);
        var start = System.Diagnostics.Stopwatch.GetTimestamp();

        IsProgressVisible = true;
        IsConverting = true;
        AreControlsEnabled = false;
        var unused = Task.Run(async () =>
        {
            var count = 1;
            foreach (var batchItem in BatchItems)
            {
                var countDisplay = count;
                ProgressText = $"Converting {countDisplay:#,###,##0}/{BatchItems.Count:#,###,##0}";
                ProgressValue = countDisplay / (double)BatchItems.Count;
                await _batchConverter.Convert(batchItem);
                count++;

                if (_cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }

            IsProgressVisible = false;
            IsConverting = false;
            AreControlsEnabled = true;

            var end = System.Diagnostics.Stopwatch.GetTimestamp();
            var message =
                $"{BatchItems.Count:#,###,##0} files converted in {ProgressHelper.ToTimeResult(new TimeSpan(end - start).TotalMilliseconds)}";
            if (_cancellationToken.IsCancellationRequested)
            {
                message += " - conversion cancelled by user";
            }

            await ShowStatus(message);
        });
    }

    [RelayCommand]
    private void CancelConvert()
    {
        _cancellationTokenSource.Cancel();
        IsConverting = false;
    }

    [RelayCommand]
    private async Task AddFiles()
    {
        var fileNames = await _fileHelper.PickOpenSubtitleFiles(Window!, "Select files to convert");
        if (fileNames.Length == 0)
        {
            return;
        }

        foreach (var fileName in fileNames)
        {
            var fileInfo = new FileInfo(fileName);
            var subtitle = Subtitle.Parse(fileName);
            var batchItem = new BatchConvertItem(fileName, fileInfo.Length,
                subtitle != null ? subtitle.OriginalFormat.Name : "Unknown", subtitle);
            BatchItems.Add(batchItem);
        }

        MakeBatchItemsInfo();
    }

    private void MakeBatchItemsInfo()
    {
        BatchItemsInfo = $"{BatchItems.Count:#,###,##0} items";
    }

    [RelayCommand]
    private void RemoveSelectedFiles()
    {
    }

    [RelayCommand]
    private void ClearAllFiles()
    {
        BatchItems.Clear();
        MakeBatchItemsInfo();
    }

    [RelayCommand]
    private async Task ShowOutputProperties()
    {
        await _windowService.ShowDialogAsync<BatchConvertSettingsWindow, BatchConvertSettingsViewModel>(Window!,
            vm => { });
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    private BatchConvertConfig MakeBatchConvertConfig()
    {
        var activeFunctions = BatchFunctions.Where(p => p.IsSelected).Select(p => p.Type).ToList();

        return new BatchConvertConfig
        {
            SaveInSourceFolder = Se.Settings.Tools.BatchConvert.SaveInSourceFolder,
            OutputFolder = Se.Settings.Tools.BatchConvert.OutputFolder,
            Overwrite = Se.Settings.Tools.BatchConvert.Overwrite,
            TargetFormatName = SelectedTargetFormat ?? string.Empty,
            TargetEncoding = SelectedTargetEncoding ?? string.Empty,

            RemoveFormatting = new BatchConvertConfig.RemoveFormattingSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.RemoveFormatting),
                RemoveAll = FormattingRemoveAll,
                RemoveItalic = FormattingRemoveItalic,
                RemoveBold = FormattingRemoveBold,
                RemoveUnderline = FormattingRemoveUnderline,
                RemoveColor = FormattingRemoveColors,
                RemoveFontName = FormattingRemoveFontTags,
                RemoveAlignment = FormattingRemoveAlignmentTags,
            },

            OffsetTimeCodes = new BatchConvertConfig.OffsetTimeCodesSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.OffsetTimeCodes),
                Forward = OffsetTimeCodesForward,
                Milliseconds = (long)OffsetTimeCodesTime.TotalMilliseconds,
            },

            AdjustDuration = new BatchConvertConfig.AdjustDurationSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.AdjustDisplayDuration),
                AdjustmentType = SelectedAdjustType.Type,
                Percentage = AdjustPercent,
                FixedMilliseconds = (int)AdjustFixed,
                MaxCharsPerSecond = (double)AdjustRecalculateMaxCharacterPerSecond,
                OptimalCharsPerSecond = (double)AdjustRecalculateOptimalCharacterPerSecond,
            },

            DeleteLines = new BatchConvertConfig.DeleteLinesSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.DeleteLines),
                DeleteXFirst = DeleteXFirstLines,
                DeleteXLast = DeleteXLastLines,
                DeleteContains = DeleteLinesContains,
            },

            ChangeFrameRate = new BatchConvertConfig.ChangeFrameRateSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.ChangeFrameRate),
                FromFrameRate = SelectedFromFrameRate,
                ToFrameRate = SelectedToFrameRate,
            },
        };
    }


    internal void SelectedFunctionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedFunction = SelectedBatchFunction;
        if (selectedFunction == null)
        {
            return;
        }

        FunctionContainer.Content = selectedFunction.View;
    }

    private async Task ShowStatus(string statusText)
    {
        StatusText = statusText;
        await Task.Delay(6_000, _cancellationToken);
        StatusText = string.Empty;
    }
}