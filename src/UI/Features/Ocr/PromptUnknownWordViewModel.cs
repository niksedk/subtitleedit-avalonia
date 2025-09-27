using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Ocr;

public partial class PromptUnknownWordViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _suggestions;
    [ObservableProperty] private string _selectedSuggestion;
    [ObservableProperty] private string? _text;
    [ObservableProperty] private string _wholeText;
    [ObservableProperty] private string _word;

    public Bitmap? Bitmap { get; set; }
    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }


    public PromptUnknownWordViewModel()
    {
        Suggestions = new ObservableCollection<string>();
        SelectedSuggestion = string.Empty;
        Text = string.Empty;
        WholeText = string.Empty;
        Word = string.Empty;
    }

    public void Initialize(Bitmap bitmap, string wholeText, string word)
    {
        Bitmap = bitmap;
        WholeText = wholeText;
        Word = word;
    }

    [RelayCommand]
    private void EditWholeText()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void EditWordOnly()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void SuggestionUse()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void SuggestionUseAlways()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Change()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void SkipOnce()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void GoogleIt()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void SkipAll()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void AddToNamesList()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void AddToUserDictionary()
    {
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
}