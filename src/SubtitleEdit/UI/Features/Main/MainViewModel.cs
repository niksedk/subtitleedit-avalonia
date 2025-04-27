using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Help;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Main;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> subtitles;

    [ObservableProperty] private SubtitleLineViewModel? selectedSubtitle;

    [ObservableProperty] private string editText;

    public DataGrid SubtitleGrid { get; set; }
    public TextBox EditTextBox { get; set; }
    public MainView View { get; set; }
    public Window Window { get; set; }
    public Grid ContentGrid { get; set; }
    public MainView MainView { get; set; }
    public Grid VideoPlayer { get; internal set; }
    public Grid Waveform { get; internal set; }

    private readonly IFileHelper _fileHelper;

    public MainViewModel(IFileHelper fileHelper)
    {
        Subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new SubtitleLineViewModel
            {
                Number = 1, StartTime = "00:00:10,500", EndTime = "00:00:13,000", Duration = "00:00:02,500",
                Text = "Hello, world!", IsVisible = true
            },
            new SubtitleLineViewModel
            {
                Number = 2, StartTime = "00:00:14,000", EndTime = "00:00:17,500", Duration = "00:00:03,500",
                Text = "This is a subtitle editor.", IsVisible = true
            },
            new SubtitleLineViewModel
            {
                Number = 3, StartTime = "00:00:18,000", EndTime = "00:00:21,000", Duration = "00:00:03,000",
                Text = "Navigate with arrow keys.", IsVisible = true
            },
            new SubtitleLineViewModel
            {
                Number = 4, StartTime = "00:00:22,000", EndTime = "00:00:25,000", Duration = "00:00:03,000",
                Text = "Edit text in the box below.", IsVisible = false
            },
            new SubtitleLineViewModel
            {
                Number = 5, StartTime = "00:00:26,000", EndTime = "00:00:30,000", Duration = "00:00:04,000",
                Text = "Press Ctrl+Enter to save changes.", IsVisible = true
            }
        };

        _fileHelper = fileHelper;
    }

    [RelayCommand]
    private void CommandExit()
    {
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
    private async Task CommandFileOpen()
    {
        var fileName = await _fileHelper.PickAndShowSubtitleFile(Window, "Open Subtitle File");
        if (!string.IsNullOrEmpty(fileName))
        {
            // Load the subtitle file and update the Subtitles collection
            // This is just a placeholder for actual loading logic
            Subtitles.Clear();
            Subtitles.Add(new SubtitleLineViewModel
            {
                Number = 1,
                StartTime = "00:00:00,000",
                EndTime = "00:00:05,000",
                Duration = "00:00:05,000",
                Text = "Loaded subtitle text from file.",
                IsVisible = true
            });
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
}