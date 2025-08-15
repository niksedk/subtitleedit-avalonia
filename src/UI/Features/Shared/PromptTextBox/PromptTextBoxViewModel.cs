using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.PromptTextBox;

public partial class PromptTextBoxViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _text;
    [ObservableProperty] private int _textBoxWidth;
    [ObservableProperty] private int _textBoxHeight;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public PromptTextBoxViewModel()
    {
        Title = string.Empty;
        Text = string.Empty;
    }

    internal void Initialize(string title, string text, int textBoxWidth, int textBoxHeight)
    {
        Title = title;
        Text = text;
        TextBoxWidth = textBoxWidth;
        TextBoxHeight = textBoxHeight;
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