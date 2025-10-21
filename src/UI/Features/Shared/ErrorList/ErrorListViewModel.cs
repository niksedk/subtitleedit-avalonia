using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.ErrorList;

public partial class ErrorListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ErrorListItem> _subtitles;
    [ObservableProperty] private ErrorListItem? _selectedSubtitle;
    [ObservableProperty] private bool _hasErrors;

    public Window? Window { get; set; }

    public bool GoToPressed { get; private set; }

    public ErrorListViewModel()
    {
        Subtitles = new ObservableCollection<ErrorListItem>();
    }
    
    [RelayCommand]
    private void GoTo()
    {
        GoToPressed = true;
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

    internal void Initialize(List<SubtitleLineViewModel> subtitleLineViewModels)
    {
        foreach (var subtitleLine in subtitleLineViewModels)
        {
            Subtitles.Add(new ErrorListItem(subtitleLine));
        }
        
        HasErrors = SelectedSubtitle != null;
    }

    internal void GridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        HasErrors = SelectedSubtitle != null;
    }

    internal void OnBookmarksGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        Dispatcher.UIThread.Invoke(GoTo);
    }

    internal void GridKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            GoTo();
        }
    }
}