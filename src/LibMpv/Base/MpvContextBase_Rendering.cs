using HanumanInstitute.LibMpv.Core;
using MpvApi = HanumanInstitute.LibMpv.Core.MpvApi;

namespace HanumanInstitute.LibMpv;

public delegate nint GetProcAddress(string name);
public delegate void UpdateCallback();

public unsafe partial class MpvContextBase
{
    private MpvRenderContext* _renderContext;
    private MpvOpenglInitParamsGetProcAddress _getProcAddress;
    private MpvRenderContextSetUpdateCallbackCallback _updateCallback;

    public void StartOpenGlRendering(GetProcAddress getProcAddress, UpdateCallback updateCallback)
    {
        if (_disposed) { return; }
        StopRendering();

        _getProcAddress = (_, name) => (void*) getProcAddress(name);
        _updateCallback = _ => updateCallback();

        using var marshalHelper = new MarshalHelper();

        var parameters = new MpvRenderParam[]
        {
            new()
            {
                Type = MpvRenderParamType.ApiType,
                Data = (void*) marshalHelper.StringToHGlobalAnsi(MpvApi.MpvRenderApiTypeOpenGl)
            },
            new()
            {
                Type = MpvRenderParamType.OpenGlInitParams,
                Data = (void*) marshalHelper.AllocHGlobal(new MpvOpenglInitParams
                {
                    GetProcAddress = _getProcAddress,
                    GetProcAddressCtx = null
                })
            },
            new()
            {
                Type = MpvRenderParamType.AdvancedControl,
                Data = (void*) marshalHelper.AllocHGlobalValue(0)
            },
            new()
            {
                Type = MpvRenderParamType.Invalid,
                Data = null
            }
        };

        fixed (MpvRenderParam* parametersPtr = parameters)
        {
            RenderContextCreate(parametersPtr);
        }
        RenderContextSetUpdateCallback(_updateCallback);
    }

    public void OpenGlRender(int width, int height, int fb = 0, int flipY = 0)
    {
        if (_disposed) return;
        if (_renderContext == null) return;

        using var marshalHelper = new MarshalHelper();

        var fbo = new MpvOpenglFbo()
        {
            W = width,
            H = height,
            Fbo = fb
        };

        var handle = GCHandle.Alloc(fbo, GCHandleType.Pinned);
        var parameters = new MpvRenderParam[]
        {
            new()
            {
                Type = MpvRenderParamType.OpenGlFbo,
                Data = &fbo
            },
            new()
            {
                Type = MpvRenderParamType.FlipY,
                Data = (void*) marshalHelper.AllocHGlobalValue(flipY)
            },
            new()
            {
                Type = MpvRenderParamType.Invalid
            },
        };

        fixed (MpvRenderParam* parametersPtr = parameters)
        {
            RenderContextRender(parametersPtr);
        }
        handle.Free();
    }

    public void StartSoftwareRendering(UpdateCallback updateCallback)
    {
        if (_disposed) return;
        StopRendering();

        _updateCallback = _ => updateCallback();

        using var marshalHelper = new MarshalHelper();

        var parameters = new MpvRenderParam[]
        {
            new()
            {
                Type = MpvRenderParamType.ApiType,
                Data = (void*) marshalHelper.StringToHGlobalAnsi(MpvApi.MpvRenderApiTypeSw)
            },
            new()
            {
                Type = MpvRenderParamType.AdvancedControl,
                Data = (void*) marshalHelper.AllocHGlobalValue(0)
            },
            new()
            {
                Type = MpvRenderParamType.Invalid,
                Data = null
            }
        };

        fixed (MpvRenderParam* parametersPtr = parameters)
        {
            RenderContextCreate(parametersPtr);
        }
        RenderContextSetUpdateCallback(_updateCallback);
    }

    public void SoftwareRender(int width, int height, nint surfaceAddress, string format)
    {
        if (_disposed) return;
        if (_renderContext == null) return;

        using var marshalHelper = new MarshalHelper();

        var size = new[] { width, height };
        var stride = new[] { (uint) width * 4 };

        fixed (int* sizePtr = size)
        {
            fixed (uint* stridePtr = stride)
            {
                var parameters = new MpvRenderParam[]
                {
                    new()
                    {
                        Type = MpvRenderParamType.SwSize,
                        Data = sizePtr
                    },
                    new()
                    {
                        Type = MpvRenderParamType.SwFormat,
                        Data = (void*) marshalHelper.CStringFromManagedUtf8String(format)
                    },
                    new()
                    {
                        Type = MpvRenderParamType.SwStride,
                        Data = stridePtr
                    },
                    new()
                    {
                        Type = MpvRenderParamType.SwPointer,
                        Data = (void*) surfaceAddress
                    },
                    new()
                    {
                        Type = MpvRenderParamType.Invalid,
                        Data = null
                    }
                };
                fixed (MpvRenderParam* parametersPtr = parameters)
                {
                    RenderContextRender(parametersPtr);
                }
            }
        }
    }

    public void StartNativeRendering(long hw)
    {
        if (_disposed) return;
        SetPropertyLong("wid", hw);
    }

    public void StopRendering()
    {
        RunCommand(null, "stop");
        if (_renderContext != null)
        {
            RenderContextFree();
        }
        else
        {
            SetPropertyLong("wid", 0);
        }
    }

    public bool IsCustomRendering() => _renderContext != null;
}
