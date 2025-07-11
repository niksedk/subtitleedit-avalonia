using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using static Nikse.SubtitleEdit.Logic.FindService;

namespace Nikse.SubtitleEdit.Features.Edit.Find;

public partial class FindViewModel : ObservableObject
{
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private bool _wholeWord;
    [ObservableProperty] private bool _findTypeNormal;
    [ObservableProperty] private bool _findTypeCanseInsensitive;
    [ObservableProperty] private bool _findTypeRegularExpression;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public FindViewModel()
    {
        SearchText = string.Empty;

        LoadSettings();
    }

    private void LoadSettings()
    {
        WholeWord = Se.Settings.Edit.Find.FindWholeWords;

        if (Se.Settings.Edit.Find.FindSearchType == FindMode.CaseInsensitive.ToString())
        {
            FindTypeCanseInsensitive = true;
        }
        else if (Se.Settings.Edit.Find.FindSearchType == FindMode.CaseSensitive.ToString())
        {
            FindTypeNormal = true;
        }
        else
        {
            FindTypeRegularExpression = true;
        }
    }

    private void SaveSettings()
    {
        Se.Settings.Edit.Find.FindWholeWords = WholeWord;
        Se.Settings.Edit.Find.FindSearchType.ToString();
    }

    [RelayCommand]
    private void FindPrevious()
    {
        OkPressed = true;
        SaveSettings();
        Window?.Close();
    }

    [RelayCommand]
    private void FindNext()
    {
        OkPressed = true;
        SaveSettings();
        Window?.Close();
    }

    [RelayCommand]
    private void Count()
    {
        SaveSettings();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void FindTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            FindNext();
        }
    }
}