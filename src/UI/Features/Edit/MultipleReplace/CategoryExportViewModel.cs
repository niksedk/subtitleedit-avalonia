using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class CategoryExportViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<RuleTreeNode> _profiles;
    [ObservableProperty] private RuleTreeNode? _selectedProfile;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public CategoryExportViewModel()
    {
        Profiles = new ObservableCollection<RuleTreeNode>();
    }

    public void Initialize(List<RuleTreeNode> profiles)
    {
        Profiles.Clear();
        foreach (var profile in profiles)
        {
            Profiles.Add(profile);
        }
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (Window == null)
        {
            return;
        }

        if (Profiles.Where(p => p.IsSelected).Count() == 0)
        {
            await MessageBox.Show(
                Window!,
                Se.Language.General.Error,
                $"No profile selected for export",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return;
        }

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