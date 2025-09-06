using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.EncodingSettings;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.SplitSubtitle;

public partial class SplitSubtitleViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SplitDisplayItem> _splitItems;
    [ObservableProperty] private SplitDisplayItem? _selectedSpiltItem;
    [ObservableProperty] private bool _splitByLines;
    [ObservableProperty] private bool _splitByCharacters;
    [ObservableProperty] private bool _splitByTime;
    [ObservableProperty] private bool _numberOfEqualParts;
    [ObservableProperty] private string _subtitleInfo;
    [ObservableProperty] private string _outputFolder;
    [ObservableProperty] private ObservableCollection<SubtitleFormat> _formats;
    [ObservableProperty] private SubtitleFormat? _selectedSubtitleFormat;
    [ObservableProperty] private ObservableCollection<EncodingDisplayItem> _encodings;
    [ObservableProperty] private EncodingDisplayItem? _selectedEncoding;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public int PartsMax { get; internal set; }

    private readonly IFolderHelper _folderHelper;

    public SplitSubtitleViewModel(IFolderHelper folderHelper)
    {
        _folderHelper = folderHelper;

        SplitItems = new ObservableCollection<SplitDisplayItem>();
        Formats = new ObservableCollection<SubtitleFormat>(SubtitleFormat.AllSubtitleFormats);
        SelectedSubtitleFormat = Formats[0];
        Encodings = new ObservableCollection<EncodingDisplayItem>(EncodingDisplayItem.GetDefaultEncodings());
        SelectedEncoding = Encodings[0];
        OutputFolder = string.Empty;
        SubtitleInfo = string.Empty;    

        LoadSettings();
    }

    public void Initialize(string fileName, Subtitle subtitle)
    {
    }

    private void LoadSettings()
    {
    }

    private void SaveSettings()
    {
        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task Browse()
    {
        if (Window == null)
        {
            return;
        }

        await _folderHelper.PickFolderAsync(Window, Se.Language.General.PickOutputFolder);
    }

    [RelayCommand]
    private void OpenFolder()
    {
        if (Window == null)
        {
            return;
        }

        _folderHelper.OpenFolder(Window, OutputFolder);
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
        OkPressed = true;
        Window?.Close();
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