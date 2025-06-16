using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public partial class BurnInResolutionPickerViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ResolutionItem> _resolutions;
    [ObservableProperty] private ResolutionItem? _selectedResolution;

    public BurnInResolutionPickerWindow? Window { get; set; }

    public bool OkPressed { get; private set; }

    public BurnInResolutionPickerViewModel()
    {
        Resolutions = new ObservableCollection<ResolutionItem>(ResolutionItem.GetResolutions());
    }

    [RelayCommand]
    private async Task Ok()
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