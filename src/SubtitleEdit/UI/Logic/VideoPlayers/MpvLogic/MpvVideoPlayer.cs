using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.OpenGL;
using Avalonia.Threading;
using Avalonia.OpenGL.Controls;
using Nikse.SubtitleEdit.Logic.VideoPlayers.MpvLogic.FFI.Raw;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.MpvLogic;

public class MpvVideoPlayer : OpenGlControlBase
{
    private bool _first = true;
    private IntPtr _mpvHanle = Mpv.mpv_create();
    private IntPtr _mpvRenderCtx;
    private static CommonCallBack? _wakupCallBack;// a simple way to make the delegete live long enough when the delegate is passed to C library
    private static CommonCallBack? _updateCallBack;// live long enough

    public MpvVideoPlayer()
    {
        _wakupCallBack = WakeUp;
        _updateCallBack = updateGl;

        Mpv.mpv_set_option_string(_mpvHanle, "vo", "libmpv"); // Prevent mpv from creating its own window

        //var retcode = Mpv.mpv_set_option_string(_mpvHanle, "terminal", "yes");
        //retcode = Mpv.mpv_set_option_string(_mpvHanle, "msg-level", "all=v");

        var code = Mpv.mpv_initialize(_mpvHanle);
        if (code != 0)
        {
            throw new Exception("init");
        }

        Mpv.mpv_set_wakeup_callback(_mpvHanle, _wakupCallBack, IntPtr.Zero);
    }

    public void CommandNode(List<string> args)
    {
        var node = new MpvNode
        {
            format = MpvFormat.MPV_FORMAT_NODE_ARRAY
        };

        var list = new MpvNodeList
        {
            num = args.Count
        };

        var nodeList = new MpvNode[list.num];
        for (int i = 0; i < nodeList.Length; i++)
        {
            nodeList[i].format = MpvFormat.MPV_FORMAT_STRING;
            nodeList[i].data.str = Marshal.StringToHGlobalAnsi(args[i]);//todo: free mem
        }
        list.nodeValues = Marshal.UnsafeAddrOfPinnedArrayElement(nodeList, 0);

        var listPtr = Marshal.AllocHGlobal(Marshal.SizeOf(list));
        Marshal.StructureToPtr(list, listPtr, true);
        node.data.nodeList = listPtr;

        var res = new MpvNode();
        unsafe
        {
            nint nodePtr = new nint(&node);
            int code = Mpv.mpv_command_node(_mpvHanle, nodePtr, out res);
            Console.WriteLine();
        }
    }

    private void WakeUp(nint data)
    {
        Task.Run(() => //avoid block here 
        {
            while (true)
            {
                Console.WriteLine("start");
                var ptr = Mpv.mpv_wait_event(_mpvHanle, 0);
                Console.WriteLine("end");
                var evt = Marshal.PtrToStructure<MpvEvent>(ptr);
                if (evt.event_id == MpvEventId.MPV_EVENT_NONE)
                {
                    break;
                }
            }
        });
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        Console.WriteLine($" this control {Bounds.Width} {Bounds.Height}");
        MpvOpenglFbo fbo = new MpvOpenglFbo()
        {
            fbo = fb,
            w = (int)Bounds.Width,
            h = (int)Bounds.Height,
            internal_format = 0
        };
        unsafe
        {
            var flip = 1;
            var fboPtr = Marshal.AllocHGlobal(Marshal.SizeOf(fbo));
            Marshal.StructureToPtr(fbo, fboPtr, true);

            var _params = new MpvRenderParam[]
            {
                new MpvRenderParam()
                {
                    type = MpvRenderParamType.MPV_RENDER_PARAM_OPENGL_FBO,
                    data = fboPtr,
                },
                new MpvRenderParam()
                {
                    type = MpvRenderParamType.MPV_RENDER_PARAM_FLIP_Y,
                    data = new nint(&flip)
                },
                new MpvRenderParam()
                {
                    type = MpvRenderParamType.MPV_RENDER_PARAM_INVALID,
                    data = nint.Zero
                }
            };
            var renderCode = Mpv.mpv_render_context_render(_mpvRenderCtx, Marshal.UnsafeAddrOfPinnedArrayElement(_params, 0));
            Console.WriteLine($"render code: {renderCode}");
            Marshal.FreeHGlobal(fboPtr);
        }
    }

    private async void updateGl(nint data)
    {
        // Console.WriteLine("request update");
        await Dispatcher.UIThread.InvokeAsync(RequestNextFrameRendering);
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        if (!_first)
        {
            Console.WriteLine("init twice");
            return;
        }
        else
        {
            _first = false;
        }

        MpvOpenglInitParams para = new()
        {
            get_pro_address = (ctx, name) =>
            {
                Console.WriteLine($"get proc name ==> {name}");
                return gl.GetProcAddress(name);
            }
        };

        var ptrs = Marshal.StringToHGlobalAnsi("opengl");
        var paramsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(para));
        Marshal.StructureToPtr(para, paramsPtr, true);
        unsafe
        {
            var _params = new MpvRenderParam[]
            {
                new MpvRenderParam()
                {
                    type = MpvRenderParamType.MPV_RENDER_PARAM_API_TYPE,
                    data = ptrs
                },
                new MpvRenderParam()
                {
                    type = MpvRenderParamType.MPV_RENDER_PARAM_OPENGL_INIT_PARAMS,
                    data = paramsPtr,
                },
                new MpvRenderParam()
                {
                    type = MpvRenderParamType.MPV_RENDER_PARAM_INVALID,
                    data = nint.Zero
                }
            };
            var code = Mpv.mpv_render_context_create(out _mpvRenderCtx, _mpvHanle, Marshal.UnsafeAddrOfPinnedArrayElement(_params, 0));
        }

        //DynamicMpv.RendeContextSetUpdateCallback(_mpvRenderCtx, _updateCallBack, IntPtr.Zero);
        Mpv.mpv_render_context_set_update_callback(_mpvRenderCtx, _updateCallBack, nint.Zero);
        Marshal.FreeHGlobal(paramsPtr);
        Marshal.FreeHGlobal(ptrs);
    }

    public void OpenMedia(string mediaFileName)
    {
        CommandNode(args: new List<string>()
        {
            "loadfile",
            mediaFileName
        });
    }

    public void Pause()
    {
        CommandNode(args: new List<string>()
        {
            "set_property",
            "pause",
            "true"
        });
    }

    public void Play()
    {
        CommandNode(args: new List<string>()
        {
            "set_property",
            "pause",
            "false"
        });
    }

    public void Stop()
    {
        CommandNode(args: new List<string>()
        {
            "stop"
        });
    }

    public void Seek(double time)
    {
        CommandNode(args: new List<string>()
        {
            "seek",
            time.ToString(),
            "absolute"
        });
    }

    public void SetVolume(double volume)
    {
        CommandNode(args: new List<string>()
        {
            "set_property",
            "volume",
            volume.ToString()
        });
    }

    public void SetSpeed(double speed)
    {
        CommandNode(args: new List<string>()
        {
            "set_property",
            "speed",
            speed.ToString()
        });
    }

    public void SetPosition(double position)
    {
        CommandNode(args: new List<string>()
        {
            "set_property",
            "time-pos",
            position.ToString()
        });
    }
}