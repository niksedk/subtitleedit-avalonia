using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public partial class FixCommonErrorsProfileViewModel : ObservableObject
{
    public ObservableCollection<ProfileDisplayItem> Profiles { get; set; }

    [ObservableProperty]
    private ProfileDisplayItem? _selectedProfile;

    public FixCommonErrorsProfileWindow? Window { get; set; }

    public bool OkPressed { get; private set; }

    public FixCommonErrorsProfileViewModel()
    {
        Profiles = new ObservableCollection<ProfileDisplayItem>();  
        SelectedProfile = null;
    }

    [RelayCommand]
    private void NewProfile()
    {
    }
    
    [RelayCommand]
    private void Delete()
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