using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Shared.AddToNamesList;

public partial class AddToNamesListViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<SpellCheckDictionaryDisplay> _dictionaries;
    [ObservableProperty] private SpellCheckDictionaryDisplay? _selectedDictionary;
    [ObservableProperty] private string _name;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public AddToNamesListViewModel()
    {
        Title = string.Empty;
        Name = string.Empty;
        Dictionaries = new ObservableCollection<SpellCheckDictionaryDisplay>();
    }

    internal void Initialize(string name, SpellCheckDictionaryDisplay? dictionary = null)
    {
        Name = name.Trim();
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