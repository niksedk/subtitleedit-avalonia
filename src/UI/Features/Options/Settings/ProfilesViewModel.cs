using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class ProfilesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ProfileDisplay> _profiles;
    [ObservableProperty] private ProfileDisplay? _selectedProfile;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;

    public ProfilesViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;

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
    private async Task Export()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService
            .ShowDialogAsync<ProfilesExportWindow, ProfilesExportViewModel>(Window, vm =>
            {
                //vm.Initialize(_profilesForEdit, SelectedProfile);
            });


        if (!result.OkPressed)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task Import()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.Options.Settings.OpenRuleFile, "Rule profile", ".profile");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }
    }

    [RelayCommand]
    private void Copy()
    {
        if (SelectedProfile == null)
        {
            return;
        }

        var newProfile = new ProfileDisplay(SelectedProfile);
        var idx = Profiles.IndexOf(SelectedProfile);

        newProfile.Name = SelectedProfile.Name + " 2";
        Profiles.Insert(idx + 1, newProfile);
    }

    [RelayCommand]
    private void Delete()
    {
        if (SelectedProfile == null)
        {
            return;
        }

        var idx = Profiles.IndexOf(SelectedProfile);
        Profiles.Remove(SelectedProfile);
        if (Profiles.Count == 0)
        {
            SelectedProfile = null;
        }
        else if (idx >= Profiles.Count)
        {
            SelectedProfile = Profiles[Profiles.Count - 1];
        }
        else
        {
            SelectedProfile = Profiles[idx];
        }
    }

    [RelayCommand]
    private void Clear()
    {
        Profiles.Clear();
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