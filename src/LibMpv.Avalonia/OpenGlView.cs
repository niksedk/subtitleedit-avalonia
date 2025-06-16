using Avalonia;
using Avalonia.Data;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;

namespace HanumanInstitute.LibMpv.Avalonia;

public class OpenGlView : OpenGlControlBase, IVideoView
{
    delegate IntPtr GetProcAddress(string proc);

    private GetProcAddress? _getProcAddress;

    public static readonly DirectProperty<OpenGlView, MpvContext> MpvContextProperty =
        AvaloniaProperty.RegisterDirect<OpenGlView, MpvContext>(nameof(MpvContext),
            o => o.MpvContext, defaultBindingMode: BindingMode.OneWayToSource);
    public MpvContext MpvContext { get; } = new();

    protected override void OnOpenGlRender(GlInterface gl, int fbo)
    {
        if (MpvContext != null && MpvContext.IsCustomRendering())
        {
            var size = GetPixelSize();
            MpvContext.OpenGlRender(size.Width, size.Height, fbo, 1);
        }
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        if (_getProcAddress != null)
        {
            return;
        }

        _getProcAddress = gl.GetProcAddress;
        MpvContext?.StopRendering();
        MpvContext?.StartOpenGlRendering((name) => _getProcAddress(name), this.UpdateVideoView);
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        MpvContext?.StopRendering();
        _getProcAddress = null;
    }

    private PixelSize GetPixelSize()
    {
        var scaling = VisualRoot!.RenderScaling;
        return new PixelSize(Math.Max(1, (int)(Bounds.Width * scaling)), Math.Max(1, (int)(Bounds.Height * scaling)));
    }

    private void UpdateVideoView()
    {
        Dispatcher.UIThread.InvokeAsync(this.RequestNextFrameRendering, DispatcherPriority.Background);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            MpvContext?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
