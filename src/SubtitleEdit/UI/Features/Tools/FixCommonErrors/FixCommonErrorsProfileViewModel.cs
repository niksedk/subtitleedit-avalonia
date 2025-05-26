using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public partial class FixCommonErrorsProfileViewModel : ObservableObject
{
    public ObservableCollection<ProfileDisplayItem> Profiles { get; set; }
    [ObservableProperty] private ProfileDisplayItem? _selectedProfile;
    [ObservableProperty] private bool _isProfileSelected;

    public FixCommonErrorsProfileWindow? Window { get; set; }

    public bool OkPressed { get; private set; }
    public TextBox ProfileNameTextBox { get; internal set; }

    private List<FixRuleDisplayItem> _fixRules;

    public FixCommonErrorsProfileViewModel()
    {
        _fixRules = new List<FixRuleDisplayItem>();
        Profiles = new ObservableCollection<ProfileDisplayItem>();
        SelectedProfile = null;
        IsProfileSelected = true;
        ProfileNameTextBox = new TextBox();
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

        if (Profiles.Count > 0)
        {
            SelectedProfile = Profiles[0];
        }
    }

    [RelayCommand]
    private void NewProfile()
    {
        var newProfile = new ProfileDisplayItem
        {
            Name = string.Empty,
            FixRules = new ObservableCollection<FixRuleDisplayItem>(_fixRules.Select(p => new FixRuleDisplayItem(p))),
        };

        Profiles.Add(newProfile);
        SelectedProfile = newProfile;

        Dispatcher.UIThread.Invoke(() =>
        {
            ProfileNameTextBox.Focus();
        });
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