using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Ocr;
using System;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public partial class NOcrDbEditViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _characters;
    [ObservableProperty] private string? _selectedCharacter;
    [ObservableProperty] private ObservableCollection<NOcrChar> _currentCharacterItems;
    [ObservableProperty] private NOcrChar? _selectedCurrentCharacterItem;
    [ObservableProperty] private string _itemText;
    [ObservableProperty] private bool _isItemItalic;
    [ObservableProperty] private string _databaseName;

    public Window? Window { get; set; }


    public bool OkPressed { get; set; }
    private NOcrDb _nOcrDb;

    public NOcrDbEditViewModel()
    {
        DatabaseName = string.Empty;
    }


    [RelayCommand]
    private void Ok()
    {
        if (string.IsNullOrWhiteSpace(DatabaseName))
        {
            return;
        }

        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private void DeleteCurrentItem()
    {
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void KeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    internal void TextBoxDatabaseNameKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Ok();
        }
    }

    internal void Initialize(NOcrDb nOcrDb)
    {
        _nOcrDb = nOcrDb;
    }
}
