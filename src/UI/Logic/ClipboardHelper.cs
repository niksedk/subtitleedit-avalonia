using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
                return await ClipboardExtensions.TryGetTextAsync(clipboard);
            }
            catch
            {
                await Task.Delay(50);
                try
                {
                    return await ClipboardExtensions.TryGetTextAsync(clipboard);
                }
                catch
                {
                    // Ignore exceptions when clipboard is not available (e.g., remote desktop)
                }
            }
        }

        return string.Empty;
    }

    public static async Task CopyImageToClipboard(Bitmap bitmap)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await CopyImageToClipboardWindows(bitmap);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            await CopyImageToClipboardLinux(bitmap);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            await CopyImageToClipboardMacOS(bitmap);
        }
        else
        {
            throw new PlatformNotSupportedException("Clipboard image copy not supported on this platform");
        }
    }

    private static async Task CopyImageToClipboardWindows(Bitmap bitmap)
    {
        try
        {
            // Save to temporary file
            var tempPath = Path.Combine(Path.GetTempPath(), $"clipboard_{Guid.NewGuid()}.png");
            bitmap.Save(tempPath);

            // Use PowerShell to copy image to clipboard
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"Set-Clipboard -Path '{tempPath}'\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(psi);
            if (process != null)
            {
                await process.WaitForExitAsync();

                // Clean up temp file after a delay
                await Task.Delay(500);
                try { File.Delete(tempPath); } catch { }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to copy image to clipboard on Windows: {ex.Message}");
            throw;
        }
    }

    private static async Task CopyImageToClipboardLinux(Bitmap bitmap)
    {
        try
        {
            // Save to temporary file
            var tempPath = Path.Combine(Path.GetTempPath(), $"clipboard_{Guid.NewGuid()}.png");
            bitmap.Save(tempPath);

            // Try xclip first
            var psi = new ProcessStartInfo
            {
                FileName = "xclip",
                Arguments = $"-selection clipboard -t image/png -i '{tempPath}'",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                using var process = Process.Start(psi);
                if (process != null)
                {
                    await process.WaitForExitAsync();

                    // Clean up temp file after a delay
                    await Task.Delay(500);
                    try { File.Delete(tempPath); } catch { }
                    return;
                }
            }
            catch
            {
                // If xclip fails, try wl-copy (Wayland)
                psi.FileName = "wl-copy";
                psi.Arguments = $"--type image/png < '{tempPath}'";

                using var process = Process.Start(psi);
                if (process != null)
                {
                    await process.WaitForExitAsync();

                    await Task.Delay(500);
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to copy image to clipboard on Linux: {ex.Message}");
            throw;
        }
    }

    private static async Task CopyImageToClipboardMacOS(Bitmap bitmap)
    {
        try
        {
            // Save to temporary file
            var tempPath = Path.Combine(Path.GetTempPath(), $"clipboard_{Guid.NewGuid()}.png");
            bitmap.Save(tempPath);

            // Use osascript to copy image to clipboard
            var psi = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e 'set the clipboard to (read (POSIX file \"{tempPath}\") as «class PNGf»)'",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(psi);
            if (process != null)
            {
                await process.WaitForExitAsync();

                // Clean up temp file after a delay
                await Task.Delay(500);
                try { File.Delete(tempPath); } catch { }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to copy image to clipboard on macOS: {ex.Message}");
            throw;
        }
    }
}
