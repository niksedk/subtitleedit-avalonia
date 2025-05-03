using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Help;

public class AboutWindow : Window
{
    public AboutWindow()
    {
        Icon = UiUtil.GetSeIcon();
        Title = "About Subtitle Edit";
        Width = 400;
        Height = 250;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var titleText = new TextBlock
        {
            Text = "Subtitle Edit",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
        };

        var versionText = new TextBlock
        {
            Text = "Version 5.0.0 Alpha 0",
        };

        var copyrightText = new TextBlock
        {
            Text = "© 2025 Nikolaj Lynge Olsson",
            FontSize = 12,
            Margin = new Thickness(0, 10, 0, 0)
        };

        var websiteLink = new TextBlock
        {
            Text = "Visit Website",
            Foreground = Brushes.Blue,
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        websiteLink.PointerPressed += (_, _) =>
        {
            // Open link (only works if app has permission)
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://www.nikse.dk/SubtitleEdit",
                    UseShellExecute = true
                });
            }
            catch
            {
                // Handle errors if needed
            }
        };

        var okButton = new Button
        {
            Content = "OK",
            Width = 80,
        };
        okButton.Click += (_, _) => Close();

        Content = new StackPanel
        {
            Spacing = 8,
            Margin = new Thickness(20),
            Children =
            {
                titleText,
                versionText,
                copyrightText,
                websiteLink,
                new StackPanel
                {
                    Margin = new Thickness(0, 20, 0, 0),
                    Children = { okButton }
                }
            }
        };
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