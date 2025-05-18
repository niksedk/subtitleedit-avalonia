using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.Validators;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Common;
using Nikse.SubtitleEdit.Features.Edit.Find;
using Nikse.SubtitleEdit.Features.Edit.GoToLineNumber;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Features.Edit.Replace;
using Nikse.SubtitleEdit.Features.Edit.ShowHistory;
using Nikse.SubtitleEdit.Features.Help;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Options.Language;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;
using Nikse.SubtitleEdit.Features.Sync.AdjustAllTimes;
using Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;
using Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Tools.ChangeCasing;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Features.Video.OpenFromUrl;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TransparentSubtitles;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Main;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    private List<SubtitleLineViewModel>? _selectedSubtitles;
    [ObservableProperty] private int? _selectedSubtitleIndex;

    [ObservableProperty] private string _editText;
    [ObservableProperty] private string _editTextCharactersPerSecond;
    [ObservableProperty] private string _editTextTotalLength;
    [ObservableProperty] private string _editTextLineLengths;

    [ObservableProperty] private ObservableCollection<SubtitleFormat> _subtitleFormats;
    [ObservableProperty] private SubtitleFormat _selectedSubtitleFormat;

    [ObservableProperty] private ObservableCollection<TextEncoding> _encodings;
    public TextEncoding SelectedEncoding { get; set; }

    [ObservableProperty] private string _statusTextLeft;
    [ObservableProperty] private string _statusTextRight;

    public DataGrid SubtitleGrid { get; set; }
    public TextBox EditTextBox { get; set; }
    public MainView View { get; set; }
    public Window Window { get; set; }
    public Grid ContentGrid { get; set; }
    public MainView MainView { get; set; }
    
    public ITreeDataGridSource? SubtitlesSource { get; set; }
    public TextBlock StatusTextLeftLabel { get; internal set; }


    public Grid Waveform { get; internal set; }
    public MenuItem MenuReopen { get; internal set; }

    private string? _subtitleFileName;
    private Subtitle _subtitle;
    private SubtitleFormat? _lastOpenSaveFormat;

    private string? _videoFileName;
    private CancellationTokenSource? _statusFadeCts;
    private int _changeSubtitleHash = -1;

    private readonly IFileHelper _fileHelper;
    private readonly IShortcutManager _shortcutManager;
    private readonly IWindowService _windowService;

    private bool IsEmpty => Subtitles.Count == 0 || string.IsNullOrEmpty(Subtitles[0].Text);

    public VideoPlayerControl? VideoPlayerControl { get; internal set; }

    public MainViewModel(IFileHelper fileHelper, IShortcutManager shortcutManager, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _shortcutManager = shortcutManager;
        _windowService = windowService;

        EditText = string.Empty;
        EditTextCharactersPerSecond = string.Empty;
        EditTextTotalLength = string.Empty;
        EditTextLineLengths = string.Empty;
        StatusTextLeftLabel = new TextBlock();
        SubtitleGrid = new DataGrid();
        EditTextBox = new TextBox();
        ContentGrid = new Grid();


        _subtitle = new Subtitle();

        Subtitles = new ObservableCollection<SubtitleLineViewModel>();

        SubtitleFormats = [.. SubtitleFormat.AllSubtitleFormats];
        SelectedSubtitleFormat = SubtitleFormats[0];

        Encodings = new ObservableCollection<TextEncoding>(EncodingHelper.GetEncodings());
        SelectedEncoding = Encodings[0];

        StatusTextLeft = string.Empty;
        StatusTextRight = string.Empty;

        SubtitlesSource = new FlatTreeDataGridSource<SubtitleLineViewModel>(Subtitles)
        {
            Columns =
            {
                new TextColumn<SubtitleLineViewModel, int>("#", x => x.Number),
                new TextColumn<SubtitleLineViewModel, string>("Show", x =>
                        TimeSpanFormatter.ToStringHms(x.StartTime),
                    (x, val) => x.StartTime = TimeSpanFormatter.FromStringHms(val)
                ),
                new TextColumn<SubtitleLineViewModel, string>("Hide", x =>
                        TimeSpanFormatter.ToStringHms(x.EndTime),
                    (x, val) => x.StartTime = TimeSpanFormatter.FromStringHms(val)
                ),
                new TextColumn<SubtitleLineViewModel, string>("Duration", x =>
                        TimeSpanFormatterShort.ToStringShort(x.Duration),
                    (x, val) => x.Duration = TimeSpanFormatterShort.FromStringShort(val)
                ),
                new TextColumn<SubtitleLineViewModel, string>("Text", x => x.Text, null,
                    new GridLength(1, GridUnitType.Star)),
            },
        };

        var dataGridSource = SubtitlesSource as FlatTreeDataGridSource<SubtitleLineViewModel>;
        dataGridSource!.RowSelection!.SingleSelect = false;

        LoadShortcuts();

        StartTitleTimer();
    }

    private void LoadShortcuts()
    {
        Se.Settings.InitializeMainShortcuts(this);
        foreach (var shortCut in ShortcutsMain.GetUsedShortcuts(this))
        {
            _shortcutManager.RegisterShortcut(shortCut);
        }
    }

    [RelayCommand]
    private async Task CommandExit()
    {
        if (HasChanges())
        {
            var result = await MessageBox.Show(
                Window,
                "Save Changes",
                "Do you want to save before exiting?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == MessageBoxResult.Cancel)
            {
                return;
            }

            if (result == MessageBoxResult.Yes)
            {
                await SaveSubtitle();
            }
        }

        Environment.Exit(0);
    }

    [RelayCommand]
    private async Task CommandShowLayout()
    {
        // Open a dialog with a specific ViewModel and get the result
        var vm = await _windowService.ShowDialogAsync<LayoutWindow, LayoutViewModel>(Window,
            viewModel => { viewModel.SelectedLayout = Se.Settings.General.LayoutNumber; });

        if (vm.OkPressed && vm.SelectedLayout != null && vm.SelectedLayout != Se.Settings.General.LayoutNumber)
        {
            Se.Settings.General.LayoutNumber = InitLayout.MakeLayout(MainView, this, vm.SelectedLayout.Value);
        }
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowHelp()
    {
        await Window!.Launcher.LaunchUriAsync(new Uri("https://www.nikse.dk/subtitleedit/help"));
    }

    [RelayCommand]
    private async Task ShowAbout()
    {
        var newWindow = new AboutWindow();
        await newWindow.ShowDialog(Window);
    }

    [RelayCommand]
    private async Task CommandFileNew()
    {
        if (HasChanges())
        {
            var result = await MessageBox.Show(
                Window,
                "Save Changes?",
                "Do you want to save changes?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == MessageBoxResult.Cancel)
            {
                return;
            }

            if (result == MessageBoxResult.Yes)
            {
                // Save logic here
            }
        }

        Subtitles.Clear();
        _subtitle = new Subtitle();
        _changeSubtitleHash = GetFastSubtitleHash();
    }

    [RelayCommand]
    private async Task CommandFileOpen()
    {
        var fileName = await _fileHelper.PickOpenSubtitleFile(Window, "Open subtitle file");
        if (!string.IsNullOrEmpty(fileName))
        {
            await SubtitleOpen(fileName);
        }
    }

    [RelayCommand]
    private async Task CommandFileReopen(RecentFile recentFile)
    {
        await SubtitleOpen(recentFile.SubtitleFileName, recentFile.VideoFileName, recentFile.SelectedLine);
    }

    [RelayCommand]
    private void CommandFileClearRecentFiles(RecentFile recentFile)
    {
        Se.Settings.File.RecentFiles.Clear();
        InitMenu.UpdateRecentFiles(this);
    }

    [RelayCommand]
    private async Task CommandFileSave()
    {
        await SaveSubtitle();
    }

    [RelayCommand]
    private async Task CommandFileSaveAs()
    {
        await SaveSubtitleAs();
    }


    [RelayCommand]
    private async Task ShowToolsAdjustDurations()
    {
        await _windowService.ShowDialogAsync<AdjustDurationWindow, AdjustDurationViewModel>(Window, vm =>
        {

        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowToolsBatchConvert()
    {
        await _windowService.ShowDialogAsync<BatchConvertWindow, BatchConvertViewModel>(Window, vm =>
        {

        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowToolsChangeCasing()
    {
        await _windowService.ShowDialogAsync<ChangeCasingWindow, ChangeCasingViewModel>(Window, vm =>
        {

        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowToolsFixCommonErrors()
    {
        await _windowService.ShowDialogAsync<FixCommonErrorsWindow, FixCommonErrorsViewModel>(Window, vm =>
        {

        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowToolsRemoveTextForHearingImpaired()
    {
        await _windowService.ShowDialogAsync<RemoveTextForHearingImpairedWindow, RemoveTextForHearingImpairedViewModel>(Window, vm =>
        {

        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandVideoOpen()
    {
        var fileName = await _fileHelper.PickOpenVideoFile(Window, "Open video file");
        if (!string.IsNullOrEmpty(fileName))
        {
            await VideoOpenFile(fileName);
        }
    }

    [RelayCommand]
    private void CommandVideoClose()
    {
        VideoCloseFile();
    }

    [RelayCommand]
    private async Task ShowSpellCheck()
    {
        await _windowService.ShowDialogAsync<SpellCheckWindow, SpellCheckViewModel>(Window, vm =>
        {

        });
        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private async Task ShowSpellCheckDictionaries()
    {
        await _windowService.ShowDialogAsync<GetDictionariesWindow, GetDictionariesViewModel>(Window, vm =>
        {

        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowVideoAudioToTextWhisper()
    {
        await _windowService.ShowDialogAsync<AudioToTextWhisperWindow, AudioToTextWhisperViewModel>(Window, vm =>
        {

        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowVideoBurnIn()
    {
        await _windowService.ShowDialogAsync<BurnInWindow, BurnInViewModel>(Window, vm =>
        {

        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowVideoOpenFromUrl()
    {
        await _windowService.ShowDialogAsync<OpenFromUrlWindow, OpenFromUrlViewModel>(Window, vm =>
        {

        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowVideoTextToSpeech()
    {
        await _windowService.ShowDialogAsync<TextToSpeechWindow, TextToSpeechViewModel>(Window, vm =>
        {

        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowVideoTransparentSubtitles()
    {
        await _windowService.ShowDialogAsync<TransparentSubtitlesWindow, TransparentSubtitlesViewModel>(Window, vm =>
        {

        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowSyncAdjustAllTimes()
    {
        await _windowService.ShowDialogAsync<AdjustAllTimesWindow, AdjustAllTimesViewModel>(Window, vm =>
        {

        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowSyncChangeFrameRate()
    {
        await _windowService.ShowDialogAsync<ChangeFrameRateWindow, ChangeFrameRateViewModel>(Window, vm =>
        {

        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowSyncChangeSpeed()
    {
        await _windowService.ShowDialogAsync<ChangeSpeedWindow, ChangeSpeedViewModel>(Window, vm =>
        {

        });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandShowAutoTranslate()
    {
        var viewModel = await _windowService.ShowDialogAsync<AutoTranslateWindow, AutoTranslateViewModel>(Window,
            viewModel => { viewModel.Initialize(GetUpdateSubtitle()); });

        if (viewModel.OkPressed)
        {
            Console.WriteLine("User confirmed the action");
        }
        
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandShowSettings()
    {
        var oldTheme = Se.Settings.Appearance.Theme;

        var viewModel = await _windowService.ShowDialogAsync<SettingsWindow, SettingsViewModel>(Window);
        _shortcutManager.ClearKeys();
        if (!viewModel.OkPressed)
        {
            return;
        }

        if (oldTheme != viewModel.SelectedTheme)
        {
            if (viewModel.SelectedTheme == "Dark")
            {
                Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
            }
            else if (viewModel.SelectedTheme == "Light")
            {
                Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
            }
            else
            {
                Application.Current!.RequestedThemeVariant = ThemeVariant.Default;
            }
        }
    }

    [RelayCommand]
    private async Task CommandShowSettingsShortcuts()
    {
        await _windowService.ShowDialogAsync<ShortcutsWindow, ShortcutsViewModel>(Window,
            vm => { vm.LoadShortCuts(this); });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandShowSettingsLanguage()
    {
        var viewModel = await _windowService.ShowDialogAsync<LanguageWindow, LanguageViewModel>(Window);
        if (viewModel.OkPressed)
        {
            // todo
        }
        
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void DeleteSelectedLines()
    {
        DeleteSelectedItems();
    }

    [RelayCommand]
    private void InsertLineBefore()
    {
    }

    [RelayCommand]
    private void InsertLineAfter()
    {
        InsertAfterSelectedItem();
    }

    [RelayCommand]
    private void ToggleLinesItalic()
    {
        ToggleItalic();
    }

    [RelayCommand]
    private async Task ShowRestoreAutoBackup()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var viewModel = await _windowService.ShowDialogAsync<RestoreAutoBackupWindow, RestoreAutoBackupViewModel>(Window, vm =>
        {
        });

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowHistory()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var viewModel = await _windowService.ShowDialogAsync<ShowHistoryWindow, ShowHistoryViewModel>(Window, vm =>
        {
        });

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowFind()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var viewModel = await _windowService.ShowDialogAsync<FindWindow, FindViewModel>(Window, vm => 
        { 
        });

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task FindNext()
    {
    }


    [RelayCommand]
    private async Task ShowReplace()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var viewModel = await _windowService.ShowDialogAsync<ReplaceWindow, ReplaceViewModel>(Window, vm =>
        {
        });

        _shortcutManager.ClearKeys();
    }



    [RelayCommand]
    private async Task ShowMultipleReplace()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var viewModel = await _windowService.ShowDialogAsync<MultipleReplaceWindow, MultipleReplaceViewModel>(Window, vm =>
        {
        });

        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private async Task ShowGoToLine()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var viewModel = await _windowService.ShowDialogAsync<GoToLineNumberWindow, GoToLineNumberViewModel>(Window,
            vm => { vm.MaxLineNumber = Subtitles.Count; });
        if (viewModel is { OkPressed: true, LineNumber: >= 0 } && viewModel.LineNumber < Subtitles.Count)
        {
            SelectAndScrollToRow(viewModel.LineNumber - 1);
        }
        
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void SelectAllLines()
    {
        SelectAllRows();
    }

    [RelayCommand]
    private void InverseSelection()
    {
        InverseRowSelection();
    }

    [RelayCommand]
    private void GoToNextLine()
    {
        var idx = SelectedSubtitleIndex ?? -1;
        if (Subtitles.Count == 0 || idx < 0 || idx >= Subtitles.Count)
        {
            return;
        }

        idx++;
        if (idx >= Subtitles.Count)
        {
            return;
        }

        SelectAndScrollToRow(idx);
    }

    [RelayCommand]
    private void GoToPreviousLine()
    {
        var idx = SelectedSubtitleIndex ?? -1;
        if (Subtitles.Count == 0 || idx < 0 || idx >= Subtitles.Count)
        {
            return;
        }

        idx--;
        if (idx < 0)
        {
            return;
        }

        SelectAndScrollToRow(idx);
    }

    private Control? _fullscreenBeforeParent;

    [RelayCommand]
    private void VideoFullScreen()
    {
        var control = VideoPlayerControl;
        if (control == null || control.IsFullScreen)
        {
            return;
        }

        var parent = (Control)control.Parent!;
        _fullscreenBeforeParent = parent;
        control.RemoveControlFromParent();
        control.IsFullScreen = true;
        var fullscreenWindow = new FullScreenVideoWindow(control, () =>
        {
            if (_fullscreenBeforeParent != null)
            {
                control.RemoveControlFromParent().AddControlToParent(_fullscreenBeforeParent);
            }

            control.IsFullScreen = false;
        });
        fullscreenWindow.Show(Window);
    }

    [RelayCommand]
    private void Unbreak()
    {
        var s = SelectedSubtitle;
        if (s == null)
        {
            return;
        }

        s.Text = Utilities.UnbreakLine(s.Text);
    }

    [RelayCommand]
    private void AutoBreak()
    {
        var s = SelectedSubtitle;
        if (s == null)
        {
            return;
        }

        s.Text = Utilities.AutoBreakLine(s.Text);
    }

    private void SelectAllRows()
    {
        SubtitleGrid.SelectAll();
    }
    
    private void InverseRowSelection()
    {
        if (SubtitleGrid.SelectedItems == null || Subtitles.Count == 0)
        {
            return;
        }

        // Store currently selected items
        var selectedItems = new HashSet<SubtitleLineViewModel>(SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>());
    
        // Clear current selection
        SubtitleGrid.SelectedItems.Clear();
    
        // Add all items that weren't previously selected
        foreach (var item in Subtitles)
        {
            if (!selectedItems.Contains(item))
            {
                SubtitleGrid.SelectedItems.Add(item);
            }
        }
    }


    // Method to select a specific row by index and make it visible
    private void SelectAndScrollToRow(int index)
    {
        if (index < 0 || index >= Subtitles.Count)
        {
            return;
        }

        // First, set the selected index (this selects the row)
        SubtitleGrid.SelectedIndex = index;

        // Then, scroll to the selected item to make it visible
        SubtitleGrid.ScrollIntoView(SubtitleGrid.SelectedItem, null);
    }

// Method to select a specific subtitle and make it visible
    public void SelectAndScrollToSubtitle(SubtitleLineViewModel subtitle)
    {
        if (subtitle == null || !Subtitles.Contains(subtitle))
        {
            return;
        }

        // First, set the selected item (this selects the row)
        SubtitleGrid.SelectedItem = subtitle;

        // Then, scroll to the selected item to make it visible
        SubtitleGrid.ScrollIntoView(subtitle, null);
    }

    private async Task SubtitleOpen(string fileName, string? videoFileName = null, int? selectedSubtitleIndex = null)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var ext = Path.GetExtension(fileName);
        var fileSize = (long)0;
        try
        {
            var fi = new FileInfo(fileName);
            fileSize = fi.Length;
        }
        catch
        {
            // ignore
        }

        if (fileSize < 10)
        {
            var message = fileSize == 0 ? "File size is zero!" : $"File size too small - only {fileSize} bytes";
            await MessageBox.Show(Window, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            foreach (var f in SubtitleFormat.GetBinaryFormats(false))
            {
                if (f.IsMine(null, fileName))
                {
                    subtitle = new Subtitle();
                    f.LoadSubtitle(subtitle, null, fileName);
                    subtitle.OriginalFormat = f;
                    break; // format found, exit the loop
                }
            }

            if (subtitle == null)
            {
                var message = "Unknown format?";
                await MessageBox.Show(Window, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        _subtitleFileName = fileName;
        _subtitle = subtitle;
        _lastOpenSaveFormat = subtitle.OriginalFormat;
        SetSubtitles(_subtitle);
        await ShowStatus($"Subtitle loaded: {fileName}");

        if (selectedSubtitleIndex != null)
        {
            SelectAndScrollToRow(selectedSubtitleIndex.Value);
        }

        if (Se.Settings.Video.AutoOpen)
        {
            if (!string.IsNullOrEmpty(videoFileName) && File.Exists(videoFileName))
            {
                await VideoOpenFile(videoFileName);
            }
            else if (FindVideoFileName.TryFindVideoFileName(fileName, out videoFileName))
            {
                await VideoOpenFile(videoFileName);
            }
        }

        AddToRecentFiles(true);
        _changeSubtitleHash = GetFastSubtitleHash();
    }

    private void SetSubtitles(Subtitle subtitle)
    {
        Subtitles.Clear();
        foreach (var p in subtitle.Paragraphs)
        {
            Subtitles.Add(new SubtitleLineViewModel
            {
                Number = p.Number,
                StartTime = p.StartTime.TimeSpan,
                EndTime = p.EndTime.TimeSpan,
                Duration = p.Duration.TimeSpan,
                Text = p.Text
            });
        }
    }

    private bool HasChanges()
    {
        return !IsEmpty && _changeSubtitleHash != GetFastSubtitleHash();
    }

    private async Task SaveSubtitle()
    {
        if (Subtitles == null || !Subtitles.Any())
        {
            await ShowStatus("Nothing to save");
            return;
        }

        if (string.IsNullOrEmpty(_subtitleFileName))
        {
            await SaveSubtitleAs();
            return;
        }

        if (_lastOpenSaveFormat == null || _lastOpenSaveFormat.Name != SelectedSubtitleFormat.Name)
        {
            await SaveSubtitleAs();
            return;
        }

        var text = GetUpdateSubtitle().ToText(SelectedSubtitleFormat);
        await File.WriteAllTextAsync(_subtitleFileName, text);
        _changeSubtitleHash = GetFastSubtitleHash();
        _lastOpenSaveFormat = SelectedSubtitleFormat;
    }

    private Subtitle GetUpdateSubtitle()
    {
        _subtitle.Paragraphs.Clear();
        foreach (var line in Subtitles)
        {
            var p = new Paragraph
            {
                Number = line.Number,
                StartTime = new TimeCode(line.StartTime),
                EndTime = new TimeCode(line.EndTime),
                Text = line.Text
            };

            _subtitle.Paragraphs.Add(p);
        }

        return _subtitle;
    }

    private async Task SaveSubtitleAs()
    {
        var newFileName = "New" + SelectedSubtitleFormat.Extension;
        if (!string.IsNullOrEmpty(_subtitleFileName))
        {
            newFileName = Path.GetFileNameWithoutExtension(_subtitleFileName) + SelectedSubtitleFormat.Extension;
        }
        else if (!string.IsNullOrEmpty(_videoFileName))
        {
            newFileName = Path.GetFileNameWithoutExtension(_videoFileName) + SelectedSubtitleFormat.Extension;
        }

        var fileName = await _fileHelper.PickSaveSubtitleFile(
            Window,
            SelectedSubtitleFormat,
            newFileName,
            "Save subtitle file");
        if (!string.IsNullOrEmpty(fileName))
        {
            _subtitleFileName = fileName;
            _subtitle.FileName = fileName;
            _lastOpenSaveFormat = SelectedSubtitleFormat;
            await SaveSubtitle();
            AddToRecentFiles(true);
        }
    }

    private void AddToRecentFiles(bool updateMenu)
    {
        if (string.IsNullOrEmpty(_subtitleFileName))
        {
            return;
        }

        Se.Settings.File.AddToRecentFiles(_subtitleFileName, _videoFileName ?? string.Empty, SelectedSubtitleIndex ?? 0,
            SelectedEncoding.DisplayName);
        Se.SaveSettings();

        if (updateMenu)
        {
            InitMenu.UpdateRecentFiles(this);
        }
    }

    private async Task ShowStatus(string message, int delayMs = 3000)
    {
        // Cancel any previous animation
        _statusFadeCts?.Cancel();
        _statusFadeCts = new CancellationTokenSource();
        var token = _statusFadeCts.Token;

        StatusTextLeft = message;
        StatusTextLeftLabel.Opacity = 1;
        StatusTextLeftLabel.IsVisible = true;

        try
        {
            await Task.Delay(delayMs, token); // Wait 3 seconds, cancellable
            StatusTextLeft = string.Empty;
        }
        catch (TaskCanceledException)
        {
            // Ignore - a new message came in and interrupted
        }
    }

    internal void OnClosing()
    {
        //MediaPlayerVlc?.Dispose();
        //libVLC?.Dispose();

        AddToRecentFiles(false);
        Se.SaveSettings();
    }

    internal void OnLoaded()
    {
        if (Se.Settings.File.ShowRecentFiles)
        {
            var first = Se.Settings.File.RecentFiles.FirstOrDefault();
            if (first != null && File.Exists(first.SubtitleFileName))
            {
                SubtitleOpen(first.SubtitleFileName, first.VideoFileName, first.SelectedLine).ConfigureAwait(false);
            }
        }
    }

    private async Task VideoOpenFile(string videoFileName)
    {
        if (VideoPlayerControl == null)
        {
            return;
        }

        await VideoPlayerControl.Open(videoFileName);

        var peakWaveFileName = WavePeakGenerator.GetPeakWaveFileName(videoFileName);
        if (!File.Exists(peakWaveFileName))
        {
            if (FfmpegHelper.IsFfmpegInstalled())
            {
                var tempWaveFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
                var process = WaveFileExtractor.GetCommandLineProcess(videoFileName, -1, tempWaveFileName,
                    Configuration.Settings.General.VlcWaveTranscodeSettings, out _);
                await ShowStatus("Extracting wave info...");
                Task.Run(async () =>
                {
                    //await ExtractWaveformAndSpectrogram(process, tempWaveFileName, peakWaveFileName);
                });
            }
        }
        else
        {
            await ShowStatus("Loading wave info from cache...");
            var wavePeaks = WavePeakData.FromDisk(peakWaveFileName);
            //_audioVisualizer.WavePeaks = wavePeaks;
        }

        _videoFileName = videoFileName;

        //if (!_stopping)
        //{
        //    _timer.Start();
        //}
    }

    private void VideoCloseFile()
    {
        VideoPlayerControl?.Close();
    }

    private int GetFastSubtitleHash()
    {
        var pre = _subtitleFileName + SelectedEncoding.DisplayName;
        unchecked // Overflow is fine, just wrap
        {
            var hash = 17;
            hash = hash * 23 + pre.GetHashCode();

            if (_subtitle.Header != null)
            {
                hash = hash * 23 + _subtitle.Header.Trim().GetHashCode();
            }

            if (_subtitle.Footer != null)
            {
                hash = hash * 23 + _subtitle.Footer.Trim().GetHashCode();
            }

            var max = Subtitles.Count;
            for (var i = 0; i < max; i++)
            {
                var p = Subtitles[i];
                hash = hash * 23 + p.Number.GetHashCode();
                hash = hash * 23 + p.StartTime.TotalMilliseconds.GetHashCode();
                hash = hash * 23 + p.EndTime.TotalMilliseconds.GetHashCode();

                foreach (var line in p.Text.SplitToLines())
                {
                    hash = hash * 23 + line.GetHashCode();
                }

                //if (p.P.Style != null)
                //{
                //    hash = hash * 23 + p.P.Style.GetHashCode();
                //}
                //if (p.P.Extra != null)
                //{
                //    hash = hash * 23 + p.P.Extra.GetHashCode();
                //}
                //if (p.P.Actor != null)
                //{
                //    hash = hash * 23 + p.P.Actor.GetHashCode();
                //}
                //hash = hash * 23 + p.P.Layer.GetHashCode();
            }

            return hash;
        }
    }

    public void DeleteSelectedItems()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? new List<SubtitleLineViewModel>();
        if (selectedItems != null && selectedItems.Any())
        {
            foreach (var item in selectedItems)
            {
                Subtitles.Remove(item);
            }
        }
    }

    public void InsertAfterSelectedItem()
    {
        var selectedItem = SelectedSubtitle;
        if (selectedItem != null)
        {
            var index = Subtitles.IndexOf(selectedItem);
            var newItem = new SubtitleLineViewModel(); // Initialize with appropriate values

            if (index >= 0)
            {
                Subtitles.Insert(index + 1, newItem);
                SelectedSubtitle = newItem;
                //TODO: SubtitleGrid.SelectedItem = newItem;
            }
        }
    }

    public void ToggleItalic()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? new List<SubtitleLineViewModel>();
        if (selectedItems != null && selectedItems.Any())
        {
            foreach (var item in selectedItems)
            {
                item.Text = item.Text.Contains("<i>")
                    ? item.Text.Replace("<i>", "").Replace("</i>", "")
                    : $"<i>{item.Text}</i>";
            }
        }
    }

    public void SubtitleContextOpening(object? sender, EventArgs e)
    {
    }

    public void KeyDown(KeyEventArgs keyEventArgs)
    {
        _shortcutManager.OnKeyPressed(this, keyEventArgs);

        var relayCommand = _shortcutManager.CheckShortcuts(ShortcutCategory.SubtitleGrid.ToStringInvariant());
        if (relayCommand != null)
        {
            keyEventArgs.Handled = true;
            relayCommand.Execute(null);
            return;
        }

        relayCommand = _shortcutManager.CheckShortcuts(ShortcutCategory.General.ToStringInvariant());
        if (relayCommand != null)
        {
            keyEventArgs.Handled = true;
            relayCommand.Execute(null);
            return;
        }
    }
    
    public void OnKeyUpHandler(object? sender, KeyEventArgs e)
    {
        _shortcutManager.OnKeyReleased(this, e);
    }

    private bool _subtitleGridIsRightClick = false;
    private bool _subtitleGridIsLeftClick = false;
    private bool _subtitleGridIsControlPressed = false;

    public void SubtitleGrid_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _subtitleGridIsControlPressed = false;
        _subtitleGridIsLeftClick = false;
        _subtitleGridIsRightClick = false;

        if (sender is Control { ContextFlyout: not null } control)
        {
            var props = e.GetCurrentPoint(control).Properties;
            _subtitleGridIsLeftClick = props.IsLeftButtonPressed;
            _subtitleGridIsRightClick = props.IsRightButtonPressed;
            _subtitleGridIsControlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        }
    }

    public void SubtitleGrid_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Control { ContextFlyout: MenuFlyout menuFlyout } control)
        {
            if (_subtitleGridIsRightClick)
            {
                menuFlyout.ShowAt(control, true);
            }

            if (OperatingSystem.IsMacOS())
            {
                if (_subtitleGridIsLeftClick && _subtitleGridIsControlPressed)
                {
                    menuFlyout.ShowAt(control, true);
                    e.Handled = true;
                }
            }
        }
    }

    public void SubtitleGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedItems = SubtitleGrid.SelectedItems;

        if (selectedItems == null)
        {
            SelectedSubtitle = null;
            SelectedSubtitleIndex = null;
            _selectedSubtitles = null;
            StatusTextRight = string.Empty;
            EditTextCharactersPerSecond = string.Empty;
            EditTextTotalLength = string.Empty;
            EditTextLineLengths = string.Empty;
            return;
        }

        _selectedSubtitles = selectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count > 1)
        {
            StatusTextRight = $"{selectedItems.Count} lines selected of {Subtitles.Count}";
            EditTextCharactersPerSecond = string.Empty;
            EditTextTotalLength = string.Empty;
            EditTextLineLengths = string.Empty;
            return;
        }

        var item = _selectedSubtitles.FirstOrDefault();
        if (item == null)
        {
            SelectedSubtitle = null;
            SelectedSubtitleIndex = null;
            StatusTextRight = string.Empty;
            return;
        }

        SelectedSubtitle = item;
        SelectedSubtitleIndex = Subtitles.IndexOf(item);
        StatusTextRight = $"{item.Number}/{Subtitles.Count}";

        MakeSubtitleTextInfo(item.Text, item);
    }

    private void MakeSubtitleTextInfo(string text, SubtitleLineViewModel item)
    {
        text = HtmlUtil.RemoveHtmlTags(text, true);
        var totalLength = text.CountCharacters(false);
        var cps = new Paragraph(text, item.StartTime.TotalMilliseconds, item.EndTime.TotalMilliseconds)
            .GetCharactersPerSecond();

        var lines = text.SplitToLines();
        var lineLenghts = new List<string>(lines);
        for (var i = 0; i < lines.Count; i++)
        {
            lineLenghts[i] = $"{lines[i].Length}";
        }

        EditTextCharactersPerSecond = $"Chars/sec: {cps:0.0}";
        EditTextTotalLength = $"Total length: {totalLength}";
        EditTextLineLengths = $"Line length: {string.Join('/', lineLenghts)}";
    }

    private DispatcherTimer _positionTimer = new DispatcherTimer();

    private void StartTitleTimer()
    {
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _positionTimer.Tick += (s, e) =>
        {
            var text = "Untitled";
            if (!string.IsNullOrEmpty(_subtitleFileName))
            {
                text = Configuration.Settings.General.TitleBarFullFileName
                    ? _subtitleFileName
                    : Path.GetFileName(_subtitleFileName);
            }

            text = text + " - " + "Subtitle Edit 5.0 Alpha";
            if (_changeSubtitleHash != GetFastSubtitleHash())
            {
                text = "*" + text;
            }

            Window.Title = text;
        };
        _positionTimer.Start();
    }


    public void SubtitleTextChanged(object? sender, TextChangedEventArgs e)
    {
        var selectedSubtitle = SelectedSubtitle;
        if (selectedSubtitle == null)
        {
            return;
        }

        MakeSubtitleTextInfo(selectedSubtitle.Text, selectedSubtitle);
    }
}