using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
    //[ObservableProperty] private ObservableCollection<AdjustDurationItem> AdjustTypes;
    //[ObservableProperty] private AdjustDurationItem SelectedAdjustType;
    [ObservableProperty] private TimeSpan _adjustSeconds;
    [ObservableProperty] private int _adjustPercentage;
    [ObservableProperty] private TimeSpan _adjustFixedValue;
    [ObservableProperty] private decimal _adjustRecalculateMaximumCharacters;
    [ObservableProperty] private decimal _adjustRecalculateOptimalCharacters;
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

    public bool OkPressed { get; private set; }

    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;

    public BatchConvertViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;

        BatchItems = new ObservableCollection<BatchConvertItem>();
        BatchFunctions = new ObservableCollection<BatchConvertFunction>();
        TargetFormats = new ObservableCollection<string>(SubtitleFormat.AllSubtitleFormats.Select(p => p.Name));
        TargetEncodings = new ObservableCollection<string>(EncodingHelper.GetEncodings().Select(p => p.DisplayName).ToList());
        DeleteLineNumbers = new ObservableCollection<int>();
        TargetFormatName = string.Empty;
        TargetEncoding = string.Empty;
        BatchItemsInfo = string.Empty;
        ProgressText = string.Empty;
        DeleteLinesContains = string.Empty;
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
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
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
            var batchItem = new BatchConvertItem(fileName, fileInfo.Length, subtitle != null ? subtitle.OriginalFormat.Name : "Unknown", subtitle);
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
    }

    [RelayCommand]
    private void ShowOutputProperties()
    {
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