using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Common;
using Nikse.SubtitleEdit.Features.Help;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using Nikse.SubtitleEdit.Features.Options.Settings;

namespace Nikse.SubtitleEdit.Features.Main;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> subtitles;

    [ObservableProperty] private SubtitleLineViewModel? selectedSubtitle;
    [ObservableProperty] private int? selectedSubtitleIndex;

    [ObservableProperty] private string editText;

    [ObservableProperty] private ObservableCollection<SubtitleFormat> subtitleFormats;
    [ObservableProperty] private SubtitleFormat selectedSubtitleFormat;

    [ObservableProperty] private ObservableCollection<TextEncoding> encodings;
    public TextEncoding SelectedEncoding { get; set; }

    [ObservableProperty] private string statusText;

    public DataGrid SubtitleGrid { get; set; }
    public TextBox EditTextBox { get; set; }
    public MainView View { get; set; }
    public Window Window { get; set; }
    public Grid ContentGrid { get; set; }
    public MainView MainView { get; set; }

    public VideoView VideoPlayer { get; internal set; }
    public MediaPlayer MediaPlayer { get; set; }

    public Grid Waveform { get; internal set; }
    public MenuItem MenuReopen { get; internal set; }

    private string? _subtitleFileName;
    private Subtitle _subtitle;

    private string? _videoFileName;

    private int _changeSubtitleHash = -1;

    private readonly IFileHelper _fileHelper;
    private readonly IShortcutManager _shortcutManager;

    public MainViewModel(IFileHelper fileHelper, IShortcutManager shortcutManager)
    {
        _fileHelper = fileHelper;
        _shortcutManager = shortcutManager;

        _subtitle = new Subtitle();

        Subtitles = new ObservableCollection<SubtitleLineViewModel>();

        SubtitleFormats = [.. SubtitleFormat.AllSubtitleFormats];
        SelectedSubtitleFormat = SubtitleFormats[0];

        Encodings = new ObservableCollection<TextEncoding>(EncodingHelper.GetEncodings());
        SelectedEncoding = Encodings[0];

        statusText = string.Empty;
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
        var layoutModel = new LayoutModel();
        layoutModel.SelectedLayout = Se.Settings.General.LayoutNumber;
        var newWindow = new LayoutWindow(layoutModel);
        newWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        await newWindow.ShowDialog(Window);

        if (layoutModel.SelectedLayout != null && layoutModel.SelectedLayout != Se.Settings.General.LayoutNumber)
        {
            Se.Settings.General.LayoutNumber = InitLayout.MakeLayout(MainView, this, layoutModel.SelectedLayout.Value);
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
        await SubtitleOpen(recentFile.SubtitleFileName, recentFile.VideoFileName);
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
            VideoOpenFile(fileName);
        }
    }

    [RelayCommand]
    private void CommandVideoClose()
    {
        VideoCloseFile();
    }

    
    [RelayCommand]                   
    private void CommandShowSettings() 
    {                                
        var settingsWindow = new SettingsWindow                        
        {                                                              
          //  WindowStartupLocation = WindowStartupLocation.CenterOwner, 
          //  Icon = null, // optional: add icon if needed    
          //  Owner = this // 'this' is your current Window              
        };                                                             
                                                                 
        settingsWindow.Show(); // non-modal                            
    }                                
    
    
    
  
  
  
  
  
  

  

    // Or use ShowDialog(this) if you want it modal:
    // await settingsWindow.ShowDialog(this);
    
    
    private async Task SubtitleOpen(string fileName, string? videoFileName = null)
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
        SetSubtitles(_subtitle);
        ShowStatus($"Subtitle loaded: {fileName}");
        AddToRecentFiles();
        _changeSubtitleHash = GetFastSubtitleHash();

        if (!string.IsNullOrEmpty(videoFileName) && File.Exists(videoFileName))
        {
            VideoOpenFile(videoFileName);
        }
        else if (FindVideoFileName.TryFindVideoFileName(fileName, out videoFileName))
        {
            VideoOpenFile(videoFileName);
        }
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

    private bool IsEmpty => Subtitles.Count == 0 || string.IsNullOrEmpty(Subtitles[0].Text);

    public LibVLC LibVLC { get; internal set; }

    private bool HasChanges()
    {
        return !IsEmpty && _changeSubtitleHash != GetFastSubtitleHash();
    }

    private async Task SaveSubtitle()
    {
        if (Subtitles == null || !Subtitles.Any())
        {
            ShowStatus("Nothing to save");
            return;
        }

        if (string.IsNullOrEmpty(_subtitleFileName))
        {
            await SaveSubtitleAs();
            return;
        }

        var text = GetUpdateSubtitle().ToText(SelectedSubtitleFormat);
        await File.WriteAllTextAsync(_subtitleFileName, text);
        _changeSubtitleHash = GetFastSubtitleHash();
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
        var fileName = await _fileHelper.PickSaveSubtitleFile(Window, SelectedSubtitleFormat, "Save Subtitle File");
        if (!string.IsNullOrEmpty(fileName))
        {
            _subtitleFileName = fileName;
            _subtitle.FileName = fileName;
            await SaveSubtitle();
            AddToRecentFiles();
        }
    }

    private void AddToRecentFiles()
    {
        if (string.IsNullOrEmpty(_subtitleFileName))
        {
            return;
        }

        Se.Settings.File.AddToRecentFiles(_subtitleFileName, _videoFileName ?? string.Empty, SelectedSubtitleIndex ?? 0,
            SelectedEncoding.DisplayName);
        Se.SaveSettings();

        InitMenu.UpdateRecentFiles(this);
    }

    public void SubtitleGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SubtitleGrid.SelectedItem is SubtitleLineViewModel selected)
        {
            SelectedSubtitle = selected;
        }
        else
        {
            SelectedSubtitle = null;
        }
    }

    private void ShowStatus(string statusText)
    {
        StatusText = statusText;
    }

    internal void OnClosing()
    {
        MediaPlayer?.Dispose();
        //libVLC?.Dispose();

        Se.SaveSettings();
    }

    internal void OnLoaded()
    {
        if (Se.Settings.File.ShowRecentFiles)
        {
            var first = Se.Settings.File.RecentFiles.FirstOrDefault();
            if (first != null && File.Exists(first.SubtitleFileName))
            {
                using var _ = SubtitleOpen(first.SubtitleFileName, first.VideoFileName);
            }
        }
    }

    private void VideoOpenFile(string videoFileName)
    {
        if (VideoPlayer == null)
        {
            return;
        }

        //_timer.Stop();
        //_audioVisualizer.WavePeaks = null;
        //VideoPlayer.Source = MediaSource.FromFile(videoFileName);
        var media = new Media(LibVLC, new Uri(videoFileName));
        MediaPlayer.Play(media);

        var peakWaveFileName = WavePeakGenerator.GetPeakWaveFileName(videoFileName);
        if (!File.Exists(peakWaveFileName))
        {
            if (FfmpegHelper.IsFfmpegInstalled())
            {
                var tempWaveFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
                var process = WaveFileExtractor.GetCommandLineProcess(videoFileName, -1, tempWaveFileName,
                    Configuration.Settings.General.VlcWaveTranscodeSettings, out _);
                ShowStatus("Extracting wave info...");
                Task.Run(async () =>
                {
                    //await ExtractWaveformAndSpectrogram(process, tempWaveFileName, peakWaveFileName);
                });
            }
        }
        else
        {
            ShowStatus("Loading wave info from cache...");
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
        if (VideoPlayer == null)
        {
            return;
        }

        _videoFileName = string.Empty;
        MediaPlayer.Media = null;
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
        var selectedItems = SubtitleGrid.SelectedItems?.Cast<SubtitleLineViewModel>().ToList();
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
        var selectedItem = SubtitleGrid.SelectedItem as SubtitleLineViewModel;
        if (selectedItem != null)
        {
            var index = Subtitles.IndexOf(selectedItem);
            var newItem = new SubtitleLineViewModel(); // Initialize with appropriate values

            if (index >= 0)
            {
                Subtitles.Insert(index + 1, newItem);
                SubtitleGrid.SelectedItem = newItem;
            }
        }
    }

    public void ToggleItalic()
    {
        var selectedItems = SubtitleGrid.SelectedItems?.Cast<SubtitleLineViewModel>().ToList();
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
}