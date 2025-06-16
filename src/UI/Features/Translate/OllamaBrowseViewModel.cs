using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Nikse.SubtitleEdit.Features.Shared;
using Projektanker.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class OllamaBrowseViewModel : ObservableObject
{
    public class OllamaResponse
    {
        public List<OllamaModelDisplay> Models { get; set; } = [];
    }   
    
    [ObservableProperty] private ObservableCollection<OllamaModelDisplay> _models;
    [ObservableProperty] private OllamaModelDisplay? _selectedModel;
    
    public OllamaBrowseWindow? Window { get; set; }
    
    public bool OkPressed { get; private set; }
    
    private readonly HttpClient _httpClient;

    public OllamaBrowseViewModel()
    {
        Models = [];
        _httpClient = new HttpClient();
    }
    
    [RelayCommand]                   
    private void Ok() 
    {
        OkPressed = true;
        Window?.Close();
    }
    
    public async Task InitializeAsync(string baseUrl)
    {
        try
        {
            Models.AddRange(await GetModelsAsync(baseUrl));
            if (Models.Count == 1)
            {
                SelectedModel = Models[0];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching models: {ex.Message}");
        }
    }
    
    [RelayCommand]                   
    private void Cancel() 
    {
        Window?.Close();
    }

    private async Task<List<OllamaModelDisplay>> GetModelsAsync(string baseUrl)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{baseUrl}/api/tags");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<OllamaResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return apiResponse?.Models ?? [];
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                await MessageBox.Show(Window!, 
                    "Error", 
                    $"Failed to fetch models from Ollama API: {ex.Message}",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            });
            
            return [];
        }
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