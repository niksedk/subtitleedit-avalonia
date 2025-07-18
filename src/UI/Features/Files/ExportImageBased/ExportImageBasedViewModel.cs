using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public partial class ExportImageBasedViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private string _translatedTitle;
    [ObservableProperty] private string _originalTitle;
    [ObservableProperty] private string _translator;
    [ObservableProperty] private string _comment;
    [ObservableProperty] private string _language;
    [ObservableProperty] private TimeSpan _startOfProgramme;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public ExportImageBasedViewModel()
    {
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        TranslatedTitle = string.Empty;
        OriginalTitle = string.Empty;
        Translator = string.Empty;
        Comment = string.Empty;
        Language = string.Empty;
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private void Importl()
    {
    }
    
    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }

    public void Initialize(ObservableCollection<SubtitleLineViewModel> observableCollection, string? subtitleFileName, string? videoFileName)
    {
    }
}