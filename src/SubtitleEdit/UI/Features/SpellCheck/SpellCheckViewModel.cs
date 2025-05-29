using System;
using System.Collections.ObjectModel;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

public partial class SpellCheckViewModel : ObservableObject
{

    [ObservableProperty] private string _lineText;
    [ObservableProperty] private string _wholeText;
    [ObservableProperty] private string _word;
    [ObservableProperty] private ObservableCollection<string> _dictionaries;
    [ObservableProperty] private string _selectedDictionary;
    [ObservableProperty] private ObservableCollection<string> _suggestions;
    [ObservableProperty] private string _selectedSuggestion;

    public SpellCheckWindow? Window { get; set; }
    
    public bool OkPressed { get; private set; }

    public SpellCheckViewModel()
    {
        WholeText = string.Empty;
        Word = string.Empty;
        Dictionaries = new ObservableCollection<string>();
        SelectedDictionary = string.Empty;
        Suggestions = new ObservableCollection<string>();
        SelectedSuggestion = string.Empty;

        LoadDictionaries();
    }

    private void LoadDictionaries()
    {
        
    }

    [RelayCommand]                   
    private void EditWholeText() 
    {
        OkPressed = true;
        Window?.Close();
    }
    
    [RelayCommand]                   
    private void Change() 
    {
        Window?.Close();
    }

    [RelayCommand]
    private void ChangeAll()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void Skip()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void SkipAll()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void AddToNamesList()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void AddToUserDictionary()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void GoogleIt()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void BrowseDictionary()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void SuggestionUseOnce()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void SuggestionUseAlways()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void Ok()
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
}