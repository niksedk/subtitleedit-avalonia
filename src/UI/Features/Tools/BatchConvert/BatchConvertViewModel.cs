using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PromptTextBox;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;

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
    [ObservableProperty] private string _outputFolderLabel;
    [ObservableProperty] private string _outputFolderLinkLabel;
    [ObservableProperty] private string _outputEncodingLabel;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private double _progressMaxValue;
    [ObservableProperty] private string _actionsSelected;
    [ObservableProperty] private bool _isTargetFormatSettingsVisible;
    [ObservableProperty] private ObservableCollection<string> _filterItems;
    [ObservableProperty] private string? _selectedFilterItem;
    [ObservableProperty] private string _filterText;
    [ObservableProperty] private bool _isFilterTextVisible;

    // Add formatting
    [ObservableProperty] private bool _formattingAddItalic;
    [ObservableProperty] private bool _formattingAddBold;
    [ObservableProperty] private bool _formattingAddUnderline;
    [ObservableProperty] private bool _formattingAddAlignmentTag;
    [ObservableProperty] private bool _formattingAddColor;
    [ObservableProperty] private Color _formattingAddColorValue;
    [ObservableProperty] private ObservableCollection<DisplayAlignment> _alignmentTagOptions;
    [ObservableProperty] private DisplayAlignment? _selectedAlignmentTagOption;

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
    [ObservableProperty] private string _deleteActorsOrStyles;

    // Change frame rate
    [ObservableProperty] private ObservableCollection<double> _fromFrameRates;
    [ObservableProperty] private double _selectedFromFrameRate;
    [ObservableProperty] private ObservableCollection<double> _toFrameRates;
    [ObservableProperty] private double _selectedToFrameRate;

    // Change speed
    [ObservableProperty] private double _changeSpeedPercent;

    // Change casing
    [ObservableProperty] private bool _normalCasing;
    [ObservableProperty] private bool _normalCasingFixNames;
    [ObservableProperty] private bool _normalCasingOnlyUpper;
    [ObservableProperty] private bool _fixNamesOnly;
    [ObservableProperty] private bool _allUppercase;
    [ObservableProperty] private bool _allLowercase;

    // Auto translate
    public ObservableCollection<IAutoTranslator> AutoTranslators { get; set; }
    [ObservableProperty] private IAutoTranslator _selectedAutoTranslator;
    [ObservableProperty] private ObservableCollection<TranslationPair> _sourceLanguages = new();
    [ObservableProperty] private TranslationPair? _selectedSourceLanguage;
    [ObservableProperty] private ObservableCollection<TranslationPair> _targetLanguages = new();
    [ObservableProperty] private TranslationPair? _selectedTargetLanguage;
    [ObservableProperty] private string _autoTranslateModelText;
    [ObservableProperty] private bool _autoTranslateModelIsVisible;
    [ObservableProperty] private bool _autoTranslateModelBrowseIsVisible;

    // Fix common errors
    [ObservableProperty] private FixCommonErrors.ProfileDisplayItem? _fixCommonErrorsProfile;

    // Merge lines with same text
    [ObservableProperty] private int _mergeSameTextMaxMillisecondsBetweenLines;
    [ObservableProperty] private bool _mergeSameTextIncludeIncrementingLines;

    // Merge lines with same time codes
    [ObservableProperty] private int _mergeSameTimeMaxMillisecondsDifference;
    [ObservableProperty] private bool _mergeSameTimeMergeDialog;
    [ObservableProperty] private bool _mergeSameTimeAutoBreak;

    // Fix right-to-left
    [ObservableProperty] private bool _rtlFixViaUniCode;
    [ObservableProperty] private bool _rtlRemoveUniCode;
    [ObservableProperty] private bool _rtlReverseStartEnd;

    // Bride gaps
    [ObservableProperty] private int _bridgeGapsSmallerThanMs;
    [ObservableProperty] private int _bridgeGapsMinGapMs;
    [ObservableProperty] private int _bridgeGapsPercentForLeft;

    // Split/break long lines
    [ObservableProperty] private bool _splitBreakSplitLongLines;
    [ObservableProperty] private int _splitBreakSingleLineMaxLength;
    [ObservableProperty] private int _splitBreakMaxNumberOfLines;
    [ObservableProperty] private bool _splitBreakRebalanceLongLines;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public ScrollViewer FunctionContainer { get; internal set; }

    private List<BatchConvertItem> _allBatchItems;
    private readonly System.Timers.Timer _filesTimer;
    private bool _isFilesDirty;
    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;
    private readonly IBatchConverter _batchConverter;
    private CancellationToken _cancellationToken;
    private CancellationTokenSource _cancellationTokenSource;
    private List<string> _encodings;
    private const int StatisticsNumberOfLinesToShow = 10;
    private List<string> _targetFormatsWithSettings;

    public BatchConvertViewModel(IWindowService windowService, IFileHelper fileHelper, IBatchConverter batchConverter, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;
        _batchConverter = batchConverter;
        _folderHelper = folderHelper;

        BatchItems = new ObservableCollection<BatchConvertItem>();
        _allBatchItems = new List<BatchConvertItem>();
        BatchFunctions = new ObservableCollection<BatchConvertFunction>();

        TargetFormats = new ObservableCollection<string>(SubtitleFormat.AllSubtitleFormats.Select(p => p.Name))
        {
            BatchConverter.FormatAyato,
            BatchConverter.FormatBdnXml,
            BatchConverter.FormatBluRaySup,
            BatchConverter.FormatCavena890,
            BatchConverter.FormatCustomTextFormat,
            BatchConverter.FormatDCinemaInterop,
            BatchConverter.FormatDCinemaSmpte2014,
            BatchConverter.FormatDostImage,
            BatchConverter.FormatFcpImage,
            BatchConverter.FormatImagesWithTimeCodesInFileName,
            BatchConverter.FormatPac,
            BatchConverter.FormatPlainText,
            BatchConverter.FormatVobSub
        };

        FilterItems =
        [
            Se.Language.General.AllFiles,
            Se.Language.Tools.BatchConvert.FileNameContainsDotDotDot,
            Se.Language.Tools.BatchConvert.TrackLanguageContainsDotDotDot,
        ];
        SelectedFilterItem = FilterItems.FirstOrDefault();
        FilterText = string.Empty;

        DeleteLineNumbers = new ObservableCollection<int>();
        BatchItemsInfo = string.Empty;
        ProgressText = string.Empty;
        ActionsSelected = string.Empty;
        DeleteLinesContains = string.Empty;
        OutputFolderLabel = string.Empty;
        OutputFolderLinkLabel = string.Empty;
        OutputEncodingLabel = string.Empty;
        StatusText = string.Empty;
        DeleteActorsOrStyles = string.Empty;
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

        AlignmentTagOptions = new ObservableCollection<DisplayAlignment>(DisplayAlignment.GetAll());
        SelectedAlignmentTagOption = AlignmentTagOptions[1];

        BatchFunctions = new ObservableCollection<BatchConvertFunction>(BatchConvertFunction.List(this));

        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;

        _encodings = EncodingHelper.GetEncodings().Select(p => p.DisplayName).ToList();

        AutoTranslateModelText = string.Empty;
        AutoTranslators = new ObservableCollection<IAutoTranslator>
        {
            new OllamaTranslate(),
            new LibreTranslate(),
            new LmStudioTranslate(),
            new NoLanguageLeftBehindServe(),
            new NoLanguageLeftBehindApi(),
        };
        SelectedAutoTranslator = AutoTranslators[0];
        OnAutoTranslatorChanged();
        UpdateAutoTranslateLanguages();

        SelectedFromFrameRate = FromFrameRates[0];
        SelectedToFrameRate = ToFrameRates[1];

        ChangeSpeedPercent = 100;

        FixCommonErrorsProfile = LoadDefaultProfile();

        _targetFormatsWithSettings = new List<string>
        {
            BatchConverter.FormatBdnXml,
            BatchConverter.FormatBluRaySup,
            BatchConverter.FormatCustomTextFormat,
            BatchConverter.FormatDostImage,
            BatchConverter.FormatFcpImage,
            BatchConverter.FormatImagesWithTimeCodesInFileName,
            BatchConverter.FormatVobSub,
            new AdvancedSubStationAlpha().Name,
        };

        LoadSettings();
        FilterComboBoxChanged();
        _filesTimer = new System.Timers.Timer(250);
        _filesTimer.Elapsed += (sender, args) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                _filesTimer.Stop();

                if (_isFilesDirty)
                {
                    _isFilesDirty = false;
                    UpdateFilteredFiles();
                }

                _filesTimer.Start();
            });
        };
        _filesTimer.Start();
    }

    private void UpdateFilteredFiles()
    {
        BatchItems.Clear();
        if (SelectedFilterItem == Se.Language.Tools.BatchConvert.FileNameContainsDotDotDot && !string.IsNullOrEmpty(FilterText))
        {
            foreach (var item in _allBatchItems)
            {
                if (item.FileName.Contains(FilterText, StringComparison.InvariantCultureIgnoreCase))
                {
                    BatchItems.Add(item);
                }
            }
        }
        else if (SelectedFilterItem == Se.Language.Tools.BatchConvert.TrackLanguageContainsDotDotDot && !string.IsNullOrEmpty(FilterText))
        {
            foreach (var item in _allBatchItems)
            {
                if (item.Format.Contains(FilterText, StringComparison.InvariantCultureIgnoreCase))
                {
                    BatchItems.Add(item);
                }
            }
        }
        else
        {
            BatchItems.AddRange(_allBatchItems);
        }
    }

    private static FixCommonErrors.ProfileDisplayItem LoadDefaultProfile()
    {
        var profiles = Se.Settings.Tools.FixCommonErrors.Profiles;
        var displayProfiles = new List<FixCommonErrors.ProfileDisplayItem>();
        var defaultName = Se.Settings.Tools.FixCommonErrors.LastProfileName;
        var allFixRules = FixCommonErrorsViewModel.MakeDefaultRules();

        foreach (var setting in profiles)
        {
            var profile = new FixCommonErrors.ProfileDisplayItem
            {
                Name = setting.ProfileName,
                FixRules = new ObservableCollection<FixRuleDisplayItem>(allFixRules.Select(rule => new FixRuleDisplayItem(rule)
                {
                    IsSelected = setting.SelectedRules.Contains(rule.FixCommonErrorFunctionName)
                }))
            };

            if (defaultName == profile.Name)
            {
                return profile;
            }

            displayProfiles.Add(profile);
        }

        return displayProfiles.First();
    }

    private void UpdateAutoTranslateLanguages()
    {
        SourceLanguages.Clear();
        SourceLanguages.Add(new TranslationPair(" - " + Se.Language.General.Autodetect + " - ", "auto"));
        foreach (var language in SelectedAutoTranslator.GetSupportedSourceLanguages())
        {
            SourceLanguages.Add(language);
        }

        TargetLanguages.Clear();
        foreach (var language in SelectedAutoTranslator.GetSupportedTargetLanguages())
        {
            TargetLanguages.Add(language);
        }
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.BatchConvert.TargetFormat = SelectedTargetFormat ?? TargetFormats.First();

        Se.Settings.Tools.BatchConvert.LastFilterItem = SelectedFilterItem ?? string.Empty;

        Se.Settings.Tools.BatchConvert.AdjustVia = SelectedAdjustType.Name;
        Se.Settings.Tools.BatchConvert.AdjustMaxCps = AdjustRecalculateMaxCharacterPerSecond;
        Se.Settings.Tools.BatchConvert.AdjustOptimalCps = AdjustRecalculateOptimalCharacterPerSecond;
        Se.Settings.Tools.BatchConvert.AdjustDurationFixedMilliseconds = (int)AdjustFixed;
        Se.Settings.Tools.BatchConvert.AdjustDurationSeconds = AdjustSeconds;
        Se.Settings.Tools.BatchConvert.AdjustDurationPercentage = AdjustPercent;

        Se.Settings.Tools.BatchConvert.AutoTranslateEngine = SelectedAutoTranslator.Name;
        Se.Settings.Tools.BatchConvert.AutoTranslateSourceLanguage = SelectedSourceLanguage?.TwoLetterIsoLanguageName ?? "auto";
        Se.Settings.Tools.BatchConvert.AutoTranslateTargetLanguage = SelectedTargetLanguage?.TwoLetterIsoLanguageName ?? "en";

        // Change casing
        if (NormalCasing)
        {
            Se.Settings.Tools.BatchConvert.ChangeCasingType = "Normal";
        }
        else if (FixNamesOnly)
        {
            Se.Settings.Tools.BatchConvert.ChangeCasingType = "FixNamesOnly";
        }
        else if (AllUppercase)
        {
            Se.Settings.Tools.BatchConvert.ChangeCasingType = "AllUppercase";
        }
        else if (AllLowercase)
        {
            Se.Settings.Tools.BatchConvert.ChangeCasingType = "AllLowercase";
        }

        // Fix right-to-left
        if (RtlFixViaUniCode)
        {
            Se.Settings.Tools.BatchConvert.FixRtlMode = "FixViaUnicode";
        }
        else if (RtlRemoveUniCode)
        {
            Se.Settings.Tools.BatchConvert.FixRtlMode = "RemoveUnicode";
        }
        else
        {
            Se.Settings.Tools.BatchConvert.FixRtlMode = "ReverseStartEnd";
        }

        Se.SaveSettings();
    }

    private void LoadSettings()
    {
        var targetFormat = TargetFormats.FirstOrDefault(p => p == Se.Settings.Tools.BatchConvert.TargetFormat);
        if (targetFormat == null)
        {
            targetFormat = TargetFormats.First();
        }

        SelectedTargetFormat = targetFormat;

        SelectedAdjustType = AdjustTypes.First();
        foreach (var adjustType in AdjustTypes)
        {
            if (adjustType.Name == Se.Settings.Tools.BatchConvert.AdjustVia)
            {
                SelectedAdjustType = adjustType;
                break;
            }
        }

        var filterItem = FilterItems.FirstOrDefault(p => p == Se.Settings.Tools.BatchConvert.LastFilterItem);
        if (filterItem != null)
        {
            SelectedFilterItem = filterItem;
        }

        AdjustRecalculateMaxCharacterPerSecond = Se.Settings.Tools.BatchConvert.AdjustMaxCps;
        AdjustRecalculateOptimalCharacterPerSecond = Se.Settings.Tools.BatchConvert.AdjustOptimalCps;
        AdjustFixed = Se.Settings.Tools.BatchConvert.AdjustDurationFixedMilliseconds;
        AdjustSeconds = Se.Settings.Tools.BatchConvert.AdjustDurationSeconds;
        AdjustPercent = Se.Settings.Tools.BatchConvert.AdjustDurationPercentage;

        var translator = AutoTranslators.FirstOrDefault(p => p.Name == Se.Settings.Tools.BatchConvert.AutoTranslateEngine);
        if (translator != null)
        {
            SelectedAutoTranslator = translator;
        }

        var sourceLanguage = SourceLanguages.FirstOrDefault(p => p.TwoLetterIsoLanguageName == Se.Settings.Tools.BatchConvert.AutoTranslateSourceLanguage);
        if (sourceLanguage != null)
        {
            SelectedSourceLanguage = sourceLanguage;
        }

        var defaultTarget = AutoTranslateViewModel.EvaluateDefaultTargetLanguageCode(string.Empty, SelectedSourceLanguage?.Code ?? string.Empty);
        SelectedTargetLanguage = TargetLanguages.FirstOrDefault(p => p.TwoLetterIsoLanguageName == defaultTarget);
        var targetLanguage = TargetLanguages.FirstOrDefault(p => p.TwoLetterIsoLanguageName == Se.Settings.Tools.BatchConvert.AutoTranslateTargetLanguage);
        if (targetLanguage != null)
        {
            SelectedTargetLanguage = targetLanguage;
        }

        // Change casing
        if (Se.Settings.Tools.BatchConvert.ChangeCasingType == "Normal")
        {
            NormalCasing = true;
        }
        else if (Se.Settings.Tools.BatchConvert.ChangeCasingType == "FixNamesOnly")
        {
            FixNamesOnly = true;
        }
        else if (Se.Settings.Tools.BatchConvert.ChangeCasingType == "AllUppercase")
        {
            AllUppercase = true;
        }
        else if (Se.Settings.Tools.BatchConvert.ChangeCasingType == "AllLowercase")
        {
            AllLowercase = true;
        }

        NormalCasingFixNames = Se.Settings.Tools.BatchConvert.NormalCasingFixNames;
        NormalCasingOnlyUpper = Se.Settings.Tools.BatchConvert.NormalCasingOnlyUpper;

        UpdateOutputProperties();

        MergeSameTextMaxMillisecondsBetweenLines = Se.Settings.Tools.MergeSameText.MaxMillisecondsBetweenLines;
        MergeSameTextIncludeIncrementingLines = Se.Settings.Tools.MergeSameText.IncludeIncrementingLines;

        MergeSameTimeMaxMillisecondsDifference = Se.Settings.Tools.MergeSameTimeCode.MaxMillisecondsDifference;
        MergeSameTimeMergeDialog = Se.Settings.Tools.MergeSameTimeCode.MergeDialog;
        MergeSameTimeAutoBreak = Se.Settings.Tools.MergeSameTimeCode.AutoBreak;

        if (Se.Settings.Tools.BatchConvert.FixRtlMode == "FixViaUnicode")
        {
            RtlFixViaUniCode = true;
        }
        else if (Se.Settings.Tools.BatchConvert.FixRtlMode == "RemoveUnicode")
        {
            RtlRemoveUniCode = true;
        }
        else
        {
            RtlReverseStartEnd = true;
        }

        BridgeGapsSmallerThanMs = Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanMs;
        BridgeGapsMinGapMs = Se.Settings.Tools.BridgeGaps.MinGapMs;
        BridgeGapsPercentForLeft = Se.Settings.Tools.BridgeGaps.PercentForLeft;

        SplitBreakSingleLineMaxLength = Se.Settings.General.SubtitleLineMaximumLength;
        SplitBreakMaxNumberOfLines = Se.Settings.General.MaxNumberOfLines;
        SplitBreakSplitLongLines = Se.Settings.Tools.SplitRebalanceLongLinesSplit;
        SplitBreakRebalanceLongLines = Se.Settings.Tools.SplitRebalanceLongLinesRebalance;
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

        if (Se.Settings.Tools.BatchConvert.SaveInSourceFolder)
        {
            OutputFolderLinkLabel = string.Empty;
            OutputFolderLabel = Se.Language.Tools.BatchConvert.OutputFolderSource;
        }
        else
        {
            OutputFolderLinkLabel = string.Format(Se.Language.Tools.BatchConvert.OutputFolderX, Se.Settings.Tools.BatchConvert.OutputFolder);
            OutputFolderLabel = string.Empty;
        }

        OutputEncodingLabel = string.Format(Se.Language.Tools.BatchConvert.EncodingXOverwriteY,
            Se.Settings.Tools.BatchConvert.TargetEncoding,
            Se.Settings.Tools.BatchConvert.Overwrite);
    }

    [RelayCommand]
    private async Task ShowRemoveTextForHearingImpairedSettings()
    {
        _ = await _windowService
            .ShowDialogAsync<RemoveTextForHearingImpairedWindow, RemoveTextForHearingImpairedViewModel>(
                Window!, vm => { vm.Initialize(new Subtitle()); });
    }

    [RelayCommand]
    private void Done()
    {
        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        _cancellationTokenSource.Cancel();
        IsConverting = false;
        foreach (var batchItem in BatchItems)
        {
            if (batchItem.Status != "-" &&
                batchItem.Status != Se.Language.General.Converted &&
                batchItem.Status != Se.Language.General.Error)
            {
                batchItem.Status = Se.Language.General.Cancelled;
            }
        }

        ProgressText = string.Empty;
    }

    [RelayCommand]
    private async Task Convert()
    {
        if (BatchItems.Count == 0)
        {
            await ShowStatus(Se.Language.General.NoFilesToConvert);
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
        var start = DateTime.UtcNow.Ticks;

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
                ProgressText = string.Format(Se.Language.General.ConvertingXofYDotDoDot, countDisplay, BatchItems.Count);
                ProgressValue = countDisplay / (double)BatchItems.Count;
                await _batchConverter.Convert(batchItem, _cancellationToken);
                count++;

                if (_cancellationToken.IsCancellationRequested)
                {
                    ProgressText = string.Empty;
                    break;
                }
            }

            IsProgressVisible = false;
            IsConverting = false;
            AreControlsEnabled = true;
            ProgressText = string.Empty;

            var end = DateTime.UtcNow.Ticks;
            var elapsed = new TimeSpan(end - start).TotalMilliseconds;
            var message = string.Format(Se.Language.General.XFilesConvertedInY, BatchItems.Count, elapsed);
            if (_cancellationToken.IsCancellationRequested)
            {
                message += Environment.NewLine + Se.Language.General.ConversionCancelledByUser;
            }

            await ShowStatus(message);
        }, _cancellationToken);
    }

    [RelayCommand]
    private void ChangeSpeedSetFromDropFrameValue()
    {
        ChangeSpeedPercent = 100.1001;
    }

    [RelayCommand]
    private void ChangeSpeedSetToDropFrameValue()
    {
        ChangeSpeedPercent = 99.9889;
    }

    [RelayCommand]
    private async Task ShowFixCommonRules()
    {
        if (Window == null || FixCommonErrorsProfile == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<BatchConvertFixCommonErrorsSettingsWindow, BatchConvertFixCommonErrorsSettingsViewModel>(Window!,
            vm => { vm.Initialize(FixCommonErrorsProfile); });

        if (result.OkPressed && result.SelectedProfile != null)
        {
            FixCommonErrorsProfile = result.SelectedProfile;
        }
    }

    [RelayCommand]
    private async Task Statistics()
    {
        if (Window == null)
        {
            return;
        }

        var stats = CalculateGeneralStatistics();
        var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window!,
            vm => { vm.Initialize(Se.Language.File.Statistics.Title, stats, 1000, 600, false, true); });
    }

    [RelayCommand]
    private void CancelConvert()
    {
        _cancellationTokenSource.Cancel();
        IsConverting = false;
    }

    [RelayCommand]
    private async Task OpenOutputFolder()
    {
        await _folderHelper.OpenFolder(Window!, Se.Settings.Tools.BatchConvert.OutputFolder);
    }

    [RelayCommand]
    private async Task ShowTargetFormatSettings()
    {
        var targetFormat = SelectedTargetFormat;
        if (targetFormat == null)
        {
            return;
        }

        if (Window == null)
        {
            return;
        }

        if (targetFormat == BatchConverter.FormatCustomTextFormat)
        {
            var subtitles = new List<SubtitleLineViewModel>();
            var p = new Paragraph("This is a sample text", 0, 1000);
            subtitles.Add(new SubtitleLineViewModel(p, new SubRip()));

            var result = await _windowService.ShowDialogAsync<ExportCustomTextFormatWindow, ExportCustomTextFormatViewModel>(Window,
                vm =>
                {
                    vm.Initialize(subtitles, string.Empty, string.Empty, true);
                });
            return;
        }

        IExportHandler? exportHandler = null;

        if (targetFormat == BatchConverter.FormatBdnXml)
        {
            exportHandler = new ExportHandlerBdnXml();
        }
        else if (targetFormat == BatchConverter.FormatBluRaySup)
        {
            exportHandler = new ExportHandlerBluRaySup();
        }
        else if (targetFormat == BatchConverter.FormatDostImage)
        {
            exportHandler = new ExportHandlerDost();
        }
        else if (targetFormat == BatchConverter.FormatFcpImage)
        {
            exportHandler = new ExportHandlerFcp();
        }
        else if (targetFormat == BatchConverter.FormatImagesWithTimeCodesInFileName)
        {
            exportHandler = new ExportHandlerImagesWithTimeCode();
        }
        else if (targetFormat == BatchConverter.FormatVobSub)
        {
            exportHandler = new ExportHandlerVobSub();
        }

        if (exportHandler != null)
        {
            var result = await _windowService.ShowDialogAsync<ExportImageBasedWindow, ExportImageBasedViewModel>(Window, vm =>
            {
                var subtitles = new ObservableCollection<SubtitleLineViewModel>();
                var p = new Paragraph("This is a sample text", 0, 1000);
                subtitles.Add(new SubtitleLineViewModel(p, new SubRip()));
                vm.Initialize(exportHandler, subtitles, string.Empty, string.Empty, true);
            });
            return;
        }
    }

    [RelayCommand]
    private async Task AddFiles()
    {
        var fileNames = await _fileHelper.PickOpenSubtitleFiles(Window!, Se.Language.General.SelectFilesToConvert);
        if (fileNames.Length == 0)
        {
            return;
        }

        foreach (var fileName in fileNames)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            var fileInfo = new FileInfo(fileName);

            Subtitle? subtitle = null;
            var format = Se.Language.General.Unknown;
            if (ext == ".sup" && FileUtil.IsBluRaySup(fileName))
            {
                format = BatchConverter.FormatBluRaySup;
            }

            if (ext == ".sub" && FileUtil.IsVobSub(fileName))
            {
                format = BatchConverter.FormatVobSub;
            }

            if (ext == ".mkv" || ext == ".mks")
            {
                using (var matroska = new MatroskaFile(fileName))
                {
                    if (matroska.IsValid)
                    {
                        var codecToFormat = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            { "S_VOBSUB", "VobSub" },
                            { "S_HDMV/PGS", "PGS" },
                            { "S_TEXT/UTF8", "SRT" },
                            { "S_TEXT/SSA", "SSA" },
                            { "S_TEXT/ASS", "ASS" },
                            { "S_DVBSUB", "DvdSub" },
                            { "S_HDMV/TEXTST", "TextST" }
                        };

                        var tracksByFormat = new Dictionary<string, List<string>>();

                        foreach (var track in matroska.GetTracks(true))
                        {
                            if (codecToFormat.TryGetValue(track.CodecId, out var formatName))
                            {
                                if (!tracksByFormat.ContainsKey(formatName))
                                {
                                    tracksByFormat[formatName] = new List<string>();
                                }

                                tracksByFormat[formatName].Add(MakeMkvTrackInfoString(track));

                                format = $"Matroska/{formatName} - {MakeMkvTrackInfoString(track)}";
                                var matroskaBatchItem = new BatchConvertItem(fileName, fileInfo.Length, format, subtitle);
                                matroskaBatchItem.LanguageCode = track.Language;
                                matroskaBatchItem.TrackNumber = track.TrackNumber.ToString(CultureInfo.InvariantCulture);
                                BatchItems.Add(matroskaBatchItem);
                                _allBatchItems.Add(matroskaBatchItem);
                            }
                        }

                        if (tracksByFormat.Count == 0)
                        {
                            format = "No subtitle tracks";
                        }

                        continue;
                    }
                }
            }
            else if (ext == ".mp4" || ext == ".m4v" || ext == ".m4s")
            {
                var mp4Files = new List<string>();
                var mp4Parser = new MP4Parser(fileName);
                var mp4SubtitleTracks = mp4Parser.GetSubtitleTracks();
                if (mp4Parser.VttcSubtitle?.Paragraphs.Count > 0)
                {
                    var name = "MP4/WebVTT - " + mp4Parser.VttcLanguage;
                    mp4Files.Add(name);
                    var mp4BatchItem = new BatchConvertItem(fileName, fileInfo.Length, name, subtitle);
                    mp4BatchItem.LanguageCode = mp4Parser.VttcLanguage;
                    BatchItems.Add(mp4BatchItem);
                    _allBatchItems.Add(mp4BatchItem);
                }

                foreach (var track in mp4SubtitleTracks)
                {
                    if (track.Mdia.IsTextSubtitle || track.Mdia.IsClosedCaption)
                    {
                        var name = $"MP4/#{mp4SubtitleTracks.IndexOf(track)} {track.Mdia.HandlerType} - {track.Mdia.Mdhd.Iso639ThreeLetterCode ?? track.Mdia.Mdhd.LanguageString}";
                        mp4Files.Add(name);
                        var mp4BatchItem = new BatchConvertItem(fileName, fileInfo.Length, name, subtitle);
                        mp4BatchItem.LanguageCode = track.Mdia.Mdhd.Iso639ThreeLetterCode ?? track.Mdia.Mdhd.LanguageString;
                        BatchItems.Add(mp4BatchItem);
                        _allBatchItems.Add(mp4BatchItem);
                    }
                }

                if (mp4Files.Count <= 0)
                {
                    format = "No subtitle tracks";
                }

                continue;
            }
            else if ((ext == ".ts" || ext == ".m2ts" || ext == ".mts" || ext == ".mpg" || ext == ".mpeg") &&
                     (FileUtil.IsTransportStream(fileName) || FileUtil.IsM2TransportStream(fileName)))
            {
                format = "Transport Stream";
                var tsBatchItem = new BatchConvertItem(fileName, fileInfo.Length, format, subtitle);
                BatchItems.Add(tsBatchItem);
                _allBatchItems.Add(tsBatchItem);
                continue;
            }

            if (format == Se.Language.General.Unknown && fileInfo.Length < 200_000)
            {
                subtitle = Subtitle.Parse(fileName);
                if (subtitle != null)
                {
                    format = subtitle.OriginalFormat.Name;
                }
            }

            if (format == Se.Language.General.Unknown)
            {
                foreach (var f in SubtitleFormat.GetBinaryFormats(false))
                {
                    if (f.IsMine(null, fileName))
                    {
                        subtitle = new Subtitle();
                        f.LoadSubtitle(subtitle, null, fileName);
                        subtitle.OriginalFormat = f;
                        format = f.Name;
                        break; // format found, exit the loop
                    }
                }
            }

            if (format == Se.Language.General.Unknown && fileInfo.Length < 300_000)
            {
                subtitle = Subtitle.Parse(fileName);
                if (subtitle != null)
                {
                    format = subtitle.OriginalFormat.Name;
                }
            }

            var batchItem = new BatchConvertItem(fileName, fileInfo.Length, format, subtitle);
            BatchItems.Add(batchItem);
            _allBatchItems.Add(batchItem);
        }

        MakeBatchItemsInfo();
        _isFilesDirty = true;
    }

    private static string MakeMkvTrackInfoString(MatroskaTrackInfo track)
    {
        return (track.Language ?? "undefined") + (track.IsForced ? " (forced)" : string.Empty) + " #" + track.TrackNumber;
    }

    private void MakeBatchItemsInfo()
    {
        if (BatchItems.Count == 0)
        {
            BatchItemsInfo = string.Empty;
        }
        else if (BatchItems.Count == 1)
        {
            BatchItemsInfo = Se.Language.General.OneFile;
        }
        else
        {
            BatchItemsInfo = string.Format(Se.Language.General.XFiles, BatchItems.Count);
        }
    }

    [RelayCommand]
    private async Task AutoTranslateBrowseModel()
    {
        var result = await _windowService.ShowDialogAsync<PickOllamaModelWindow, PickOllamaModelViewModel>(Window!,
            vm => { vm.Initialize(Se.Language.General.PickOllamaModel, Se.Settings.AutoTranslate.OllamaModel, Se.Settings.AutoTranslate.OllamaUrl); });

        if (result is { OkPressed: true, SelectedModel: not null })
        {
            Se.Settings.AutoTranslate.OllamaModel = result.SelectedModel;
            SaveSettings();
        }
    }

    [RelayCommand]
    private async Task RemoveSelectedFiles()
    {
        var selected = SelectedBatchItem;
        if (selected == null || Window == null)
        {
            return;
        }

        if (Se.Settings.General.PromptDeleteLines)
        {
            var result = await MessageBox.Show(
                Window,
                Se.Language.General.Remove,
                Se.Language.General.RemoveSelectedFile,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        var idx = BatchItems.IndexOf(selected);
        BatchItems.Remove(selected);
        if (BatchItems.Count > 0)
        {
            if (idx >= BatchItems.Count)
            {
                idx = BatchItems.Count - 1;
            }

            SelectedBatchItem = BatchItems[idx];
        }

        _allBatchItems.Remove(selected);

        MakeBatchItemsInfo();
    }

    [RelayCommand]
    private void ClearAllFiles()
    {
        BatchItems.Clear();
        _allBatchItems.Clear();
        MakeBatchItemsInfo();
    }

    [RelayCommand]
    private async Task ShowOutputProperties()
    {
        await _windowService.ShowDialogAsync<BatchConvertSettingsWindow, BatchConvertSettingsViewModel>(Window!);
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

            AdjustDuration = new BatchConvertConfig.AdjustDurationSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.AdjustDisplayDuration),
                AdjustmentType = SelectedAdjustType.Type,
                Percentage = AdjustPercent,
                FixedMilliseconds = (int)AdjustFixed,
                MaxCharsPerSecond = (double)AdjustRecalculateMaxCharacterPerSecond,
                OptimalCharsPerSecond = (double)AdjustRecalculateOptimalCharacterPerSecond,
                Seconds = (double)AdjustSeconds,
            },

            AutoTranslate = new BatchConvertConfig.AutoTranslateSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.AutoTranslate),
                Translator = SelectedAutoTranslator,
                SourceLanguage = SelectedSourceLanguage ?? SourceLanguages.First(),
                TargetLanguage = SelectedTargetLanguage ?? TargetLanguages.First(),
            },

            ChangeCasing = new BatchConvertConfig.ChangeCasingSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.ChangeCasing),

                NormalCasing = NormalCasing,
                NormalCasingOnlyUpper = NormalCasingOnlyUpper,
                NormalCasingFixNames = NormalCasingFixNames,
                FixNamesOnly = FixNamesOnly,
                AllLowercase = AllLowercase,
                AllUppercase = AllUppercase,
            },

            ChangeSpeed = new BatchConvertConfig.ChangeSpeedSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.ChangeSpeed),
                SpeedPercent = ChangeSpeedPercent,
            },

            ChangeFrameRate = new BatchConvertConfig.ChangeFrameRateSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.ChangeFrameRate),
                FromFrameRate = SelectedFromFrameRate,
                ToFrameRate = SelectedToFrameRate,
            },

            DeleteLines = new BatchConvertConfig.DeleteLinesSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.DeleteLines),
                DeleteXFirst = DeleteXFirstLines,
                DeleteXLast = DeleteXLastLines,
                DeleteContains = DeleteLinesContains,
                DeleteActorsOrStyles = DeleteActorsOrStyles,
            },

            FixCommonErrors = new BatchConvertConfig.FixCommonErrorsSettings2
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.FixCommonErrors),
                Profile = FixCommonErrorsProfile,
            },

            MergeLinesWithSameTexts = new BatchConvertConfig.MergeLinesWithSameTextsSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.MergeLinesWithSameText),
                IncludeIncrementingLines = MergeSameTextIncludeIncrementingLines,
                MaxMillisecondsBetweenLines = MergeSameTextMaxMillisecondsBetweenLines,
            },

            MergeLinesWithSameTimeCodes = new BatchConvertConfig.MergeLinesWithSameTimeCodesSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.MergeLinesWithSameTimeCodes),
                MaxMillisecondsDifference = MergeSameTextMaxMillisecondsBetweenLines,
                MergeDialog = MergeSameTimeMergeDialog,
                AutoBreak = MergeSameTimeAutoBreak,
            },

            OffsetTimeCodes = new BatchConvertConfig.OffsetTimeCodesSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.OffsetTimeCodes),
                Forward = OffsetTimeCodesForward,
                Milliseconds = (long)OffsetTimeCodesTime.TotalMilliseconds,
            },

            AddFormatting = new BatchConvertConfig.AddFormattingSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.AddFormatting),
                AddItalic = FormattingAddItalic,
                AddBold = FormattingAddBold,
                AddUnderline = FormattingAddUnderline,
                AddColor = FormattingAddColor,
                AddColorValue = FormattingAddColorValue,
                AddAlignment = FormattingAddAlignmentTag,
                AddAlignmentValue = SelectedAlignmentTagOption?.Code ?? "an2",
            },

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

            RightToLeft = new BatchConvertConfig.RightToLeftSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.FixRightToLeft),
                FixViaUnicode = RtlFixViaUniCode,
                RemoveUnicode = RtlRemoveUniCode,
                ReverseStartEnd = RtlReverseStartEnd,
            },

            BridgeGaps = new BatchConvertConfig.BridgeGapsSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.BridgeGaps),
                BridgeGapsSmallerThanMs = BridgeGapsSmallerThanMs,
                MinGapMs = BridgeGapsMinGapMs,
                PercentForLeft = BridgeGapsPercentForLeft,
            },

            SplitBreakLongLines = new BatchConvertConfig.SplitBreakLongLinesSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.SplitBreakLongLines),
                SplitLongLines = SplitBreakSplitLongLines,
                RebalanceLongLines = SplitBreakRebalanceLongLines,
                MaxNumberOfLines = SplitBreakMaxNumberOfLines,
                SingleLineMaxLength = SplitBreakSingleLineMaxLength,
            },
        };
    }

    internal void SelectedFunctionChanged()
    {
        var selectedFunction = SelectedBatchFunction;
        if (selectedFunction != null)
        {
            FunctionContainer.Content = selectedFunction.View;
        }

        Dispatcher.UIThread.Post(() =>
        {
            var totalFunctionsSelected = 0;
            foreach (var function in BatchFunctions)
            {
                if (function.IsSelected)
                {
                    totalFunctionsSelected++;
                }
            }

            if (totalFunctionsSelected == 0)
            {
                ActionsSelected = string.Empty;
            }
            else if (totalFunctionsSelected == 1)
            {
                ActionsSelected = Se.Language.Tools.BatchConvert.OneActionsSelected;
            }
            else
            {
                ActionsSelected = string.Format(Se.Language.Tools.BatchConvert.XActionsSelected, totalFunctionsSelected);
            }
        });
    }

    private async Task ShowStatus(string statusText)
    {
        StatusText = statusText;
        await Task.Delay(6000, _cancellationToken).ConfigureAwait(false);
        StatusText = string.Empty;
    }

    private string CalculateGeneralStatistics()
    {
        if (BatchItems.Count == 0 || BatchItems.All(p => p.Subtitle == null))
        {
            return Se.Language.File.Statistics.NothingFound;
        }

        var allText = new StringBuilder();
        int minimumLineLength = 99999999;
        int maximumLineLength = 0;
        long totalLineLength = 0;
        int minimumSingleLineLength = 99999999;
        int maximumSingleLineLength = 0;
        long totalSingleLineLength = 0;
        long totalSingleLines = 0;
        int minimumSingleLineWidth = 99999999;
        int maximumSingleLineWidth = 0;
        long totalSingleLineWidth = 0;
        double minimumDuration = 100000000;
        double maximumDuration = 0;
        double totalDuration = 0;
        double minimumCharsSec = 100000000;
        double maximumCharsSec = 0;
        double totalCharsSec = 0;
        double minimumWpm = 100000000;
        double maximumWpm = 0;
        double totalWpm = 0;
        var gapMinimum = double.MaxValue;
        var gapMaximum = 0d;
        var gapTotal = 0d;

        var aboveOptimalCpsCount = 0;
        var aboveMaximumCpsCount = 0;
        var aboveMaximumWpmCount = 0;
        var belowMinimumDurationCount = 0;
        var aboveMaximumDurationCount = 0;
        var aboveMaximumLineLengthCount = 0;
        var aboveMaximumLineWidthCount = 0;
        var belowMinimumGapCount = 0;

        var sourceLength = 0;
        var totalSubtitleFiles = 0;
        var totalNumberOfLines = 0;
        var allParagraphs = new List<Paragraph>();

        foreach (var batchItem in BatchItems)
        {
            if (batchItem.Subtitle == null || batchItem.Subtitle.Paragraphs.Count == 0)
            {
                continue;
            }

            totalSubtitleFiles++;

            var _subtitle = batchItem.Subtitle;

            foreach (var p in _subtitle.Paragraphs)
            {
                allText.Append(p.Text);
                allParagraphs.Add(p);

                var len = GetLineLength(p);
                minimumLineLength = Math.Min(minimumLineLength, len);
                maximumLineLength = Math.Max(len, maximumLineLength);
                totalLineLength += len;

                var duration = p.DurationTotalMilliseconds;
                minimumDuration = Math.Min(duration, minimumDuration);
                maximumDuration = Math.Max(duration, maximumDuration);
                totalDuration += duration;

                var charsSec = p.GetCharactersPerSecond();
                minimumCharsSec = Math.Min(charsSec, minimumCharsSec);
                maximumCharsSec = Math.Max(charsSec, maximumCharsSec);
                totalCharsSec += charsSec;

                var wpm = p.WordsPerMinute;
                minimumWpm = Math.Min(wpm, minimumWpm);
                maximumWpm = Math.Max(wpm, maximumWpm);
                totalWpm += wpm;

                var next = _subtitle.GetParagraphOrDefault(_subtitle.GetIndex(p) + 1);
                if (next != null)
                {
                    var gap = next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds;
                    if (gap < gapMinimum)
                    {
                        gapMinimum = gap;
                    }

                    if (gap > gapMaximum)
                    {
                        gapMaximum = gap;
                    }

                    if (gap < Configuration.Settings.General.MinimumMillisecondsBetweenLines)
                    {
                        belowMinimumGapCount++;
                    }

                    gapTotal += gap;
                }

                foreach (var line in p.Text.SplitToLines())
                {
                    var l = GetSingleLineLength(line);
                    minimumSingleLineLength = Math.Min(l, minimumSingleLineLength);
                    maximumSingleLineLength = Math.Max(l, maximumSingleLineLength);
                    totalSingleLineLength += l;

                    if (l > Configuration.Settings.General.SubtitleLineMaximumLength)
                    {
                        aboveMaximumLineLengthCount++;
                    }

                    if (Configuration.Settings.Tools.ListViewSyntaxColorWideLines)
                    {
                        var w = GetSingleLineWidth(line);
                        minimumSingleLineWidth = Math.Min(w, minimumSingleLineWidth);
                        maximumSingleLineWidth = Math.Max(w, maximumSingleLineWidth);
                        totalSingleLineWidth += w;

                        if (w > Configuration.Settings.General.SubtitleLineMaximumPixelWidth)
                        {
                            aboveMaximumLineWidthCount++;
                        }
                    }

                    totalSingleLines++;
                }

                var cps = p.GetCharactersPerSecond();
                if (cps > Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds)
                {
                    aboveOptimalCpsCount++;
                }

                if (cps > Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                {
                    aboveMaximumCpsCount++;
                }

                if (p.WordsPerMinute > Configuration.Settings.General.SubtitleMaximumWordsPerMinute)
                {
                    aboveMaximumWpmCount++;
                }

                if (p.DurationTotalMilliseconds < Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                {
                    belowMinimumDurationCount++;
                }

                if (p.DurationTotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    aboveMaximumDurationCount++;
                }
            }

            sourceLength = +_subtitle.ToText(_subtitle.OriginalFormat ?? new SubRip()).Length;
            totalNumberOfLines += _subtitle.Paragraphs.Count;
        }

        var allTextToLower = allText.ToString().ToLowerInvariant();

        var sb = new StringBuilder();
        var _l = Se.Language.File.Statistics;
        var _format =
            sb.AppendLine(string.Format(_l.NumberOfFilesX, totalSubtitleFiles));
        sb.AppendLine(string.Format(_l.NumberOfLinesX, totalNumberOfLines));
        //sb.AppendLine(string.Format(_l.LengthInFormatXinCharactersY, _format.FriendlyName, sourceLength));
        sb.AppendLine(string.Format(_l.NumberOfCharactersInTextOnly, allText.ToString().CountCharacters(false)));
        sb.AppendLine(string.Format(_l.TotalDuration, new TimeCode(totalDuration).ToDisplayString()));
        sb.AppendLine(string.Format(_l.TotalCharsPerSecond, (double)allText.ToString().CountCharacters(true) / (totalDuration / TimeCode.BaseUnit)));
        //sb.AppendLine(string.Format(_l.TotalWords, _totalWords));
        sb.AppendLine(string.Format(_l.NumberOfItalicTags, Utilities.CountTagInText(allTextToLower, "<i>")));
        sb.AppendLine(string.Format(_l.NumberOfBoldTags, Utilities.CountTagInText(allTextToLower, "<b>")));
        sb.AppendLine(string.Format(_l.NumberOfUnderlineTags, Utilities.CountTagInText(allTextToLower, "<u>")));
        sb.AppendLine(string.Format(_l.NumberOfFontTags, Utilities.CountTagInText(allTextToLower, "<font ")));
        sb.AppendLine(string.Format(_l.NumberOfAlignmentTags, Utilities.CountTagInText(allTextToLower, "{\\an")));
        sb.AppendLine();
        sb.AppendLine(string.Format(_l.LineLengthMinimum, minimumLineLength) + " (" + GetIndicesWithLength(allParagraphs, minimumLineLength) + ")");
        sb.AppendLine(string.Format(_l.LineLengthMaximum, maximumLineLength) + " (" + GetIndicesWithLength(allParagraphs, maximumLineLength) + ")");
        sb.AppendLine(string.Format(_l.LineLengthAverage, (double)totalLineLength / totalNumberOfLines));
        sb.AppendLine();
        sb.AppendLine(string.Format(_l.LinesPerSubtitleAverage, (double)totalSingleLines / totalNumberOfLines));
        sb.AppendLine();
        sb.AppendLine(string.Format(_l.SingleLineLengthMinimum, minimumSingleLineLength) + " (" + GetIndicesWithSingleLineLength(allParagraphs, minimumSingleLineLength) + ")");
        sb.AppendLine(string.Format(_l.SingleLineLengthMaximum, maximumSingleLineLength) + " (" + GetIndicesWithSingleLineLength(allParagraphs, maximumSingleLineLength) + ")");
        sb.AppendLine(string.Format(_l.SingleLineLengthAverage, (double)totalSingleLineLength / totalSingleLines));
        sb.AppendLine();
        sb.AppendLine(string.Format(_l.SingleLineLengthExceedingMaximum, Configuration.Settings.General.SubtitleLineMaximumLength, aboveMaximumLineLengthCount,
            ((double)aboveMaximumLineLengthCount / totalNumberOfLines) * 100.0));
        sb.AppendLine();

        if (Configuration.Settings.Tools.ListViewSyntaxColorWideLines)
        {
            sb.AppendLine(string.Format(_l.SingleLineWidthMinimum, minimumSingleLineWidth) + " (" + GetIndicesWithSingleLineWidth(allParagraphs, minimumSingleLineWidth) + ")");
            sb.AppendLine(string.Format(_l.SingleLineWidthMaximum, maximumSingleLineWidth) + " (" + GetIndicesWithSingleLineWidth(allParagraphs, maximumSingleLineWidth) + ")");
            sb.AppendLine(string.Format(_l.SingleLineWidthAverage, (double)totalSingleLineWidth / totalSingleLines));
            sb.AppendLine();
            sb.AppendLine(string.Format(_l.SingleLineWidthExceedingMaximum, Configuration.Settings.General.SubtitleLineMaximumPixelWidth, aboveMaximumLineWidthCount,
                ((double)aboveMaximumLineWidthCount / totalNumberOfLines) * 100.0));
            sb.AppendLine();
        }

        sb.AppendLine(string.Format(_l.DurationMinimum, minimumDuration / TimeCode.BaseUnit) + " (" + GetIndicesWithDuration(allParagraphs, minimumDuration) + ")");
        sb.AppendLine(string.Format(_l.DurationMaximum, maximumDuration / TimeCode.BaseUnit) + " (" + GetIndicesWithDuration(allParagraphs, maximumDuration) + ")");
        sb.AppendLine(string.Format(_l.DurationAverage, totalDuration / totalNumberOfLines / TimeCode.BaseUnit));
        sb.AppendLine();
        sb.AppendLine(string.Format(_l.DurationExceedingMinimum, Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds / TimeCode.BaseUnit, belowMinimumDurationCount,
            ((double)belowMinimumDurationCount / totalNumberOfLines) * 100.0));
        sb.AppendLine(string.Format(_l.DurationExceedingMaximum, Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds / TimeCode.BaseUnit, aboveMaximumDurationCount,
            ((double)aboveMaximumDurationCount / totalNumberOfLines) * 100.0));
        sb.AppendLine();
        sb.AppendLine(string.Format(_l.CharactersPerSecondMinimum, minimumCharsSec) + " (" + GetIndicesWithCps(allParagraphs, minimumCharsSec) + ")");
        sb.AppendLine(string.Format(_l.CharactersPerSecondMaximum, maximumCharsSec) + " (" + GetIndicesWithCps(allParagraphs, maximumCharsSec) + ")");
        sb.AppendLine(string.Format(_l.CharactersPerSecondAverage, totalCharsSec / totalNumberOfLines));
        sb.AppendLine();
        sb.AppendLine(string.Format(_l.CharactersPerSecondExceedingOptimal, Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds, aboveOptimalCpsCount,
            ((double)aboveOptimalCpsCount / totalNumberOfLines) * 100.0));
        sb.AppendLine(string.Format(_l.CharactersPerSecondExceedingMaximum, Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds, aboveMaximumCpsCount,
            ((double)aboveMaximumCpsCount / totalNumberOfLines) * 100.0));
        sb.AppendLine();
        sb.AppendLine(string.Format(_l.WordsPerMinuteMinimum, minimumWpm) + " (" + GetIndicesWithWpm(allParagraphs, minimumWpm) + ")");
        sb.AppendLine(string.Format(_l.WordsPerMinuteMaximum, maximumWpm) + " (" + GetIndicesWithWpm(allParagraphs, maximumWpm) + ")");
        sb.AppendLine(string.Format(_l.WordsPerMinuteAverage, totalWpm / totalNumberOfLines));
        sb.AppendLine();
        sb.AppendLine(string.Format(_l.WordsPerMinuteExceedingMaximum, Configuration.Settings.General.SubtitleMaximumWordsPerMinute, aboveMaximumWpmCount,
            ((double)aboveMaximumWpmCount / totalNumberOfLines) * 100.0));
        sb.AppendLine();

        if (totalNumberOfLines > 1)
        {
            sb.AppendLine(string.Format(_l.GapMinimum, gapMinimum) + " (" + GetIndicesWithGap(allParagraphs, gapMinimum) + ")");
            sb.AppendLine(string.Format(_l.GapMaximum, gapMaximum) + " (" + GetIndicesWithGap(allParagraphs, gapMaximum) + ")");
            sb.AppendLine(string.Format(_l.GapAverage, gapTotal / totalNumberOfLines - 1));
            sb.AppendLine();
            sb.AppendLine(string.Format(_l.GapExceedingMinimum, Configuration.Settings.General.MinimumMillisecondsBetweenLines, belowMinimumGapCount,
                ((double)belowMinimumGapCount / totalNumberOfLines) * 100.0));
            sb.AppendLine();
        }

        return sb.ToString().Trim();
    }

    private static int GetLineLength(Paragraph p)
    {
        return p.Text.Replace(Environment.NewLine, string.Empty).CountCharacters(Configuration.Settings.General.CpsLineLengthStrategy, false);
    }

    private static int GetSingleLineLength(string s)
    {
        return s.CountCharacters(Configuration.Settings.General.CpsLineLengthStrategy, false);
    }

    private static int GetSingleLineWidth(string s)
    {
        var textBlock = new TextBlock();
        var x = TextMeasurer.MeasureString(HtmlUtil.RemoveHtmlTags(s, true), textBlock.FontFamily.Name, (float)textBlock.FontSize);
        return (int)x.Width;
    }

    private string GetIndicesWithLength(List<Paragraph> all, int length)
    {
        var indices = new List<string>();
        for (var i = 0; i < all.Count; i++)
        {
            var p = all[i];
            if (GetLineLength(p) == length)
            {
                if (indices.Count >= StatisticsNumberOfLinesToShow)
                {
                    indices.Add("...");
                    break;
                }

                indices.Add("#" + (i + 1).ToString(CultureInfo.InvariantCulture));
            }
        }

        return string.Join(", ", indices);
    }

    private string GetIndicesWithSingleLineLength(List<Paragraph> all, int length)
    {
        var indices = new List<string>();
        for (var i = 0; i < all.Count; i++)
        {
            var p = all[i];
            foreach (var line in p.Text.SplitToLines())
            {
                if (GetSingleLineLength(line) == length)
                {
                    if (indices.Count >= StatisticsNumberOfLinesToShow)
                    {
                        indices.Add("...");
                        return string.Join(", ", indices);
                    }

                    indices.Add("#" + (i + 1).ToString(CultureInfo.InvariantCulture));
                    break;
                }
            }
        }

        return string.Join(", ", indices);
    }

    private string GetIndicesWithSingleLineWidth(List<Paragraph> all, int width)
    {
        var indices = new List<string>();
        for (var i = 0; i < all.Count; i++)
        {
            var p = all[i];
            foreach (var line in p.Text.SplitToLines())
            {
                if (GetSingleLineWidth(line) == width)
                {
                    if (indices.Count >= StatisticsNumberOfLinesToShow)
                    {
                        indices.Add("...");
                        return string.Join(", ", indices);
                    }

                    indices.Add("#" + (i + 1).ToString(CultureInfo.InvariantCulture));
                    break;
                }
            }
        }

        return string.Join(", ", indices);
    }

    private string GetIndicesWithDuration(List<Paragraph> all, double duration)
    {
        var indices = new List<string>();
        for (var i = 0; i < all.Count; i++)
        {
            var p = all[i];
            if (Math.Abs(p.DurationTotalMilliseconds - duration) < 0.01)
            {
                if (indices.Count >= StatisticsNumberOfLinesToShow)
                {
                    indices.Add("...");
                    break;
                }

                indices.Add("#" + (i + 1).ToString(CultureInfo.InvariantCulture));
            }
        }

        return string.Join(", ", indices);
    }

    private string GetIndicesWithCps(List<Paragraph> all, double cps)
    {
        var indices = new List<string>();
        for (var i = 0; i < all.Count; i++)
        {
            var p = all[i];
            if (Math.Abs(p.GetCharactersPerSecond() - cps) < 0.01)
            {
                if (indices.Count >= StatisticsNumberOfLinesToShow)
                {
                    indices.Add("...");
                    break;
                }

                indices.Add("#" + (i + 1).ToString(CultureInfo.InvariantCulture));
            }
        }

        return string.Join(", ", indices);
    }

    private string GetIndicesWithWpm(List<Paragraph> all, double wpm)
    {
        var indices = new List<string>();
        for (var i = 0; i < all.Count; i++)
        {
            var p = all[i];
            if (Math.Abs(p.WordsPerMinute - wpm) < 0.01)
            {
                if (indices.Count >= StatisticsNumberOfLinesToShow)
                {
                    indices.Add("...");
                    break;
                }

                indices.Add("#" + (i + 1).ToString(CultureInfo.InvariantCulture));
            }
        }

        return string.Join(", ", indices);
    }

    private string GetIndicesWithGap(List<Paragraph> all, double cps)
    {
        var indices = new List<string>();
        for (var i = 0; i < all.Count - 1; i++)
        {
            var p = all[i];
            var next = all[i + 1];
            var gap = next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds;
            if (Math.Abs(gap - cps) < 0.01)
            {
                if (indices.Count >= StatisticsNumberOfLinesToShow)
                {
                    indices.Add("...");
                    break;
                }

                indices.Add("#" + (i + 1).ToString(CultureInfo.InvariantCulture));
            }
        }

        return string.Join(", ", indices);
    }

    internal void FileGridOnDragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy; // show copy cursor
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    internal void FileGridOnDrop(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.File))
        {
            return;
        }

        var files = e.DataTransfer.TryGetFiles();
        if (files != null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var file in files)
                {
                    var path = file.Path?.LocalPath;
                    if (path != null && File.Exists(path))
                    {
                        var fileInfo = new FileInfo(path);
                        var subtitle = Subtitle.Parse(path);
                        var batchItem = new BatchConvertItem(path, fileInfo.Length,
                            subtitle != null ? subtitle.OriginalFormat.Name : Se.Language.General.Unknown, subtitle);
                        BatchItems.Add(batchItem);
                        _allBatchItems.Add(batchItem);
                    }
                }
                _isFilesDirty = true;
            });
        }
    }

    internal void OnAutoTranslatorChanged()
    {
        var engine = SelectedAutoTranslator;

        AutoTranslateModelIsVisible = engine is OllamaTranslate;

        if (engine is OllamaTranslate)
        {
            AutoTranslateModelText = Se.Settings.AutoTranslate.OllamaModel;
            AutoTranslateModelBrowseIsVisible = true;
            AutoTranslateModelIsVisible = true;
        }
        else
        {
            AutoTranslateModelText = string.Empty;
            AutoTranslateModelBrowseIsVisible = false;
            AutoTranslateModelIsVisible = false;
        }
    }

    internal void Onloaded(object? sender, RoutedEventArgs e)
    {
        UiUtil.RestoreWindowPosition(Window);
    }

    internal void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        UiUtil.SaveWindowPosition(Window);
    }

    internal void ComboBoxSubtitleFormatChanged()
    {
        var selectedFormat = SelectedTargetFormat ?? string.Empty;
        IsTargetFormatSettingsVisible = _targetFormatsWithSettings.Contains(selectedFormat);
    }

    internal void FilterComboBoxChanged()
    {
        var selection = SelectedFilterItem;
        if (selection == Se.Language.General.AllFiles)
        {
            IsFilterTextVisible = false;
        }
        else
        {
            IsFilterTextVisible = true;
        }
        _isFilesDirty = true;
    }

    internal void FilterTextChanged()
    {
        _isFilesDirty = true;
    }
}