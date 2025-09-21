using Avalonia.Controls;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic;

public static class ClipboardHelper
{
    public static async Task SetTextAsync(Window window, string text)
    {
        var clipboard = TopLevel.GetTopLevel(window)?.Clipboard;
        if (clipboard != null)
        {
            try
            {
                await clipboard.SetTextAsync(text);
            }
            catch
            {
                await Task.Delay(50);

                try
                {
                    await clipboard.SetTextAsync(text);
                }
                catch
                {
                    // Ignore exceptions when clipboard is not available (e.g., remote desktop)
                }
            }
        }
    }

    // GetTextAsync 
    public static async Task<string?> GetTextAsync(Window window)
    {
        var clipboard = TopLevel.GetTopLevel(window)?.Clipboard;
        if (clipboard != null)
        {
            try
            {
                return await clipboard.GetTextAsync();
            }
            catch
            {
                await Task.Delay(50);
                try
                {
                    return await clipboard.GetTextAsync();
                }
                catch
                {
                    // Ignore exceptions when clipboard is not available (e.g., remote desktop)
                }
            }
        }

        return string.Empty;
    }
}
