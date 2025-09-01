using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Shared.SourceView;

public partial class SourceViewViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _text;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public SourceViewViewModel()
    {
        Title = string.Empty;
        Text = string.Empty;
    }

    internal void Initialize(string title, string text)
    {
        Title = title;
        Text = text;
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
}