using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Shared.Bookmarks;

public partial class BookmarksListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public BookmarksListViewModel()
    {
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void DeleteSelectedLine()
    {
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
        Subtitles.AddRange(subtitleLineViewModels);
    }

    internal void GridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
    }

    internal void OnBookmarksGridDoubleTapped(object? sender, TappedEventArgs e)
    {
    }
}