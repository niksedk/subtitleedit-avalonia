using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using System;

namespace Nikse.SubtitleEdit.Features.Help;

public class AboutWindow : Window
{
    public AboutWindow()
    {
        Icon = UiUtil.GetSeIcon();
        Title = "About Subtitle Edit";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var titleText = new TextBlock
        {
            Text = "Subtitle Edit version 5.0, Alpha 1",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
        };

        var licenseText = new TextBlock
        {
            Text = "Subtitle Edit is free software under the MIT license.",
        };

        var descriptionText = new TextBlock
        {
            Text = "Subtitle Edit 5 Alpha is an early version of the next major release." + Environment.NewLine +
           "Some features may be missing, incomplete or experimental." + Environment.NewLine +
           "We welcome your feedback to help improve the final version." + Environment.NewLine +
           Environment.NewLine +
           "Thank you for testing and supporting Subtitle Edit :)",
            FontSize = 12,
            Margin = new Thickness(0, 10, 0, 10)
        };

        var githubLink = new TextBlock
        {
            Text = "Github",
            Foreground = Brushes.Blue,
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        githubLink.PointerPressed += async (_, _) =>
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/niksedk/subtitleedit-avalonia"));
        };

        var panelGithub = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock { Text = "Issue tracking and source code: " },
                githubLink,
            }
        };

        var paypalLink = new TextBlock
        {
            Text = "PayPal",
            Foreground = Brushes.Blue,
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        paypalLink.PointerPressed += async (_, _) =>
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.paypal.com/donate/?hosted_button_id=4XEHVLANCQBCU"));
        };

        var githubSponsorLink = new TextBlock
        {
            Text = "Github sponsor",
            Foreground = Brushes.Blue,
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        githubSponsorLink.PointerPressed += async (_, _) =>
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/sponsors/niksedk"));
        };      


        var panelDonate = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock { Text = "Donate: " },
                paypalLink,
                new TextBlock { Text = " or " },
                githubSponsorLink
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(null);
        buttonOk.Click += (_, _) => Close();
        var panelButtons = UiUtil.MakeButtonBar(buttonOk);

        Content = new StackPanel
        {
            Spacing = 8,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                titleText,
                licenseText,
                descriptionText,
                panelGithub,
                panelDonate,
                panelButtons,
            }
        };

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}