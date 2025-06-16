namespace HanumanInstitute.LibMpv;

public class MpvCommand<T>
{
    private readonly MpvContextBase _context;
    private readonly object?[] _cmd;

    public MpvCommand(MpvContextBase context, params object?[] cmd)
    {
        _context = context;
        _cmd = cmd;
    }

    public T Invoke(MpvCommandOptions? options = null) => (T)_context.RunCommandRet<T>(options, _cmd);

    public async Task<T> InvokeAsync(MpvCommandOptions? options = null) => (T)(await _context.CommandAsync<T>(options, _cmd))!;
}

public class MpvCommand
{
    private readonly MpvContextBase _context;
    private readonly object?[] _cmd;

    public MpvCommand(MpvContextBase context, params object?[] cmd)
    {
        _context = context;
        _cmd = cmd;
    }

    public void Invoke(MpvCommandOptions? options = null) => _context.RunCommand(options, _cmd);

    public async Task InvokeAsync(MpvCommandOptions? options = null) => await _context.CommandAsync<object>(options, _cmd);
}
