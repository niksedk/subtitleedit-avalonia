using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.RosettaProperties;

public partial class TmpegEncXmlPropertiesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _fontNames;
    [ObservableProperty] private string _selectedFontName;
    [ObservableProperty] private decimal _fontHeight;
    [ObservableProperty] private int _position;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public TmpegEncXmlPropertiesViewModel()
    {
        FontNames = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        SelectedFontName = FontNames.First();

        FontHeight = Se.Settings.Formats.TmpegEncXmlFontHeight;

        Position = Se.Settings.Formats.TmpegEncXmlPosition;

        LoadSettings();
    }

    private void LoadSettings()
    {
      
    }

    private void SaveSettings()
    {
        

        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
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