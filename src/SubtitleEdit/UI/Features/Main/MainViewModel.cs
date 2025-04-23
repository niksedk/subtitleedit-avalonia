using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main.Layout;

namespace Nikse.SubtitleEdit.Features.Main;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string _myObject = "Hello World";

    public Window Window { get; set; }

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
    
    [RelayCommand]
    private async Task CommandShowLayout()
    {
        var newWindow = new LayoutView();
        await newWindow.ShowDialog(Window); 
    }
}