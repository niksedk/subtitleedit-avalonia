using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Logic;

public static class UiUtil
{
    public const int WindowMarginWidth = 10;
    public const int CornerRadius = 4;
    public const int SplitterWidthOrHeight = 4;

    public static Button MakeButton(string text)
    {
        return MakeButton(text, null);
    }

    public static IBrush GetTextColor()
    {
        return new TextBlock().Foreground ?? new SolidColorBrush(Colors.Black);
    }

    public static IBrush GetTextColor(double opacity)
    {
        var brush = new TextBlock().Foreground ?? new SolidColorBrush(Colors.Black, opacity);
        if (brush is ImmutableSolidColorBrush cb)
        {
            return new SolidColorBrush(cb.Color, opacity);
        }

        return brush;
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

    public static CheckBox MakeCheckBox()
    {
        return new CheckBox
        {
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
            HorizontalAlignment = HorizontalAlignment.Left,
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

    public static TextBlock MakeMenuItem(string text, IRelayCommand command, object commandParameter)
    {
        var link = new TextBlock
        {
            Text = text,
            FontWeight = FontWeight.Bold,
            FontSize = 16,
            Margin = new Thickness(0),
            Padding = new Thickness(10, 5, 10, 5),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };

        link.PointerEntered += (_, __) =>
        {
            link.Background = new SolidColorBrush(Colors.LightGray);
        };

        link.PointerExited += (_, __) =>
        {
            link.Background = Brushes.Transparent;
        };

        link.PointerPressed += (_, __) =>
        {
            if (command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
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

    public static TextBlock WithBackgroundColor(this TextBlock textBlock, IBrush brush)
    {
        textBlock.Background = brush;
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

    public static Border MakeBorderForControl(Control control)
    {
        return new Border
        {
            Child = control,
            BorderThickness = new Thickness(1),
            BorderBrush = GetTextColor(0.3d),
            Padding = new Thickness(5),
            CornerRadius = new CornerRadius(CornerRadius),
        };
    }

    public static T BindVisible<T>(this T control, object vm, string visibilityPropertyPath) where T : Visual
    {
        control.DataContext = vm;
        control.Bind(Visual.IsVisibleProperty, new Binding
        {
            Path = visibilityPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static T BindText<T>(this T control, object vm, string textPropertyPath) where T : TextBox
    {
        control.DataContext = vm;
        control.Bind(TextBox.TextProperty, new Binding
        {
            Path = textPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static WindowIcon? GetSeIcon()
    {
        return new WindowIcon(AssetLoader.Open(new Uri("avares://Nikse.SubtitleEdit/Assets/se.ico")));
    }

    public static Control RemoveControlFromParent(this Control control)
    {
        if (control.Parent is Panel parent)
        {
            parent.Children.Remove(control);
        }
        else if (control.Parent is Decorator decorator)
        {
            if (decorator.Child == control)
            {
                decorator.Child = null;
            }
        }
        else if (control.Parent is ContentControl contentControl)
        {
            if (contentControl.Content == control)
            {
                contentControl.Content = null;
            }
        }

        return control;
    }

    public static Control AddControlToParent(this Control control, Control parent)
    {
        if (parent is Panel panel)
        {
            panel.Children.Add(control);
        }
        else if (parent is Decorator decorator)
        {
            decorator.Child = control;
        }
        else if (parent is ContentControl contentControl2)
        {
            contentControl2.Content = control;
        }

        return control;
    }

    internal static Thickness MakeWindowMargin()
    {
        return new Thickness(WindowMarginWidth, WindowMarginWidth * 2, WindowMarginWidth, WindowMarginWidth);
    }
}