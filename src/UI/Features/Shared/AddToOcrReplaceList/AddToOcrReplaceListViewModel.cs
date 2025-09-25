using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Shared.AddToOcrReplaceList;

public partial class AddToOcrReplaceListViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<SpellCheckDictionaryDisplay> _dictionaries;
    [ObservableProperty] private SpellCheckDictionaryDisplay? _selectedDictionary;
    [ObservableProperty] private string _from;
    [ObservableProperty] private string _to;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public AddToOcrReplaceListViewModel()
    {
        Title = string.Empty;
        Dictionaries = new ObservableCollection<SpellCheckDictionaryDisplay>();
        From = string.Empty;
        To = string.Empty;
    }

    internal void Initialize(string from, SpellCheckDictionaryDisplay? dictionary = null)
    {
        From = from.Trim();
        SelectedDictionary = dictionary;
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