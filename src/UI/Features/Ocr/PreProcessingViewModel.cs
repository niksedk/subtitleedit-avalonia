using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr;

public partial class PreProcessingViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _models;
    [ObservableProperty] private string? _selectedModel;
    [ObservableProperty] private string _title;
    private string? _oldModel;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public PreProcessingViewModel()
    {
        Models = new ObservableCollection<string>();
        Title = string.Empty;
    }

    public void Initialize()
    {
       
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}