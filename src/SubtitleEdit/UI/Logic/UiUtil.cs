using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Translate;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Logic;

public static class UiUtil
{
    public const int WindowMarginWidth = 10;

    public static Button MakeButton(string text, IRelayCommand command)
    {
        return new Button
        {
            Content = text,
            Margin = new Thickness(0),
            Padding = new Thickness(12, 6), 
            MinWidth = 80,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = command,
        };
    }

    public static Control MakeComboBox<T>(ObservableCollection<T> sourceLanguages, T selectedAutoTranslator)
    {
        var comboBox = new ComboBox
        {
            Width = 150,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        comboBox.ItemsSource = sourceLanguages;
        comboBox.SelectedItem = selectedAutoTranslator;
        return comboBox;
    }

    public static Control MakeTextBlock(string text)
    {
        return new TextBlock
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    public static TextBlock MakeLink(string text, IRelayCommand command)
    {
        var link = new TextBlock
        {
            Text = text,
            Foreground = Brushes.Blue,
            TextDecorations = TextDecorations.Underline,
            Cursor = new Cursor(StandardCursorType.Hand),
            Margin = new Thickness(0),
            Padding = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        link.PointerPressed += (_, __) =>
        {
            if (command.CanExecute(null))
            {
                command.Execute(null);
            }
        };

        return link;
    }


    public static StackPanel MakeButtonBar(params Control[] buttons)
    {
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(10),
            Spacing = 10,
        };

        stackPanel.Children.AddRange(buttons);

        return stackPanel;
    }

    public static Border MakeSeparatorForHorizontal()
    {
        return new Border
        {
            Width = 1,
            Background = Brushes.Gray,
            Margin = new Thickness(5, 5, 5, 5),
            VerticalAlignment = VerticalAlignment.Center,
        };
    }
}