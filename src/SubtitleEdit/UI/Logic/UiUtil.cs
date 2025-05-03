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

    public static Button MakeButton(string text)
    {
        return MakeButton(text, null);
    }

    public static IBrush GetTextColor()     
    {
        //var faTheme = Application.Current?.Styles.OfType<FluentAvaloniaTheme>().FirstOrDefault();
        //faTheme.TryGetResource("TextFillColorPrimary", Application.Current.RequestedThemeVariant, out resource);
        // var found1 = this.TryGetResource("TheKey", this.ActualThemeVariant, out var result1);
        return new TextBlock().Foreground ?? new SolidColorBrush(Colors.Black);
    }

    public static Button MakeButton(string text, IRelayCommand? command)
    {
        return new Button
        {
            Content = text,
            Margin = new Thickness(4, 0),
            Padding = new Thickness(12, 6),
            MinWidth = 80,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = command,
        };
    }

    public static Control MakeComboBox<T>(
        ObservableCollection<T> sourceLanguages, 
        object viewModal, 
        string? propertySelectedPath, 
        string? propertyIsVisiblePath)
    {
        var comboBox = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        comboBox.ItemsSource = sourceLanguages;
        comboBox.DataContext = viewModal;

        if (propertySelectedPath != null)
        {
            comboBox.Bind(ComboBox.SelectedItemProperty, new Binding
            {
                Path = propertySelectedPath,
                Mode = BindingMode.TwoWay,
            });
        }

        if (propertyIsVisiblePath != null)
        {
            comboBox.Bind(ComboBox.IsVisibleProperty, new Binding
            {
                Path = propertyIsVisiblePath,
                Mode = BindingMode.TwoWay,
            });
        }

        return comboBox;
    }

    public static Control MakeComboBox<T>(
        ObservableCollection<T> sourceLanguages,
        object viewModal,
        string? propertySelectedPath)
    {
        return MakeComboBox(sourceLanguages, viewModal, propertySelectedPath, null);
    }

    public static TextBox MakeTextBox(int width, object viewModel, string propertyTextPath)
    {

        return MakeTextBox(width, viewModel, propertyTextPath, null);
    }

    public static TextBox MakeTextBox(int width, object viewModel, string? propertyTextPath, string? propertyIsVisiblePath)
    {
        var textBox = new TextBox
        {
            Width = width,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };

        textBox.DataContext = viewModel;

        if (propertyTextPath != null)
        {
            textBox.Bind(TextBox.TextProperty, new Binding
            {
                Path = propertyTextPath,
                Mode = BindingMode.TwoWay,
            });
        }

        if (propertyIsVisiblePath != null)
        {
            textBox.Bind(TextBox.IsVisibleProperty, new Binding
            {
                Path = propertyIsVisiblePath,
                Mode = BindingMode.TwoWay,
            });
        }

        return textBox;
    }

    public static TextBlock MakeTextBlock(string text)
    {
        return new TextBlock
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    public static TextBlock MakeTextBlock(string text, object viewModel, string? textPropertyPath, string? visibilityPropertyPath)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = viewModel,
        };

        if (textPropertyPath != null)
        {
            textBlock.Bind(TextBlock.TextProperty, new Binding
            {
                Path = textPropertyPath,
                Mode = BindingMode.TwoWay,
            });
        }

        if (visibilityPropertyPath != null)
        {
            textBlock.Bind(TextBlock.IsVisibleProperty, new Binding
            {
                Path = visibilityPropertyPath,
                Mode = BindingMode.TwoWay,
            });
        }

        return textBlock;
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

    public static TextBlock MakeLink(string text, IRelayCommand command, object viewModel, string propertyTextPath)
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
            Path = propertyTextPath,
            Mode = BindingMode.TwoWay,
        });

        return link;
    }

    public static TextBlock WithMarginRight(this TextBlock textBlock, int marginRight)
    {
        var m = textBlock.Margin;
        textBlock.Margin = new Thickness(m.Left, m.Top, marginRight, m.Bottom);
        return textBlock;
    }

    public static TextBlock WithMarginLeft(this TextBlock textBlock, int marginLeft)
    {
        var m = textBlock.Margin;
        textBlock.Margin = new Thickness(marginLeft, m.Top, m.Right, m.Bottom);
        return textBlock;
    }

    public static StackPanel MakeButtonBar(params Control[] buttons)
    {
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(10),
            Spacing = 0,
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
            Spacing = 0,
        };

        stackPanel.Children.AddRange(buttons);

        return stackPanel;
    }

    public static Border MakeSeparatorForHorizontal()
    {
        return new Border
        {
            Width = 1,
            Background = GetTextColor(), // Brushes.Gray,
            Margin = new Thickness(5, 5, 5, 5),
            VerticalAlignment = VerticalAlignment.Center,
        };
    }
}