using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using Projektanker.Icons.Avalonia;
using System;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Logic;

public static class UiUtil
{
    public const int WindowMarginWidth = 12;
    public const int CornerRadius = 4;
    public const int SplitterWidthOrHeight = 4;

    public static readonly ControlTheme DataGridNoBorderCellTheme = new ControlTheme(typeof(DataGridCell))
    {
        Setters =
        {
            new Setter(DataGridCell.BorderThicknessProperty, new Thickness(0)),
            new Setter(DataGridCell.BorderBrushProperty, Brushes.Transparent),
            new Setter(DataGridCell.BackgroundProperty, Brushes.Transparent),
            new Setter(DataGridCell.FocusAdornerProperty, null),
            new Setter(DataGridCell.PaddingProperty, new Thickness(4)),
        }
    };

    public static readonly ControlTheme DataGridNoBorderNoPaddingCellTheme = new ControlTheme(typeof(DataGridCell))
    {
        Setters =
        {
            new Setter(DataGridCell.BorderThicknessProperty, new Thickness(0)),
            new Setter(DataGridCell.BorderBrushProperty, Brushes.Transparent),
            new Setter(DataGridCell.BackgroundProperty, Brushes.Transparent),
            new Setter(DataGridCell.FocusAdornerProperty, null),
            new Setter(DataGridCell.PaddingProperty, new Thickness(0)),
        }
    };

    public static Button MakeButton(string text)
    {
        return MakeButton(text, null);
    }

    public static IBrush GetTextColor(double opacity = 1.0)
    {
        var app = Application.Current;
        if (app == null)
        {
            new SolidColorBrush(Colors.Black, opacity);
        }

        var theme = app!.ActualThemeVariant;
        if (theme == ThemeVariant.Dark)
        {
            return new SolidColorBrush(Colors.White, opacity);
        }

        return new SolidColorBrush(Colors.Black, opacity);
    }

    public static IBrush GetBorderColor()
    {
        var app = Application.Current;
        if (app == null)
        {
            new SolidColorBrush(Colors.Black);
        }

        var theme = app!.ActualThemeVariant;
        if (theme == ThemeVariant.Dark)
        {
            return new SolidColorBrush(Colors.White, 0.5);
        }

        return new SolidColorBrush(Colors.Black, 0.5);
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

    public static Button MakeBrowseButton(IRelayCommand? command)
    {
        return new Button
        {
            Content = "...",
            Margin = new Thickness(4, 0),
            Padding = new Thickness(6, 6),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = command,
        };
    }

    public static Button MakeButtonOk(IRelayCommand? command)
    {
        return MakeButton(Se.Language.General.Ok, command);
    }

    public static Button MakeButtonDone(IRelayCommand? command)
    {
        return MakeButton(Se.Language.General.Done, command);
    }

    public static Button MakeButtonCancel(IRelayCommand? command)
    {
        return MakeButton(Se.Language.General.Cancel, command);
    }

    public static Button MakeButton(IRelayCommand? command, string iconName)
    {
        var button = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = command,
        };

        Attached.SetIcon(button, iconName);

        return button;
    }

    public static Button MakeButton(IRelayCommand? command, string iconName, int fontSize)
    {
        var button = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = command,
            FontSize = fontSize,
        };

        Attached.SetIcon(button, iconName);

        return button;
    }

    public static Button MakeButtonBrowse(IRelayCommand? command, string? propertyIsVisiblePath = null)
    {
        var button = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = command,
        };

        if (propertyIsVisiblePath != null)
        {
            button.Bind(Button.IsVisibleProperty, new Binding
            {
                Path = propertyIsVisiblePath,
            });
        }

        Attached.SetIcon(button, IconNames.MdiDotsHorizontal);

        return button;
    }

    public static Button BindIsEnabled(this Button control, object viewModal, string propertyIsEnabledPath)
    {
        control.Bind(Button.IsEnabledProperty, new Binding
        {
            Path = propertyIsEnabledPath,
            Mode = BindingMode.OneWay,
            Source = viewModal,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        return control;
    }

    public static ComboBox BindIsEnabled(this ComboBox control, object viewModal, string propertyIsEnabledPath)
    {
        control.Bind(ComboBox.IsEnabledProperty, new Binding
        {
            Path = propertyIsEnabledPath,
            Mode = BindingMode.OneWay,
            Source = viewModal,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        return control;
    }

    public static ComboBox BindIsEnabled(this ComboBox control, object viewModal, string propertyIsEnabledPath, IValueConverter converter)
    {
        control.Bind(ComboBox.IsEnabledProperty, new Binding
        {
            Path = propertyIsEnabledPath,
            Mode = BindingMode.OneWay,
            Source = viewModal,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            Converter = converter,
        });

        return control;
    }

    public static Button BindIsEnabled(this Button control, object viewModal, string propertyIsEnabledPath, IValueConverter converter)
    {
        control.Bind(Button.IsEnabledProperty, new Binding
        {
            Path = propertyIsEnabledPath,
            Mode = BindingMode.OneWay,
            Source = viewModal,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            Converter = converter,
        });

        return control;
    }

    public static TextBox BindIsEnabled(this TextBox control, object viewModal, string propertyIsEnabledPath, IValueConverter converter)
    {
        control.Bind(TextBox.IsEnabledProperty, new Binding
        {
            Path = propertyIsEnabledPath,
            Mode = BindingMode.OneWay,
            Source = viewModal,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            Converter = converter,
        });

        return control;
    }

    public static ComboBox MakeComboBox<T>(
        ObservableCollection<T> sourceLanguages,
        object viewModal,
        string? propertySelectedPath,
        string? propertyIsVisiblePath)
    {
        var comboBox = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Left,
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

    public static ComboBox MakeComboBox<T>(
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

    public static CheckBox MakeCheckBox(object viewModel, string? isCheckedPropertyPath)
    {
        var checkBox = new CheckBox
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = viewModel,
        };

        if (isCheckedPropertyPath != null)
        {
            checkBox.Bind(CheckBox.IsCheckedProperty, new Binding
            {
                Path = isCheckedPropertyPath,
                Mode = BindingMode.TwoWay,
            });
        }

        return checkBox;
    }

    public static CheckBox MakeCheckBox(string text, object viewModel, string? isCheckedPropertyPath)
    {
        var checkBox = new CheckBox
        {
            Content = text,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = viewModel,
        };

        if (isCheckedPropertyPath != null)
        {
            checkBox.Bind(CheckBox.IsCheckedProperty, new Binding
            {
                Path = isCheckedPropertyPath,
                Mode = BindingMode.TwoWay,
            });
        }

        return checkBox;
    }


    public static TextBlock MakeLink(string text, IRelayCommand command)
    {
        var link = new TextBlock
        {
            Text = text,
            Foreground = MakeLinkForeground(),
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

    public static SolidColorBrush MakeLinkForeground()
    {
        return new SolidColorBrush(Color.FromArgb(255, 30, 144, 255)); 
    }

    public static Button MakeMenuItem(string text, IRelayCommand command, object commandParameter, string iconName)
    {
        var label = new Label() { Content = text, Padding = new Thickness(4, 0, 0, 0) };
        var image = new ContentControl();
        Attached.SetIcon(image, iconName);
        var stackPanelApplyFixes = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { image, label }
        };

        var link = new Button
        {
            Content = stackPanelApplyFixes,
            FontWeight = FontWeight.DemiBold,
            Margin = new Thickness(0),
            Padding = new Thickness(10, 5, 10, 5),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Command = command,
            CommandParameter = commandParameter,
        };

        return link;
    }

    public static TextBlock MakeLink(string text, IRelayCommand command, object viewModel, string propertyTextPath)
    {
        var link = new TextBlock
        {
            Text = text,
            Foreground = MakeLinkForeground(),
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

    public static TextBlock WithMarginRight(this TextBlock control, int marginRight)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, marginRight, m.Bottom);
        return control;
    }

    public static TextBox WithMarginRight(this TextBox control, int marginRight)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, marginRight, m.Bottom);
        return control;
    }

    public static TextBox WithMarginLeft(this TextBox control, int marginLeft)
    {
        var m = control.Margin;
        control.Margin = new Thickness(marginLeft, m.Top, m.Right, m.Bottom);
        return control;
    }

    public static TextBox WithHeight(this TextBox control, int height)
    {
        var m = control.Margin;
        control.Height = height;
        return control;
    }

    public static TextBlock WithMarginLeft(this TextBlock control, int marginLeft)
    {
        var m = control.Margin;
        control.Margin = new Thickness(marginLeft, m.Top, m.Right, m.Bottom);
        return control;
    }

    public static TextBlock WithMarginBottom(this TextBlock control, int marginBottom)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, m.Right, marginBottom);
        return control;
    }

    public static Border WithMarginBottom(this Border control, int marginBottom)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, m.Right, marginBottom);
        return control;
    }
    
    public static Border WithMinWidth(this Border control, int minWidth)
    {
        control.MinWidth = minWidth;
        return control;
    }

    public static Border WithMinHeight(this Border control, int minHeight)
    {
        control.MinHeight = minHeight;
        return control;
    }

    public static Border WithHeight(this Border control, int height)
    {
        control.Height = height;
        return control;
    }

    public static Border WithMarginRight(this Border control, int marginRight)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, marginRight, m.Bottom);
        return control;
    }

    public static TextBlock WithMarginTop(this TextBlock control, int topBottom)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, topBottom, m.Right, m.Bottom);
        return control;
    }

    public static ComboBox WithMarginBottom(this ComboBox control, int marginBottom)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, m.Right, marginBottom);
        return control;
    }

    public static ComboBox WithMargin(this ComboBox control, int left, int top, int right, int bottom)
    {
        control.Margin = new Thickness(left, top, right, bottom);
        return control;
    }

    public static Button WithMarginBottom(this Button control, int marginBottom)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, m.Right, marginBottom);
        return control;
    }

    public static TextBlock WithBackgroundColor(this TextBlock control, IBrush brush)
    {
        control.Background = brush;
        return control;
    }

    public static Button WithLeftAlignment(this Button control)
    {
        control.HorizontalAlignment = HorizontalAlignment.Left;
        return control;
    }

    public static Button WithRightAlignment(this Button control)
    {
        control.HorizontalAlignment = HorizontalAlignment.Right;
        return control;
    }

    public static Button WithTopAlignment(this Button control)
    {
        control.VerticalAlignment = VerticalAlignment.Top;
        return control;
    }

    public static Button WithCenterAlignment(this Button control)
    {
        control.VerticalAlignment = VerticalAlignment.Center;
        return control;
    }

    public static Button WithBottomAlignment(this Button control)
    {
        control.VerticalAlignment = VerticalAlignment.Bottom;
        return control;
    }

    public static Button WithIconRight(this Button control, string iconName)
    {
        var label = new TextBlock() { Text = control.Content?.ToString(), Padding = new Thickness(0, 0, 4, 0) };
        var image = new ContentControl();
        Attached.SetIcon(image, iconName);
        var stackPanelApplyFixes = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { label, image }
        };

        control.Content = stackPanelApplyFixes;

        return control;
    }

    public static Button WithIconLeft(this Button control, string iconName)
    {
        var label = new TextBlock() { Text = control.Content?.ToString(), Padding = new Thickness(4, 0, 0, 0) };
        var image = new ContentControl();
        Attached.SetIcon(image, iconName);
        var stackPanelApplyFixes = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { image, label }
        };

        control.Content = stackPanelApplyFixes;

        return control;
    }

    public static Button WithCommandParameter<T>(this Button control, T parameter)
    {
        control.CommandParameter = parameter;
        return control;
    }

    public static ComboBox WithLeftAlignment(this ComboBox control)
    {
        control.HorizontalAlignment = HorizontalAlignment.Left;
        return control;
    }

    public static Button WithBindEnabled(this Button control, string isEnabledPropertyPath)
    {
        control.Bind(Button.IsEnabledProperty, new Binding
        {
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static CheckBox WithBindEnabled(this CheckBox control, string isEnabledPropertyPath)
    {
        control.Bind(CheckBox.IsEnabledProperty, new Binding
        {
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static TextBox WithBindEnabled(this TextBox control, string isEnabledPropertyPath)
    {
        control.Bind(TextBox.IsEnabledProperty, new Binding
        {
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Button WithBindEnabled(this Button control, string isEnabledPropertyPath, IValueConverter converter)
    {
        control.Bind(Button.IsEnabledProperty, new Binding
        {
            Converter = converter,
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static TextBox WithBindIsVisible(this TextBox control, string isVisiblePropertyPath)
    {
        control.Bind(TextBox.IsVisibleProperty, new Binding
        {
            Path = isVisiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static TextBox WithBindIsVisible(this TextBox control, string isEnabledPropertyPath, IValueConverter converter)
    {
        control.Bind(TextBox.IsVisibleProperty, new Binding
        {
            Converter = converter,
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Button WithBindIsVisible(this Button control, string isVisiblePropertyPath)
    {
        control.Bind(Button.IsVisibleProperty, new Binding
        {
            Path = isVisiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Button WithBindIsVisible(this Button control, string isVisiblePropertyPath, IValueConverter converter)
    {
        control.Bind(Button.IsVisibleProperty, new Binding
        {
            Path = isVisiblePropertyPath,
            Mode = BindingMode.TwoWay,
            Converter = converter,
        });

        return control;
    }

    public static Border WithBindIsVisible(this Border control, string isVisiblePropertyPath, IValueConverter converter)
    {
        control.Bind(Border.IsVisibleProperty, new Binding
        {
            Path = isVisiblePropertyPath,
            Mode = BindingMode.TwoWay,
            Converter = converter,
        });

        return control;
    }

    public static Border WithBindIsVisible(this Border control, string isVisiblePropertyPath)
    {
        control.Bind(Border.IsVisibleProperty, new Binding
        {
            Path = isVisiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Button WithBindIsEnabled(this Button control, string isEnabledPropertyPath)
    {
        control.Bind(Button.IsEnabledProperty, new Binding
        {
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Button WithBindIsEnabled(this Button control, string isEnabledPropertyPath, IValueConverter converter)
    {
        control.Bind(Button.IsEnabledProperty, new Binding
        {
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
            Converter = converter,
        });

        return control;
    }

    public static ComboBox WithBindSelected(this ComboBox control, string selectedPropertyBinding)
    {
        control.Bind(ComboBox.SelectedItemProperty, new Binding
        {
            Path = selectedPropertyBinding,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static TextBlock WithMargin(this TextBlock control, int margin)
    {
        control.Margin = new Thickness(margin);
        return control;
    }

    public static TextBlock WithPadding(this TextBlock control, int padding)
    {
        control.Padding = new Thickness(padding);
        return control;
    }

    public static TextBlock WithFontSize(this TextBlock control, double fontSize)
    {
        control.FontSize = fontSize;
        return control;
    }

    public static StackPanel WithMarginTop(this StackPanel control, int marginTop)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, marginTop, m.Right, m.Bottom);
        return control;
    }

    public static StackPanel WithAlignmentLeft(this StackPanel control)
    {
        control.HorizontalAlignment = HorizontalAlignment.Left;
        return control;
    }

    public static StackPanel WithAlignmentTop(this StackPanel control)
    {
        control.VerticalAlignment = VerticalAlignment.Top;
        return control;
    }

    public static Label WithAlignmentTop(this Label control)
    {
        control.VerticalAlignment = VerticalAlignment.Top;
        return control;
    }

    public static Label HorizontalContentAlignmentCenter(this Label control)
    {
        control.HorizontalContentAlignment = HorizontalAlignment.Center;
        return control;
    }

    public static Label WithBold(this Label control)
    {
        control.FontWeight = FontWeight.Bold;
        return control;
    }

    public static Label WithMarginLeft(this Label control, int marginLeft)
    {
        var m = control.Margin;
        control.Margin = new Thickness(marginLeft, m.Top, m.Right, m.Bottom);
        return control;
    }

    public static Label WithMarginTop(this Label control, int marginTop)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, marginTop, m.Right, m.Bottom);
        return control;
    }

    public static Label WithFontSize(this Label control, int fontSize)
    {
        control.FontSize = fontSize;
        return control;
    }

    public static ComboBox WithMarginTop(this ComboBox control, int marginTop)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, marginTop, m.Right, m.Bottom);
        return control;
    }

    public static ComboBox WithMarginLeft(this ComboBox control, int marginLeft)
    {
        var m = control.Margin;
        control.Margin = new Thickness(marginLeft, m.Top, m.Right, m.Bottom);
        return control;
    }

    public static ComboBox WithMarginRight(this ComboBox control, int marginRight)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, marginRight, m.Bottom);
        return control;
    }

    public static Button WithMarginTop(this Button control, int marginTop)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, marginTop, m.Right, m.Bottom);
        return control;
    }

    public static Button WithFontSize(this Button control, double fontSize)
    {
        control.FontSize = fontSize;
        return control;
    }

    public static Button WithMarginLeft(this Button control, int marginLeft)
    {
        var m = control.Margin;
        control.Margin = new Thickness(marginLeft, m.Top, m.Right, m.Bottom);
        return control;
    }

    public static Button WithMarginRight(this Button control, int marginRight)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, m.Right, marginRight);
        return control;
    }

    public static Button Compact(this Button control)
    {
        var m = control.Padding;
        control.Padding = new Thickness(8, m.Top, 8, m.Bottom);
        control.MinWidth = 0;
        return control;
    }

    public static Button WithMargin(this Button control, int margin)
    {
        control.Margin = new Thickness(margin);
        return control;
    }

    public static Button WithPadding(this Button control, int padding)
    {
        control.Padding = new Thickness(padding);
        return control;
    }

    public static Button WithMargin(this Button control, int left, int top, int right, int bottom)
    {
        control.Margin = new Thickness(left, top, right, bottom);
        return control;
    }

    public static Button WithBold(this Button control)
    {
        control.FontWeight = FontWeight.Bold;
        return control;
    }

    public static TextBlock WithMinwidth(this TextBlock control, int width)
    {
        control.MinWidth = width;
        return control;
    }

    public static Button WithMinWidth(this Button control, int width)
    {
        control.MinWidth = width;
        return control;
    }

    public static ComboBox WithMinWidth(this ComboBox control, int width)
    {
        control.MinWidth = width;
        return control;
    }

    public static ComboBox WithWidth(this ComboBox control, int width)
    {
        control.Width = width;
        return control;
    }

    public static StackPanel MakeButtonBar(params Control[] buttons)
    {
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(10, 20, 10, 10),
            Spacing = 0,
            Height = double.NaN, // Allow it to grow vertically if needed
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
            Background = GetBorderColor(),
            Margin = new Thickness(5, 5, 5, 5),
            VerticalAlignment = VerticalAlignment.Stretch,
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

    public static T BindIsVisible<T>(this T control, object vm, string visibilityPropertyPath) where T : Visual
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
        return new WindowIcon(AssetLoader.Open(new Uri("avares://SubtitleEdit/Assets/se.ico")));
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

    internal static ColorPicker MakeColorPicker(object vm, string colorPropertyPath)
    {
        return new ColorPicker
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            IsAlphaEnabled = true,
            IsAlphaVisible = true,
            IsColorSpectrumSliderVisible = false,
            IsColorComponentsVisible = true,
            IsColorModelVisible = false,
            IsColorPaletteVisible = false,
            IsAccentColorsVisible = false,
            IsColorSpectrumVisible = true,
            IsComponentTextInputVisible = true,
            [!ColorPicker.ColorProperty] = new Binding(colorPropertyPath)
            {
                Source = vm,
                Mode = BindingMode.TwoWay
            },
        };
    }

    internal static Label MakeLabel(string text)
    {
        return new Label
        {
            Content = text,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    internal static Label MakeLabel(string text, string propertyIsVisiblePath)
    {
        var label = new Label
        {
            Content = text,
            VerticalAlignment = VerticalAlignment.Center,
        };

        label.Bind(ComboBox.IsVisibleProperty, new Binding
        {
            Path = propertyIsVisiblePath,
            Mode = BindingMode.TwoWay,
        });

        return label;
    }

    internal static Control MakeLabel(Binding binding)
    {
        var label = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
        };

        label.Bind(Label.ContentProperty, binding);

        return label;
    }

    internal static RadioButton MakeRadioButton(string text, object viewModel, string isCheckedPropertyPath)
    {
        var control = new RadioButton
        {
            Content = text,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = viewModel,
        };

        if (isCheckedPropertyPath != null)
        {
            control.Bind(RadioButton.IsCheckedProperty, new Binding
            {
                Path = isCheckedPropertyPath,
                Mode = BindingMode.TwoWay,
            });
        }

        return control;
    }

    public static NumericUpDown MakeNumericUpDownInt(int min, int max, double width, object viewModel, string? propertyValuePath = null, string? propertyIsVisiblePath = null)
    {
        var control = new NumericUpDown
        {
            Width = width,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = viewModel,
            Minimum = min,
            Maximum = max,
            Increment = 1,
            FormatString = "F0",
        };

        if (propertyValuePath != null)
        {
            control.Bind(NumericUpDown.ValueProperty, new Binding
            {
                Path = propertyValuePath,
                Mode = BindingMode.TwoWay,
            });
        }

        if (propertyIsVisiblePath != null)
        {
            control.Bind(NumericUpDown.IsVisibleProperty, new Binding
            {
                Path = propertyIsVisiblePath,
                Mode = BindingMode.TwoWay,
            });
        }

        return control;
    }

    public static NumericUpDown MakeNumericUpDownTwoDecimals(decimal min, decimal max, double width, object viewModel, string? propertyValuePath = null, string? propertyIsVisiblePath = null)
    {
        var control = new NumericUpDown
        {
            Width = width,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = viewModel,
            Minimum = min,
            Maximum = max,
            Increment = 0.01m,
            FormatString = "F2" // Force two decimals
        };

        if (propertyValuePath != null)
        {
            control.Bind(NumericUpDown.ValueProperty, new Binding
            {
                Path = propertyValuePath,
                Mode = BindingMode.TwoWay,
            });
        }

        if (propertyIsVisiblePath != null)
        {
            control.Bind(NumericUpDown.IsVisibleProperty, new Binding
            {
                Path = propertyIsVisiblePath,
                Mode = BindingMode.TwoWay,
            });
        }

        return control;
    }

    public static Label WithBindText(this Label control, object viewModel, string contentPropertyPath)
    {
        control.DataContext = viewModel;
        control.Bind(Label.ContentProperty, new Binding
        {
            Path = contentPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static TextBlock WithBindText(this TextBlock control, object viewModel, string contentPropertyPath)
    {
        control.DataContext = viewModel;
        control.Bind(TextBlock.TextProperty, new Binding
        {
            Path = contentPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Label WithBindVisible(this Label control, object viewModel, string visiblePropertyPath)
    {
        control.DataContext = viewModel;
        control.Bind(Label.IsVisibleProperty, new Binding
        {
            Path = visiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Grid WithBindVisible(this Grid control, object viewModel, string visiblePropertyPath)
    {
        control.DataContext = viewModel;
        control.Bind(Grid.IsVisibleProperty, new Binding
        {
            Path = visiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static TextBlock WithBindVisible(this TextBlock control, object viewModel, string visiblePropertyPath)
    {
        control.DataContext = viewModel;
        control.Bind(TextBlock.IsVisibleProperty, new Binding
        {
            Path = visiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Label WithBindVisible(this Label control, object viewModel, string visiblePropertyPath, IValueConverter converter)
    {
        control.DataContext = viewModel;
        control.Bind(Label.IsVisibleProperty, new Binding
        {
            Path = visiblePropertyPath,
            Mode = BindingMode.TwoWay,
            Converter = converter,
        });

        return control;
    }

    public static StackPanel WithBindVisible(this StackPanel control, object viewModel, string visiblePropertyPath)
    {
        control.DataContext = viewModel;
        control.Bind(StackPanel.IsVisibleProperty, new Binding
        {
            Path = visiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static StackPanel WithBindVisible(this StackPanel control, object viewModel, string visiblePropertyPath, IValueConverter converter)
    {
        control.DataContext = viewModel;
        control.Bind(StackPanel.IsVisibleProperty, new Binding
        {
            Path = visiblePropertyPath,
            Mode = BindingMode.TwoWay,
            Converter = converter,
        });

        return control;
    }

    internal static object MakeLabel(object topBottomMargin)
    {
        throw new NotImplementedException();
    }
}