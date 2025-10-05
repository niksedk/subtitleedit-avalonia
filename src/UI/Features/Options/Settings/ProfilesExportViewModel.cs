using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.Validators;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class ProfilesExportViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ProfileDisplay> _profiles;
    [ObservableProperty] private ProfileDisplay? _selectedProfile;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public ProfilesExportViewModel()
    {
        Profiles = new ObservableCollection<ProfileDisplay>();
    }

    public void Initialize(List<ProfileDisplay> profiles, string profileName)
    {
        Profiles.Clear();
        foreach (var profile in profiles)
        {
            Profiles.Add(new ProfileDisplay(profile));
        }

        SelectedProfile = Profiles.FirstOrDefault(p => p.Name == profileName);
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

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}