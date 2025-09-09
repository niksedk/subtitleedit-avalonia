using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Shared.PickFontName;

public partial class PickFontNameViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _fontNames;
    [ObservableProperty] private string? _selectedFontName;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public PickFontNameViewModel()
    {
        FontNames = new ObservableCollection<string>(FontHelper.GetSystemFonts());
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

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Initialize(SubtitleLineViewModel? selectedSubtitle, int count)
    {
        
    }

    internal void DataGridFontNameSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
    }
}