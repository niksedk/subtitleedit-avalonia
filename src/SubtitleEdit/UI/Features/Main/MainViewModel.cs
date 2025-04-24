using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Help;
using Nikse.SubtitleEdit.Features.Main.Layout;

namespace Nikse.SubtitleEdit.Features.Main;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string _myObject = "Hello World";

    public Window Window { get; set; }

    [RelayCommand]
    private void CommandExit()
    {
        Environment.Exit(0);
    }
    
    [RelayCommand]
    private async Task CommandShowLayout()
    {
        var newWindow = new LayoutWindow();
        await newWindow.ShowDialog(Window); 
    }
    
    [RelayCommand]
    private async Task CommandShowAbout()
    {
        var newWindow = new AboutWindow();
        await newWindow.ShowDialog(Window); 
    }
}