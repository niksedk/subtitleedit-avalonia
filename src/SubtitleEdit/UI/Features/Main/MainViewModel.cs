using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Main;

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