using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitToolbar
{
    public static Border Make(MainViewModel vm)
    {
        return new Border
        {
            Height = 40,
            Child = CreateToolbar(vm),
        };
    }

    private static StackPanel CreateToolbar(MainViewModel vm)
    {
        var path = $"Assets/Themes/{Se.Settings.Appearance.Theme}/";

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Margin = new Avalonia.Thickness(5),
        };

        var appearance = Se.Settings.Appearance;
        if (appearance.ToolbarShowFileNew)
        {
            stackPanel.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(path + "New.png"),
                    Width = 32,
                    Height = 32,
                },
                [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandFileNewCommand)),
            });
        }

        if (appearance.ToolbarShowFileOpen)
        {
            stackPanel.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(path + "Open.png"),
                    Width = 32,
                    Height = 32,
                },
                [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandFileOpenCommand)),
            });
        }

        if (appearance.ToolbarShowSave)
        {
            stackPanel.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(path + "Save.png"),
                    Width = 32,
                    Height = 32,
                },
                [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandFileSaveCommand)),
            });
        }

        if (appearance.ToolbarShowSaveAs)
        {
            stackPanel.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(path + "SaveAs.png"),
                    Width = 32,
                    Height = 32,
                },
                [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandFileSaveAsCommand)),
            });
        }

        stackPanel.Children.Add(MakeSeparator());

        if (appearance.ToolbarShowFind)
        {
            stackPanel.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(path + "Find.png"),
                    Width = 32,
                    Height = 32,
                },
            });
        }

        if (appearance.ToolbarShowSaveAs)
        {
            stackPanel.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(path + "Replace.png"),
                    Width = 32,
                    Height = 32,
                },
            });
        }

        stackPanel.Children.Add(MakeSeparator());

        if (appearance.ToolbarShowSpellCheck)
        {
            stackPanel.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(path + "SpellCheck.png"),
                    Width = 32,
                    Height = 32,
                },
            });
        }

        if (appearance.ToolbarShowFind)
        {
            stackPanel.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(path + "Settings.png"),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.CommandShowSettingsCommand,
            });
        }

        if (appearance.ToolbarShowReplace)
        {
            stackPanel.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(path + "Layout.png"),
                    Width = 32,
                    Height = 32,
                },
                [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandShowLayoutCommand)),
            });
        }

        stackPanel.Children.Add(MakeSeparator());

        if (appearance.ToolbarShowReplace)
        {
            stackPanel.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = new Bitmap(path + "Help.png"),
                    Width = 32,
                    Height = 32,
                },
            });
        }

        stackPanel.Children.Add(MakeSeparator());

        // subtitle formats
        stackPanel.Children.Add(new TextBlock
        {
            Text = "Subtitle Format",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(5, 0, 0, 0),
        });
        stackPanel.Children.Add(new ComboBox
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

        stackPanel.Children.Add(MakeSeparator());

        // encoding
        stackPanel.Children.Add(new TextBlock
        {
            Text = "Encoding",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(5, 0, 0, 0),
        });
        stackPanel.Children.Add(new ComboBox
        {
            Width = 200,
            Height = 30,
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Encodings)),
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedEncoding)),
            DataContext = vm,
        });

        return stackPanel;
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