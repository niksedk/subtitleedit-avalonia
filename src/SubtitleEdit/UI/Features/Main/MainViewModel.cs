using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

namespace Nikse.SubtitleEdit.Features.Main;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> subtitles;

    [ObservableProperty] private SubtitleLineViewModel? selectedSubtitle;

    [ObservableProperty] private string editText;

    [ObservableProperty] private ObservableCollection<SubtitleFormat> subtitleFormats;
    public SubtitleFormat SelectedSubtitleFormat { get; set; }

    [ObservableProperty] private ObservableCollection<TextEncoding> encodings;
    public TextEncoding? SelectedEncoding { get; set; }

    [ObservableProperty] private string statusText;

    public DataGrid SubtitleGrid { get; set; }
    public TextBox EditTextBox { get; set; }
    public MainView View { get; set; }
    public Window Window { get; set; }
    public Grid ContentGrid { get; set; }
    public MainView MainView { get; set; }
    public Grid VideoPlayer { get; internal set; }
    public Grid Waveform { get; internal set; }

    private string? _subtitleFileName;
    private Subtitle _subtitle;

    private readonly IFileHelper _fileHelper;

    public MainViewModel(IFileHelper fileHelper)
    {
        _subtitle = new Subtitle();

        Subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            //new SubtitleLineViewModel
            //{
            //    Number = 1, StartTime = "00:00:10,500", EndTime = "00:00:13,000", Duration = "00:00:02,500",
            //    Text = "Hello, world!", IsVisible = true
            //},
            new SubtitleLineViewModel
            {
                Number = 2, StartTime = TimeSpan.FromSeconds(0), EndTime = TimeSpan.FromSeconds(1), Duration = TimeSpan.FromSeconds(1),
                Text = "This is a subtitle editor.",
            },
            new SubtitleLineViewModel
            {
                Number = 3, StartTime = TimeSpan.FromSeconds(10), EndTime = TimeSpan.FromSeconds(1), Duration = TimeSpan.FromSeconds(1),
                Text = "Navigate with arrow keys.",
            },
            new SubtitleLineViewModel
            {
                Number = 4, StartTime = TimeSpan.FromSeconds(20), EndTime = TimeSpan.FromSeconds(1), Duration = TimeSpan.FromSeconds(1),
                Text = "Edit text in the box below.",
            },
            new SubtitleLineViewModel
            {
                Number = 5, StartTime = TimeSpan.FromSeconds(30), EndTime = TimeSpan.FromSeconds(1), Duration = TimeSpan.FromSeconds(1),
                Text = "Press Ctrl+Enter to save changes.",
            }
        };

        SubtitleFormats = [.. SubtitleFormat.AllSubtitleFormats];
        SelectedSubtitleFormat = SubtitleFormats[0];

        Encodings = new ObservableCollection<TextEncoding>(EncodingHelper.GetEncodings());
        SelectedEncoding = Encodings[0];

        _fileHelper = fileHelper;

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
    }

    [RelayCommand]
    private async Task CommandFileOpen()
    {
        var fileName = await _fileHelper.PickOpenSubtitleFile(Window, "Open Subtitle File");
        if (!string.IsNullOrEmpty(fileName))
        {
            await SubtitleOpen(fileName);
        }
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

    private async Task SubtitleOpen(string fileName)
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


        _subtitle = subtitle;
        SetSubtitles(_subtitle);
        ShowStatus($"Subtitle loaded: {fileName}");
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
        return false;
    }

    private async Task SaveSubtitle()
    {
        if (Subtitles == null || !Subtitles.Any())
        {
            //ShowStatus("Nothing to save");
            return;
        }

        if (string.IsNullOrEmpty(_subtitleFileName))
        {
            await SaveSubtitleAs();
            return;
        }

        var text = GetUpdateSubtitle().ToText(SelectedSubtitleFormat);
        await File.WriteAllTextAsync(_subtitleFileName, text);
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
        // Show save dialog
        var fileName = await _fileHelper.PickOpenSubtitleFile(Window, "Save Subtitle File");
        if (!string.IsNullOrEmpty(fileName))
        {
            _subtitleFileName = fileName;
            _subtitle.FileName = fileName;
            await SaveSubtitle();
        }
    }

    public void SubtitleGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SubtitleGrid.SelectedItem is SubtitleLineViewModel selectedLine)
        {
            SelectedSubtitle = selectedLine;
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
}