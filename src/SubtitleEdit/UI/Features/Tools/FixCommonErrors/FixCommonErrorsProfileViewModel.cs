using System.Collections.Generic;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public partial class FixCommonErrorsProfileViewModel : ObservableObject
{
    public ObservableCollection<ProfileDisplayItem> Profiles { get; set; }
    [ObservableProperty] private ProfileDisplayItem? _selectedProfile;
    [ObservableProperty] private bool _isProfileSelected;

    public FixCommonErrorsProfileWindow? Window { get; set; }

    public bool OkPressed { get; private set; }
    private List<FixRuleDisplayItem> _fixRules;

    public FixCommonErrorsProfileViewModel()
    {
        _fixRules = new List<FixRuleDisplayItem>();
        Profiles = new ObservableCollection<ProfileDisplayItem>();  
        SelectedProfile = null;
        IsProfileSelected = true;
    }
    
    public void Initialize(List<FixRuleDisplayItem> allFixRules)
    {
        _fixRules = allFixRules;

        foreach (var rule in Se.Settings.Tools.FixCommonErrors.Profiles)
        {
            var profile = new ProfileDisplayItem
            {
                Name = rule.ProfileName,
                FixRules = new ObservableCollection<FixRuleDisplayItem>()
            };

            foreach (var fixRule in allFixRules)
            {
                var displayItem = allFixRules.Find(x => x.Name == fixRule.Name);
                var isSelected = displayItem != null;
                profile.FixRules.Add(new FixRuleDisplayItem(fixRule) { IsSelected = isSelected });
            }

            Profiles.Add(profile);
        }
    }

    [RelayCommand]
    private void NewProfile()
    {
        var newProfile = new ProfileDisplayItem
        {
            Name = "Untitled",
            FixRules = new ObservableCollection<FixRuleDisplayItem>(_fixRules)
        };

        Profiles.Add(newProfile);
        SelectedProfile = newProfile;
    }
    
    [RelayCommand]
    private void Delete(ProfileDisplayItem? profile)
    {
        if (profile == null || !Profiles.Contains(profile))
        {
            return;
        }

        Profiles.Remove(profile);
        SelectedProfile = Profiles.Count > 0 ? Profiles[0] : null;
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