using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Shared.PickLayers;
public partial class LayerItem : ObservableObject
{
    [ObservableProperty] private bool _isSelected;
    public int Layer { get; set; }

    public LayerItem(int layer, bool isSelected)
    {
        Layer = layer;
        IsSelected = isSelected;
    }
}
