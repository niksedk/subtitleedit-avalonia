using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Help;

public partial class AboutViewModel : ObservableObject
{
    public Window? Window { get; set; }

    public string TitleText => $"Subtitle Edit {Se.Version}";
    public string LicenseText => "Subtitle Edit is free software under the MIT license.";
    public string DescriptionText =>
        "Subtitle Edit 5 previews are early versions of the next major release." + Environment.NewLine +
        "Some features may be missing, incomplete or experimental." + Environment.NewLine +
        "We welcome your feedback to help improve the final version." + Environment.NewLine +
        Environment.NewLine +
        "Thank you for testing and supporting Subtitle Edit :)";

    [RelayCommand]
    private async Task OpenGithub()
    {
        if (Window == null)
        {
            return;
        }

        await Window.Launcher.LaunchUriAsync(new Uri("https://github.com/niksedk/subtitleedit-avalonia"));
    }

    [RelayCommand]
    private async Task OpenPaypal()
    {
        if (Window == null)
        {
            return;
        }
        
        await Window.Launcher.LaunchUriAsync(new Uri("https://www.paypal.com/donate/?hosted_button_id=4XEHVLANCQBCU"));
    }

    [RelayCommand]
    private async Task OpenGithubSponsor()
    {
        if (Window == null)
        {
            return;
        }
        
        await Window.Launcher.LaunchUriAsync(new Uri("https://github.com/sponsors/niksedk"));
    }

    [RelayCommand]
    private void Ok()
    {
        Window?.Close();
    }

    public void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
