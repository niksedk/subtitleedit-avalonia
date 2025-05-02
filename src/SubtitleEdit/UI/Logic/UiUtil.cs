using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
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
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        comboBox.ItemsSource = sourceLanguages;
        comboBox.SelectedItem = selectedAutoTranslator;
        return comboBox;
    }

    public static TextBox MakeTextBox(int width, object viewModel, string propertyPath)
    {
        var textBox = new TextBox
        {
            Width = width,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        };

        textBox.Bind(TextBox.TextProperty, new Binding
        {
            Path = propertyPath,
            Mode = BindingMode.TwoWay,
        });

        return textBox;
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

    public static TextBlock MakeLink(string text, IRelayCommand command, object viewModel, string propertyPath)
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

        link.Bind(TextBlock.TextProperty, new Binding
        {
            Path = propertyPath,
            Mode = BindingMode.TwoWay,
        });

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

    public static StackPanel MakeControlBarLeft(params Control[] buttons)
    {
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
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