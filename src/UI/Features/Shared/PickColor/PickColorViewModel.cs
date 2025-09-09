using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickColor;

public partial class PickColorViewModel : ObservableObject
{
    [ObservableProperty] private Color _selectedColor = Colors.White;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public ColorView ColorView { get; internal set; }

    public PickColorViewModel()
    {
        ColorView = new ColorView();
        LoadSettings();
    }

    public void Initialize(Color initialColor)
    {
        SelectedColor = initialColor;
    }

    private void LoadSettings()
    {
        // Load any color picker settings if needed
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
