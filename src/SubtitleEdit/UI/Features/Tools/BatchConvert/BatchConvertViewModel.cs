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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public partial class BatchConvertViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<BatchConvertItem> _batchItems;
    [ObservableProperty] private BatchConvertItem? _selectedBatchItem;
    [ObservableProperty] private string _batchItemsInfo;
    [ObservableProperty] private ObservableCollection<string> _targetFormats;
    [ObservableProperty] private string? _selectedTargetFormat;
    [ObservableProperty] private ObservableCollection<BatchConvertFunction> _batchFunctions;
    [ObservableProperty] private BatchConvertFunction? _selectedBatchFunction;
    [ObservableProperty] private bool _isProgressVisible;
    [ObservableProperty] private bool _isConverting;
    [ObservableProperty] private bool _areControlsEnabled;
    [ObservableProperty] private string _outputPropertiesText;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private double _progressMaxValue;

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
    [ObservableProperty] private ObservableCollection<double> _fromFrameRates;
    [ObservableProperty] private double _selectedFromFrameRate;
    [ObservableProperty] private ObservableCollection<double> _toFrameRates;
    [ObservableProperty] private double _selectedToFrameRate;
    
    // Change speed
    [ObservableProperty] private double _changeSpeedPercent;
    [ObservableProperty] private bool _changeSpeedAdjustAll;
    [ObservableProperty] private bool _changeSpeedAdjustSelectedLines;
    [ObservableProperty] private bool _changeSpeedAdjustSelectedLinesAndForward;
    
    
    public BatchConvertWindow? Window { get; set; }

    public bool OkPressed { get; private set; }
    public ScrollViewer FunctionContainer { get; internal set; }

    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;
    private readonly IBatchConverter _batchConverter;
    private CancellationToken _cancellationToken;
    private CancellationTokenSource _cancellationTokenSource;
    private List<string> _encodings;

    public BatchConvertViewModel(IWindowService windowService, IFileHelper fileHelper, IBatchConverter batchConverter)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;
        _batchConverter = batchConverter;

        BatchItems = new ObservableCollection<BatchConvertItem>();
        BatchFunctions = new ObservableCollection<BatchConvertFunction>();
        TargetFormats = new ObservableCollection<string>(SubtitleFormat.AllSubtitleFormats.Select(p => p.Name));
        DeleteLineNumbers = new ObservableCollection<int>();
        BatchItemsInfo = string.Empty;
        ProgressText = string.Empty;
        DeleteLinesContains = string.Empty;
        OutputPropertiesText = string.Empty;
        StatusText = string.Empty;
        FunctionContainer = new ScrollViewer();
        FromFrameRates = new ObservableCollection<double>
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
        ToFrameRates = new ObservableCollection<double>
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

        BatchFunctions = new ObservableCollection<BatchConvertFunction>(BatchConvertFunction.List(this));

        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;

        _encodings = EncodingHelper.GetEncodings().Select(p => p.DisplayName).ToList();
        
        LoadSettings();
    }

    private void LoadSettings()
    {
        var targetFormat = TargetFormats.FirstOrDefault(p => p == Se.Settings.Tools.BatchConvert.TargetFormat);
        if (targetFormat == null)
        {
            targetFormat = TargetFormats.First();
        }
        SelectedTargetFormat = targetFormat;

        UpdateOutputProperties();
    }

    private void UpdateOutputProperties()
    {
        var targetEncoding = 
            _encodings.FirstOrDefault(p => p == Se.Settings.Tools.BatchConvert.TargetEncoding);
        if (targetEncoding == null)
        {
            targetEncoding = _encodings.First();
            Se.Settings.Tools.BatchConvert.TargetEncoding = targetEncoding;
        }

        if (!Se.Settings.Tools.BatchConvert.SaveInSourceFolder &&
            string.IsNullOrWhiteSpace(Se.Settings.Tools.BatchConvert.OutputFolder))
        {
            Se.Settings.Tools.BatchConvert.SaveInSourceFolder = true;
        }
        
        string text;
        if (Se.Settings.Tools.BatchConvert.SaveInSourceFolder)
        {
            text = string.Format("Outputfolder: {0}", "Source folder");
        }
        else
        {
            text = string.Format("Outputfolder: {0}", Se.Settings.Tools.BatchConvert.OutputFolder);
        }

        text += Environment.NewLine +
                "Encoding: " + Se.Settings.Tools.BatchConvert.TargetEncoding + ", overwrite: " +
                Se.Settings.Tools.BatchConvert.Overwrite;
        
        OutputPropertiesText = text;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.BatchConvert.TargetFormat = SelectedTargetFormat ?? TargetFormats.First();
        Se.SaveSettings();
    }

    [RelayCommand]
    private void Done()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private async Task Convert()
    {
        if (BatchItems.Count == 0)
        {
            await ShowStatus("No files to convert");
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
        ProgressMaxValue = BatchItems.Count;
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
        }, _cancellationToken);
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
        UpdateOutputProperties();
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
            TargetEncoding = Se.Settings.Tools.BatchConvert.TargetEncoding,

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
        await Task.Delay(6000, _cancellationToken).ConfigureAwait(false);
        StatusText = string.Empty;
    }
}