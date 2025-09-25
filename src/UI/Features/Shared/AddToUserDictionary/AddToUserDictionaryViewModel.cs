using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Shared.AddToUserDictionary;

public partial class AddToUserDictionaryViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<SpellCheckDictionaryDisplay> _dictionaries;
    [ObservableProperty] private SpellCheckDictionaryDisplay? _selectedDictionary;
    [ObservableProperty] private string _word;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public AddToUserDictionaryViewModel()
    {
        Title = string.Empty;
        Word = string.Empty;
        Dictionaries = new ObservableCollection<SpellCheckDictionaryDisplay>();
    }

    internal void Initialize(string word, SpellCheckDictionaryDisplay? dictionary = null)
    {
        Word = word.Trim();
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Ok();
        }
    }
}