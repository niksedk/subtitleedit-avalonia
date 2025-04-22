using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit;

public class MainView : ViewBase
{
    protected override object Build()
    {
        var vm = new MainViewModel();
        DataContext = vm;

        var contentGrid = new Grid
        {
            Background = Brushes.White,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };

        var root = new DockPanel();

        // Menu bar
        root.Children.Add(new Menu
        {
            Height = 30,
            Background = Brushes.Black,
            DataContext = vm,
            Items =
            {
                new MenuItem
                {
                    Header = "_File",
                    Items =
                    {
                        new MenuItem { Header = "_New" },
                        new MenuItem { Header = "_Open..." },
                        new MenuItem { Header = "_Save" },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "E_xit",
                            [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandExitCommand))
                        }
                    }
                }
            }
        }.Dock(Dock.Top));

        // Toolbar
        root.Children.Add(new Border
        {
            Background = Brushes.Black,
            Height = 40,
            Child = CreateToolbar(),
        }.Dock(Dock.Top));

        // Footer
        root.Children.Add(new Border
        {
            Background = Brushes.Black,
            Height = 25,
            Child = new TextBlock
            {
                Text = "Footer: 00:00:00 / 00:05:00 | EN subtitles | Saved",
                Margin = new Thickness(10, 4),
                VerticalAlignment = VerticalAlignment.Center
            }
        }.Dock(Dock.Bottom));

        // Main content (fills all remaining space)
        root.Children.Add(new Grid
        {
            Background = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Children = { contentGrid }
        });

        return root;
    }
    
    public StackPanel CreateToolbar()
    {
        //var iconNew = AssetLoader.Open( new Uri($"Assets/Themes/Dark/New.png"));
        
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Background = Brushes.Black,
            Margin = new Avalonia.Thickness(5),
            Children =
            {
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap("Assets/Themes/Dark/New.png"),
                        Width = 32,
                        Height = 32,
                    },
                },
                new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap("Assets/Themes/Dark/Open.png"),
                        Width = 32,
                        Height = 32,
                    },
                },
                // Add more toolbar buttons here as needed
            }
        };
    }
}

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string _myObject = "Hello World";


    public void MyCommand(object? commandParameter)
    {
        MyObject = $"You called command with parameter: {commandParameter}";
    }


    [RelayCommand]
    private void CommandExit()
    {
        // Exit logic here
        Console.WriteLine("Exiting...");
        Environment.Exit(0);
    }
}