using System.Collections.Generic;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public partial class FixCommonErrorsProfileViewModel : ObservableObject
{
    public ObservableCollection<ProfileDisplayItem> Profiles { get; set; }
    [ObservableProperty] private ProfileDisplayItem? _selectedProfile;

    public FixCommonErrorsProfileWindow? Window { get; set; }

    public bool OkPressed { get; private set; }
    public List<FixRuleDisplayItem> FixRules { get; set; }

    public FixCommonErrorsProfileViewModel()
    {
        Profiles = new ObservableCollection<ProfileDisplayItem>();  
        Profiles.Add(new ProfileDisplayItem() { Name = "Default", FixRules = new ObservableCollection<FixRuleDisplayItem>()});
        Profiles.Add(new ProfileDisplayItem() { Name = "Default2" });
        SelectedProfile = null;
    }
    
    public void Initialize(List<FixRuleDisplayItem> allFixRules)
    {
        FixRules = allFixRules;

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
                if (displayItem != null)
                {
                    displayItem.IsSelected = true;
                }
                profile.FixRules.Add(new FixRuleDisplayItem(fixRule));
            }

            Profiles.Add(profile);
        }
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