using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Common;
using Nikse.SubtitleEdit.Features.Help;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Options.Language;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Features.Translate;
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
using Avalonia.Threading;

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

    public IVideoPlayerInstance? _videoPlayerInstance { get; internal set; }
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
                // Save logic here
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
    }

    [RelayCommand]
    private async Task CommandShowAbout()
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
    private async Task CommandShowAutoTranslate()
    {
        var viewModel = await _windowService.ShowDialogAsync<AutoTranslateWindow, AutoTranslateViewModel>(Window,
            viewModel => { viewModel.Initialize(GetUpdateSubtitle()); });

        if (viewModel.OkPressed)
        {
            Console.WriteLine("User confirmed the action");
        }
    }

    [RelayCommand]
    private async Task CommandShowSettings()
    {
        var oldTheme = Se.Settings.Appearance.Theme;

        var viewModel = await _windowService.ShowDialogAsync<SettingsWindow, SettingsViewModel>(Window);
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
    }

    [RelayCommand]
    private async Task CommandShowSettingsLanguage()
    {
        var viewModel = await _windowService.ShowDialogAsync<LanguageWindow, LanguageViewModel>(Window);
        if (viewModel.OkPressed)
        {
            // todo
        }
    }

    [RelayCommand]
    private async Task DeleteSelectedLines()
    {
        await DeleteSelectedItems();
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
        //if (SubtitlesSource is FlatTreeDataGridSource<SubtitleLineViewModel> source &&
        //    source.Selection is ITreeDataGridRowSelectionModel<SubtitleLineViewModel> selection &&
        //    SubtitleGrid is TreeDataGrid treeGrid)
        //{
        //    var currentIndex = selection.SelectedIndexes.FirstOrDefault().Count > 0
        //        ? selection.SelectedIndexes.FirstOrDefault()[0]
        //        : -1;
        //    if (currentIndex < 0 || currentIndex + 1 >= source.Rows.Count)
        //    {
        //        return;
        //    }

        //    var newIndex = currentIndex + 1;

        //    //Dispatcher.UIThread.Post(() =>
        //    //{
        //    selection.Clear();
        //    selection.Select(new IndexPath(newIndex)); // Use IndexPath constructor to create a valid index
        //    SubtitleGrid.RowsPresenter?.BringIntoView(newIndex);
        //    // }, DispatcherPriority.Background);
        //}
    }

    [RelayCommand]
    private void GoToPreviousLine()
    {
        //if (SubtitlesSource is FlatTreeDataGridSource<SubtitleLineViewModel> source &&
        //    source.Selection is ITreeDataGridRowSelectionModel<SubtitleLineViewModel> selection &&
        //    SubtitleGrid is TreeDataGrid treeGrid)
        //{
        //    var currentIndex = selection.SelectedIndexes.FirstOrDefault().Count > 0
        //        ? selection.SelectedIndexes.FirstOrDefault()[0]
        //        : -1;
        //    if (currentIndex < 1)
        //    {
        //        return;
        //    }

        //    var newIndex = currentIndex - 1;

        //    // Dispatcher.UIThread.Post(() =>
        //    //  {
        //    selection.Clear();
        //    selection.Select(new IndexPath(newIndex)); // Use IndexPath constructor to create a valid index
        //    SubtitleGrid.RowsPresenter?.BringIntoView(newIndex);
        //    //}, DispatcherPriority.Render);
        //}
    }

    private Control _fullscreenBeforeParent;

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
    private async Task Unbreak()
    {
        var s = SelectedSubtitle;
        if (s == null)
        {
            return;
        }

        s.Text = Utilities.UnbreakLine(s.Text);
    }

    [RelayCommand]
    private async Task AutoBreak()
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
        if (SubtitlesSource is FlatTreeDataGridSource<SubtitleLineViewModel> source)
        {
            if (source.Selection is ITreeDataGridRowSelectionModel<SubtitleLineViewModel> x)
            {
                x.Clear();
                for (var i = 0; i < source.Items.Count(); i++)
                {
                    x.Select(i);
                }
            }
        }
    }

    private void InverseRowSelection()
    {
        if (SubtitlesSource is FlatTreeDataGridSource<SubtitleLineViewModel> source)
        {
            if (source.Selection is ITreeDataGridRowSelectionModel<SubtitleLineViewModel> selection)
            {
                // Fix for CA1826: Use a for loop instead of LINQ methods
                var oldIndices = new HashSet<int>();
                foreach (var indexPath in selection.SelectedIndexes)
                {
                    if (indexPath.Count > 0)
                    {
                        oldIndices.Add(indexPath[0]);
                    }
                }

                selection.Clear();
                for (var i = 0; i < source.Items.Count(); i++)
                {
                    if (!oldIndices.Contains(i))
                    {
                        selection.Select(new IndexPath(i)); // Use IndexPath constructor to create a valid index
                    }
                }
            }
        }
    }
    
    // Method to select a specific row by index and make it visible
    public void SelectAndScrollToRow(int index)
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

    public async Task DeleteSelectedItems()
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

        var relayCommand = _shortcutManager.CheckShortcuts("grid");
        if (relayCommand != null)
        {
            keyEventArgs.Handled = true;
            relayCommand.Execute(null);
            return;
        }

        relayCommand = _shortcutManager.CheckShortcuts(null);
        if (relayCommand != null)
        {
            keyEventArgs.Handled = true;
            relayCommand.Execute(null);
            return;
        }
    }

    public void KeyUp(KeyEventArgs keyEventArgs)
    {
        _shortcutManager.OnKeyReleased(this, keyEventArgs);
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
            SelectedSubtitle = null;
            SelectedSubtitleIndex = null;
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
            string separator = " + ";
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