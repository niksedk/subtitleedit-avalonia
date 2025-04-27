using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Nikse.SubtitleEdit.Features.Common;

public enum MessageBoxResult
{
    None,
    OK,
    Cancel,
    Yes,
    No
}

public enum MessageBoxButtons
{
    OK,
    OKCancel,
    YesNo,
    YesNoCancel
}

public enum MessageBoxIcon
{
    None,
    Information,
    Warning,
    Error,
    Question
}

public class MessageBox : Window
{
    private MessageBoxResult _result = MessageBoxResult.None;

    private MessageBox(string title, string message, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        Width = 400;
        Height = 200;
        Title = title;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        CanResize = false;

        // Icon (optional)
        var iconImage = new Image
        {
            Width = 48,
            Height = 48,
            Margin = new Thickness(10)
        };

        if (icon != MessageBoxIcon.None)
        {
            var iconPath = $"Assets/Themes/Dark/{icon}.png"; // <- change YourAppName
            try
            {
                iconImage.Source = new Bitmap(iconPath);
            }
            catch
            {
                // If icon not found, silently ignore
            }
        }

        var textBlock = new TextBlock
        {
            Text = message,
            Margin = new Thickness(10),
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };

        var contentPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
            Spacing = 10,
            Children = { iconImage, textBlock }
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(10)
        };

        void AddButton(string text, MessageBoxResult result)
        {
            var btn = new Button { Content = text, Width = 80, Margin = new Thickness(5) };
            btn.Click += (_, _) => { _result = result; Close(_result); };
            buttonPanel.Children.Add(btn);
        }

        // Add buttons based on MessageBoxButtons
        switch (buttons)
        {
            case MessageBoxButtons.OK:
                AddButton("OK", MessageBoxResult.OK);
                break;
            case MessageBoxButtons.OKCancel:
                AddButton("OK", MessageBoxResult.OK);
                AddButton("Cancel", MessageBoxResult.Cancel);
                break;
            case MessageBoxButtons.YesNo:
                AddButton("Yes", MessageBoxResult.Yes);
                AddButton("No", MessageBoxResult.No);
                break;
            case MessageBoxButtons.YesNoCancel:
                AddButton("Yes", MessageBoxResult.Yes);
                AddButton("No", MessageBoxResult.No);
                AddButton("Cancel", MessageBoxResult.Cancel);
                break;
        }

        Content = new DockPanel
        {
            LastChildFill = true,
            Children =
            {
                new Border
                {
                    Child = contentPanel,
                    Margin = new Thickness(10),
                    [DockPanel.DockProperty] = Dock.Top
                },
                new Border
                {
                    Child = buttonPanel,
                    Margin = new Thickness(10),
                    [DockPanel.DockProperty] = Dock.Bottom
                }
            }
        };

        // Handle Escape key
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                _result = MessageBoxResult.Cancel;
                Close(_result);
                e.Handled = true;
            }
        };
    }

    public static async Task<MessageBoxResult> Show(Window owner, string title, string message,
        MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
    {
        var msgBox = new MessageBox(title, message, buttons, icon);
        return await msgBox.ShowDialog<MessageBoxResult>(owner);
    }
}
