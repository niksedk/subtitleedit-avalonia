using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Shared.Bookmarks;

public partial class BookmarkEditViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _bookmarkText;
    [ObservableProperty] private ObservableCollection<AdjustDurationDisplay> _adjustTypes;
    [ObservableProperty] private double _adjustRecalculateOptimalCharacterPerSecond;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public BookmarkEditViewModel()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
    }

    private void SaveSettings()
    {
        Se.SaveSettings();
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

    internal void Initialize(string? bookmark)
    {
        Title = bookmark == null ? Se.Language.General.BookmarkAdd : Se.Language.General.BookmarkEdit;
        BookmarkText = bookmark ?? string.Empty;
    }
}