using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Runtime.InteropServices;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared;

public class FullScreenVideoWindow : Window
{
    private DispatcherTimer? _mouseMoveDetectionTimer;
    private (int X, int Y) _lastCursorPosition;
    private (int X, int Y) _lastPoiterMovedCursorPosition;

    // Windows API for getting cursor position
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    // macOS API for getting cursor position
    private const string CoreGraphicsLib = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";
    private const string ApplicationServicesLib = "/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices";


    [DllImport(CoreGraphicsLib)]
    private static extern CGPoint CGEventSourceGetCursorPosition(uint source);

    [DllImport(CoreGraphicsLib)]
    private static extern IntPtr CGEventCreate(IntPtr source);

    [DllImport(ApplicationServicesLib)]
    private static extern CGPoint CGEventGetLocation(IntPtr eventRef);

    [DllImport(CoreGraphicsLib)]
    private static extern void CFRelease(IntPtr cf);

    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double X;
        public double Y;
    }

    // Linux X11 API for getting cursor position
    [DllImport("libX11.so.6")]
    private static extern IntPtr XOpenDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XCloseDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern bool XQueryPointer(IntPtr display, IntPtr window, out IntPtr root, out IntPtr child,
        out int rootX, out int rootY, out int winX, out int winY, out uint mask);

    [DllImport("libX11.so.6")]
    private static extern IntPtr XDefaultRootWindow(IntPtr display);

    private (int X, int Y)? GetCursorPosition()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                if (GetCursorPos(out POINT cursorPos))
                {
                    return (cursorPos.X, cursorPos.Y);
                }
            }
            else if (OperatingSystem.IsMacOS())
            {
                var eventRef = CGEventCreate(IntPtr.Zero);
                if (eventRef != IntPtr.Zero)
                {
                    var point = CGEventGetLocation(eventRef);
                    CFRelease(eventRef);
                    var x = (int)Math.Round(point.X, MidpointRounding.AwayFromZero);
                    var y = (int)Math.Round(point.Y, MidpointRounding.AwayFromZero);
                    return (x, y);
                }
            }
            else if (OperatingSystem.IsLinux())
            {
                var display = XOpenDisplay(IntPtr.Zero);
                if (display != IntPtr.Zero)
                {
                    var rootWindow = XDefaultRootWindow(display);
                    if (XQueryPointer(display, rootWindow, out _, out _, out int rootX, out int rootY,
                            out _, out _, out _))
                    {
                        XCloseDisplay(display);
                        return (rootX, rootY);
                    }

                    XCloseDisplay(display);
                }
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception);
        }

        return null;
    }

    public FullScreenVideoWindow(Controls.VideoPlayer.VideoPlayerControl videoPlayer, string videoFileName, Action onClose)
    {
        WindowState = WindowState.FullScreen;
        SystemDecorations = SystemDecorations.None;

        var grid = new Grid
        {
            Background = Brushes.Transparent // Enable hit testing for pointer events
        }.Children(videoPlayer);

        Content = grid;

        // Initialize cursor position tracking
        _lastCursorPosition = (-1, -1);

        const int mouseMovementMinPixels = 20;

        // Poll for actual cursor position using platform APIs
        // This works regardless of Avalonia event handling or MPV
        _mouseMoveDetectionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _mouseMoveDetectionTimer.Tick += (s, e) =>
        {
            try
            {
                var cursorPos = GetCursorPosition();
                if (cursorPos.HasValue)
                {
                    if (Math.Abs(cursorPos.Value.X - _lastCursorPosition.X) > mouseMovementMinPixels ||
                        Math.Abs(cursorPos.Value.Y - _lastCursorPosition.Y) > mouseMovementMinPixels)
                    {
                        _lastCursorPosition = cursorPos.Value;
                        videoPlayer.NotifyUserActivity();
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        };

        // Keep these handlers as fallback if native APIs fail
        grid.PointerMoved += (_, e) =>
        {
            var pos = e.GetCurrentPoint(this);
            if (Math.Abs(pos.Position.X - _lastPoiterMovedCursorPosition.X) > mouseMovementMinPixels ||
                Math.Abs(pos.Position.Y - _lastPoiterMovedCursorPosition.Y) > mouseMovementMinPixels)
            {
                videoPlayer.NotifyUserActivity();
                _lastPoiterMovedCursorPosition = ((int)pos.Position.X, (int)pos.Position.Y);
            }

            videoPlayer.IsFullScreen = true;
        };

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            else if (e.Key == Key.F11)
            {
                Close();
            }
            else if (e.Key == Key.Space)
            {
                videoPlayer.TogglePlayPause();
            }

            // Also notify on any key press
            videoPlayer.NotifyUserActivity();
        };

        videoPlayer.FullscreenCollapseRequested += () => { Close(); };

        Closing += (_, _) =>
        {
            _mouseMoveDetectionTimer?.Stop();
            _mouseMoveDetectionTimer = null;
            videoPlayer.FullscreenCollapseRequested -= () => Close();
            onClose?.Invoke();
        };

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
        Loaded += (_, _) =>
        {
            WindowState = WindowState.Maximized;
            WindowState = WindowState.FullScreen;

            // Start polling for cursor movement
            _mouseMoveDetectionTimer?.Start();

            if (OperatingSystem.IsMacOS() && !string.IsNullOrEmpty(videoFileName))
            {
                videoPlayer.Reload();
            }
        };
    }
}