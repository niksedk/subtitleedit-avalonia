using System;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers;

public class MpvVideoPlayer : OpenGlControlBase
{
    private LibMpvDynamic _mpv;

    // We'll use proper mpv OpenGL integration instead of storing the context directly
    private bool _isInitialized;
    private string _videoPath;
    private IntPtr _glContext;

    public MpvVideoPlayer()
    {
        // Initialize MPV on control load
        Loaded += (s, e) => InitializeMpv();
        Unloaded += (s, e) => DisposeMpv();
    }

    public void LoadVideo(string path)
    {
        _videoPath = path;
        if (_mpv == null)
        {
            _mpv = new LibMpvDynamic();
        }
        
        _mpv.Initialize(this, path, null, null);
    }

    public void Play()
    {
        if (_isInitialized)
        {
            _mpv.Play();
        }
    }

    public void Pause()
    {
        if (_isInitialized)
        {
            _mpv.Pause();
        }
    }

    public void Stop()
    {
        if (_isInitialized)
        {
            _mpv.Stop();
        }
    }

    private void InitializeMpv()
    {
        try
        {
            // Create a new MPV instance
            _mpv = new LibMpvDynamic();
            _mpv.Initialize(this, string.Empty, null, null);

            _mpv.InitializeOpenGl(_videoPath);
            _isInitialized = true;
            
            // // Set up MPV for OpenGL rendering
            // _mpv.SetPropertyString("vo", "opengl");
            // _mpv.SetPropertyString("gpu-api", "opengl");
            //
            // // Configure mpv to use our OpenGL context
            // _mpv.SetPropertyString("opengl-backend", "auto");
            //
            // // Initialize OpenGL in MPV
            // _isInitialized = true;
            //
            // // If we already have a video path, load it
            // if (!string.IsNullOrEmpty(_videoPath))
            // {
            //     _mpv.Command("loadfile", _videoPath);
            // }

            // Set up rendering loop
            StartRenderLoop();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing MPV: {ex.Message}");
        }
    }

    private void StartRenderLoop()
    {
        // Request redraw at video framerate
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60fps
        };

        timer.Tick += (s, e) => { RequestNextFrameRendering(); };

        timer.Start();
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (!_isInitialized) return;

        try
        {
            // Clear the framebuffer
            gl.ClearColor(0, 0, 0, 1);
            gl.Clear(GlConsts.GL_COLOR_BUFFER_BIT);

            // Get the current size of the control
            var size = Bounds.Size;

            // Set the viewport
            gl.Viewport(0, 0, (int)size.Width, (int)size.Height);

            // Render the video frame (this would involve MPV's OpenGL rendering functions)
            // Note: Proper implementation requires deeper MPV OpenGL integration
            // This is a simplified example

            // In a real implementation, you would use MPV's render API to draw the video frame
            // MpvRenderContextRenderParameters renderParams = new MpvRenderContextRenderParameters
            // {
            //     FlipY = 0
            // };
            // _mpvRenderContext.Render((int)size.Width, (int)size.Height, renderParams);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OpenGL render: {ex.Message}");
        }
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        // Store the OpenGL context
        _glContext = gl.GetProcAddress("name");

        // Additional OpenGL initialization if needed
    }

    private void DisposeMpv()
    {
        if (_isInitialized)
        {
            _mpv?.Dispose();
            _isInitialized = false;
        }
    }
}
