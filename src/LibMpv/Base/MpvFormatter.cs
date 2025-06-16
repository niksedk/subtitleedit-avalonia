using HanumanInstitute.LibMpv.Core;

namespace HanumanInstitute.LibMpv;

public static unsafe class MpvFormatter
{
    
    /// <summary>Read the value of the given property.</summary>
    /// <param name="name">The property name.</param>
    /// <typeparam name="T">The data type of the property to read.</typeparam>
    public static T GetProperty<T>(MpvHandle* ctx, string name)
    {
        var format = GetMpvFormat<T>();
        switch (format)
        {
            case MpvFormat.Int64:
                var vLong = 0L;
                MpvApi.GetProperty(ctx, name, MpvFormat.Int64, &vLong).CheckCode();
                return (T)(object)vLong;
            case MpvFormat.Double:
                var vDouble = 0.0;
                MpvApi.GetProperty(ctx, name, MpvFormat.Double, &vDouble).CheckCode();
                return (T)(object)vDouble;
            case MpvFormat.Flag:
                var vBool = 0;
                MpvApi.GetProperty(ctx, name, MpvFormat.Flag, &vBool).CheckCode();
                return (T)(object)(vBool == 1);
            case MpvFormat.String:
            case MpvFormat.OsdString:
                var value = MpvApi.GetPropertyString(ctx, name);
                return (T)(object)(value != null ? Utf8Marshaler.FromNative(value, Encoding.UTF8) : null)!;
        }
        return default!;
    }

    public static MpvFormat GetMpvFormat<T>()
    {
        var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        return type switch
        {
            _ when type == typeof(long) || type == typeof(int) => MpvFormat.Int64,
            _ when type == typeof(double) || type == typeof(float) => MpvFormat.Double,
            _ when type == typeof(bool) => MpvFormat.Flag,
            _ when type == typeof(string) => MpvFormat.String,
            _ => MpvFormat.None
        };
    }
    
    public static void SetProperty<T>(MpvHandle* ctx, string name, T value)
    {
        var format = GetMpvFormat<T>();
        switch (format)
        {
            case MpvFormat.Int64:
                var vLong = Convert.ToInt64(value);
                MpvApi.SetProperty(ctx, name, MpvFormat.Int64, &vLong).CheckCode();
                break;
            case MpvFormat.Double:
                var vDouble = Convert.ToDouble(value);
                MpvApi.SetProperty(ctx, name, MpvFormat.Double, &vDouble).CheckCode();
                break;
            case MpvFormat.Flag:
                var vBool = value as bool? == true ? 1 : 0;
                MpvApi.SetProperty(ctx, name, MpvFormat.Flag, &vBool).CheckCode();
                break;
            case MpvFormat.String:
            case MpvFormat.OsdString:
                MpvApi.SetPropertyString(ctx, name, value.ToStringInvariant()).CheckCode();
                break;
        }
    }

    public static void SetPropertyAsync<T>(MpvHandle* ctx, string name, T value, ulong requestId)
    {
        var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        switch (type)
        {
            case not null when type == typeof(long) || type == typeof(int):
                var vLong = Convert.ToInt64(value);
                MpvApi.SetPropertyAsync(ctx, requestId, name, MpvFormat.Int64, &vLong).CheckCode();
                break;
            case not null when type == typeof(double) || type == typeof(float):
                var vDouble = Convert.ToDouble(value);
                MpvApi.SetPropertyAsync(ctx, requestId, name, MpvFormat.Double, &vDouble).CheckCode();
                break;
            case not null when type == typeof(bool):
                var vBool = value as bool? == true ? 1 : 0;
                MpvApi.SetPropertyAsync(ctx, requestId, name, MpvFormat.Flag, &vBool).CheckCode();
                break;
            case not null when type == typeof(string):
                var vString = Utf8Marshaler.FromManaged(Encoding.UTF8, value.ToStringInvariant());
                MpvApi.SetPropertyAsync(ctx, requestId, name, MpvFormat.String, &vString).CheckCode();
                break;
        }
    }
    
    public static T? Parse<T>(this MpvNode node)
    {
        var result = node.Format switch
        {
            MpvFormat.Double => (object)node.U.Double,
            MpvFormat.Flag => node.U.Flag,
            MpvFormat.Int64 => node.U.Int64,
            MpvFormat.String => Utf8Marshaler.FromNative(node.U.String, Encoding.UTF8),
            MpvFormat.OsdString => Utf8Marshaler.FromNative(node.U.String, Encoding.UTF8),
            _ => default
        };
        return (T)result!;
    }

    public static T? ParseData<T>(IntPtr data)
    {
        object? value = null;
        var format = GetMpvFormat<T>();
        if (data == IntPtr.Zero)
        {
            return default;
        }
        else if (format == MpvFormat.String)
        {
            value = Utf8Marshaler.FromNative(data);
            // value = MarshalHelper.PtrToStringUtf8OrNull((nint) property.Data);
        }
        else if (format == MpvFormat.Int64)
        {
            value = Marshal.ReadInt64(data);
        }
        else if (format == MpvFormat.Flag)
        {
            var flag = Marshal.ReadInt32(data);
            value = flag == 1;
        }
        else if (format == MpvFormat.Double)
        {
            var doubleBytes = new byte[sizeof(double)];
            Marshal.Copy(data, doubleBytes, 0, sizeof(double));
            value = BitConverter.ToDouble(doubleBytes, 0);
        }
        return (T?)value;
    }
}
