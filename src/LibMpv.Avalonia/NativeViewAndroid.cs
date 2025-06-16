#if ANDROID
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Avalonia;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Platform;

namespace HanumanInstitute.LibMpv.Avalonia;

public class NativeView : NativeControlHost, IVideoView
{
    public class MpvSurfaceView : SurfaceView, ISurfaceHolderCallback
    {
        private MpvContext? _mpvContext;
        public IntPtr NativeHandle { get; private set; } = IntPtr.Zero;

        public MpvSurfaceView(Context context) : base(context)
        {
            this.SetZOrderOnTop(true);
            Holder.AddCallback(this);
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            if (_mpvContext != null)
            {
                _mpvContext.SetPropertyString("android-surface-size", $"{width}x{height}");
            }
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            NativeHandle = holder.Surface!.Handle;
            if (_mpvContext != null)
            {
                Attach(_mpvContext);
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            Detach();
            NativeHandle = IntPtr.Zero;
        }

        public void Attach(MpvContext mpvContext)
        {
            _mpvContext = mpvContext;
            if (NativeHandle != IntPtr.Zero)
            {
                _mpvContext.StartNativeRendering(NativeHandle.ToInt64());
            }
        }

        public void Detach()
        {
            _mpvContext?.StopRendering();
            _mpvContext = null;
        }
    }

    private MpvSurfaceView? _mpvSurfaceView;

    // MpvContext property
    public static readonly DirectProperty<NativeView, MpvContext?> MpvContextProperty = AvaloniaProperty.RegisterDirect<NativeView, MpvContext?>(
            nameof(MpvContext), o => o.MpvContext, defaultBindingMode: BindingMode.OneWayToSource);
    public MpvContext MpvContext { get; } = new();

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        MpvContext.SetOptionString("vo", "gpu");
        MpvContext.SetOptionString("gpu-debug", "yes");
        MpvContext.SetOptionString("gpu-context", "android");
        MpvContext.SetOptionString("opengl-es", "yes");
        
        var parentContext = (parent as AndroidViewControlHandle)?.View.Context ?? Android.App.Application.Context;
        _mpvSurfaceView = new MpvSurfaceView(parentContext);
        _mpvSurfaceView.Attach(MpvContext);
        return new AndroidViewControlHandle(_mpvSurfaceView);
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        _mpvSurfaceView?.Detach();
        _mpvSurfaceView = null;
        base.DestroyNativeControlCore(control);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _mpvSurfaceView?.Dispose();
            MpvContext.Dispose();
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
#endif
