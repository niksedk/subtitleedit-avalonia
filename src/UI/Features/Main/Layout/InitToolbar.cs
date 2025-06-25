using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitToolbar
{
    public static Border Make(MainViewModel vm)
    {
        var toolbar = CreateToolbar(vm);

        return new Border
        {
            Height = 40,
            Child = toolbar,
        };
    }

    private static Grid CreateToolbar(MainViewModel vm)
    {
        var path = System.IO.Path.Combine(Se.ThemesFolder, Se.Settings.Appearance.Theme);

        var stackPanelLeft = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Margin = new Avalonia.Thickness(5),
            VerticalAlignment = VerticalAlignment.Top,
        };

        var appearance = Se.Settings.Appearance;
        var isLastSeparator = true;

        if (appearance.ToolbarShowFileNew)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(System.IO.Path.Combine(path, "New.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.CommandFileNewCommand,
                Background = Brushes.Transparent,
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowFileOpen)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(System.IO.Path.Combine(path, "Open.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.CommandFileOpenCommand,
                Background = Brushes.Transparent,
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowSave)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(System.IO.Path.Combine(path, "Save.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.CommandFileSaveCommand,
                Background = Brushes.Transparent,
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowSaveAs)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(System.IO.Path.Combine(path, "SaveAs.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.CommandFileSaveAsCommand,
                Background = Brushes.Transparent,
            });
            isLastSeparator = false;
        }

        if (!isLastSeparator)
        {
            stackPanelLeft.Children.Add(MakeSeparator());
            isLastSeparator = true;
        }

        if (appearance.ToolbarShowFind)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(System.IO.Path.Combine(path, "Find.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.ShowFindCommand,
                Background = Brushes.Transparent,
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowReplace)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(System.IO.Path.Combine(path, "Replace.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.ShowReplaceCommand,
                Background = Brushes.Transparent,
            });
            isLastSeparator = false;
        }


        if (!isLastSeparator)
        {
            stackPanelLeft.Children.Add(MakeSeparator());
            isLastSeparator = true;
        }

        if (appearance.ToolbarShowSpellCheck)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(System.IO.Path.Combine(path, "SpellCheck.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.ShowSpellCheckCommand,
                Background = Brushes.Transparent,
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowSettings)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(System.IO.Path.Combine(path, "Settings.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.CommandShowSettingsCommand,
                Background = Brushes.Transparent,
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowLayout)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(System.IO.Path.Combine(path, "Layout.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.CommandShowLayoutCommand,
                Background = Brushes.Transparent,
            });
            isLastSeparator = false;
        }

        if (!isLastSeparator)
        {
            stackPanelLeft.Children.Add(MakeSeparator());
        }

        if (appearance.ToolbarShowHelp)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(System.IO.Path.Combine(path, "Help.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.ShowHelpCommand,
                Background = Brushes.Transparent,
            });
        }


        var stackPanelRight = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Margin = new Avalonia.Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
        };

        // subtitle formats
        stackPanelRight.Children.Add(new TextBlock
        {
            Text = Se.Language.General.SubtitleFormat,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(5, 0, 0, 0),
        });
        stackPanelRight.Children.Add(new ComboBox
        {
            Width = 200,
            Height = 30,
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.SubtitleFormats)),
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedSubtitleFormat)),
            DataContext = vm,
            ItemTemplate = new FuncDataTemplate<object>((item, _) =>
                new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleFormat.Name)),
                    Width = 150,
                }, true)
        });
        isLastSeparator = false;


        if (!isLastSeparator && appearance.ToolbarShowEncoding)
        {
            stackPanelRight.Children.Add(MakeSeparator());
        }

        if (appearance.ToolbarShowEncoding)
        {
            stackPanelRight.Children.Add(new TextBlock
            {
                Text = Se.Language.General.Encoding,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(5, 0, 0, 0),
            });
            stackPanelRight.Children.Add(new ComboBox
            {
                Width = 200,
                Height = 30,
                [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Encodings)),
                [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedEncoding)),
                DataContext = vm,
            });
        }

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
        };

        grid.Add(stackPanelLeft, 0, 0);
        grid.Add(stackPanelRight, 0, 1);

        return grid;
    }

    private static Border MakeSeparator()
    {
        return new Border
        {
            Width = 1,
            Background = Brushes.Gray,
            Margin = new Avalonia.Thickness(5, 5, 5, 5),
        };
    }
}