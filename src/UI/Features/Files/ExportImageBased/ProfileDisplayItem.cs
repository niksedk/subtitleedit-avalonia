using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public partial class ProfileDisplayItem : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private bool _isSelected;

    public ProfileDisplayItem(string displayName, bool isSelected)
    {
        Name = displayName;
        IsSelected = isSelected;
    }

    public override string ToString()
    {
        return Name;
    }
}