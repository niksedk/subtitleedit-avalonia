using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Assa;

public partial class AssaPropertiesViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _scriptTitle;
    [ObservableProperty] private string _originalScript;
    [ObservableProperty] private string _translation;
    [ObservableProperty] private string _editing;
    [ObservableProperty] private string _timing;
    [ObservableProperty] private string _syncPoint;
    [ObservableProperty] private string _updatedBy;
    [ObservableProperty] private string _updateDetails;
    [ObservableProperty] private int _videoWidth;
    [ObservableProperty] private int _videoHeight;
    [ObservableProperty] private ObservableCollection<WrapStyleItem> _wrapStyles;
    [ObservableProperty] private WrapStyleItem? _selectedWrapStyle;
    [ObservableProperty] private ObservableCollection<BorderAndShadowScalingItem> _borderAndShadowScalingStyles;
    [ObservableProperty] private BorderAndShadowScalingItem? _selectedBorderAndShadowScalingStyle;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }
    public string Header { get; set; }

    private readonly IFileHelper _fileHelper;
    private string _fileName;
    private Subtitle _subtitle;
    private readonly System.Timers.Timer _timerUpdatePreview;

    public AssaPropertiesViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

        Title = string.Empty;
        ScriptTitle = string.Empty;
        OriginalScript = string.Empty;
        Translation = string.Empty;
        Editing = string.Empty;
        Timing = string.Empty;
        SyncPoint = string.Empty;
        UpdatedBy = string.Empty;
        UpdateDetails = string.Empty;
        WrapStyles = new ObservableCollection<WrapStyleItem>(WrapStyleItem.List());
        SelectedWrapStyle = WrapStyles[0];
        BorderAndShadowScalingStyles = new ObservableCollection<BorderAndShadowScalingItem>(BorderAndShadowScalingItem.List());
        SelectedBorderAndShadowScalingStyle = BorderAndShadowScalingStyles[2];

        _fileName = string.Empty;
        Header = string.Empty;
        _subtitle = new Subtitle();

        LoadSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        SaveSettings();
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private void BrowseResolution()
    {
    }

    [RelayCommand]
    private void GetResolutionFromCurrentVideo()
    {
    }

    public void Initialize(Subtitle subtitle, SubtitleFormat format, string fileName)
    {
        Title = string.Format(Se.Language.Assa.PropertiesTitleX, fileName);
        Header = subtitle.Header;
        _subtitle = subtitle;
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    private void SaveSettings()
    {
    }

    private void LoadSettings()
    {
    }


    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
}
