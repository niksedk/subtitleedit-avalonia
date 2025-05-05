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

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Margin = new Avalonia.Thickness(5),
            Children =
            {
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap(path + "New.png"),
                        Width = 32,
                        Height = 32,
                    },
                    [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandFileNewCommand)),
                },
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap(path + "Open.png"),
                        Width = 32,
                        Height = 32,
                    },
                    [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandFileOpenCommand)),
                },
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap(path + "Save.png"),
                        Width = 32,
                        Height = 32,
                    },
                    [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandFileSaveCommand)),
                },
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap(path + "SaveAs.png"),
                        Width = 32,
                        Height = 32,
                    },
                    [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandFileSaveAsCommand)),
                },
                new Border
                {
                    Width = 1,
                    Background = Brushes.Gray,
                    Margin = new Avalonia.Thickness(5, 5, 5, 5),
                },
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap(path + "Find.png"),
                        Width = 32,
                        Height = 32,
                    },
                },
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap(path + "Replace.png"),
                        Width = 32,
                        Height = 32,
                    },
                },
                MakeSeparator(),
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap(path + "SpellCheck.png"),
                        Width = 32,
                        Height = 32,
                    },
                },
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap(path + "Settings.png"),
                        Width = 32,
                        Height = 32,                       
                    },
                    Command = vm.CommandShowSettingsCommand,
                },
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap(path + "Layout.png"),
                        Width = 32,
                        Height = 32,
                    },
                    [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandShowLayoutCommand)),
                },
                MakeSeparator(),
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap(path + "Help.png"),
                        Width = 32,
                        Height = 32,
                    },
                },
                MakeSeparator(),

                // subtitle formats
                new TextBlock
                {
                    Text = "Subtitle Format",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Avalonia.Thickness(5, 0, 0, 0),
                },
                new ComboBox
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
                },

                MakeSeparator(),

                // encoding
                new TextBlock
                {
                    Text = "Encoding",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Avalonia.Thickness(5, 0, 0, 0),
                },
                new ComboBox
                {
                    Width = 200,
                    Height = 30,
                    [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Encodings)),
                    [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedEncoding)),
                    DataContext = vm,
                },
            }
        };
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