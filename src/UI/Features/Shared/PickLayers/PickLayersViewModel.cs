using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.PickLayers;

public partial class PickLayersViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<LayerItem> _layers;
    [ObservableProperty] private LayerItem? _selectedLayer;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public List<int> SelectedLayers { get; private set; }

    public PickLayersViewModel()
    {
        Layers = new ObservableCollection<LayerItem>();
        SelectedLayers = new List<int>();
    }

    internal void Initialize(List<SubtitleLineViewModel> subtitleLineViewModels, List<int>? visibleLayers)
    {
        visibleLayers ??= new List<int>();

        foreach (var layer in subtitleLineViewModels.Select(p=>p.Layer).Distinct().OrderBy(p=>p))
        {
            var item = new LayerItem(layer, visibleLayers.Contains(layer)); 
            Layers.Add(item);
        }
    }

    [RelayCommand]
    private void Ok()
    {
        SelectedLayers = Layers.Where(l => l.IsSelected).Select(l => l.Layer).ToList();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}