using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Edit.Replace;

public partial class ReplaceViewModel : ObservableObject
{
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private bool _wholeWord;
    [ObservableProperty] private string _replaceText;
    [ObservableProperty] private bool _findTypeNormal;
    [ObservableProperty] private bool _findTypeCanseInsensitive;
    [ObservableProperty] private bool _findTypeRegularExpression;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public ReplaceViewModel()
    {
        SearchText = string.Empty;
        ReplaceText = string.Empty;

        LoadSettings();
    }

    private void LoadSettings()
    {
        WholeWord = Se.Settings.Edit.FindWholeWords;
    }

    [RelayCommand]
    private void Replace()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void ReplaceAll()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void FindNext()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Count()
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