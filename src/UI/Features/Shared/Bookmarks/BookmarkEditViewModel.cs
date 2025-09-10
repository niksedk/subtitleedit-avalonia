using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.Bookmarks;

public partial class BookmarkEditViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string? _bookmarkText;
    [ObservableProperty] private bool _showRemoveButton;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public BookmarkEditViewModel()
    {
        Title = string.Empty;
        BookmarkText = string.Empty;
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Delete()
    {
        BookmarkText = null;
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
        ShowRemoveButton = bookmark != null;
    }

    internal void OnTextBoxKeyDown(KeyEventArgs args)
    {
        if (args.Key == Key.Enter)
        {
            args.Handled = true;
            Ok();
        }
    }
}