namespace HanumanInstitute.LibMpv.Core;

public sealed class Utf8Marshaler : ICustomMarshaler
{
    private static readonly Utf8Marshaler Instance = new();

    public object? MarshalNativeToManaged(IntPtr pNativeData) => FromNative(pNativeData, Encoding.UTF8);

    public IntPtr MarshalManagedToNative(object? managedObj)
    {
        if (managedObj == null)
            return IntPtr.Zero;

        if (managedObj is not string str)
            throw new MarshalDirectiveException($"{GetType().Name} must be used on a string.");

        return FromManaged(Encoding.UTF8, str);
    }

    public void CleanUpNativeData(IntPtr pNativeData)
    {
        //Free anything allocated by MarshalManagedToNative
        //This is called after the native function call completes

        if (pNativeData != IntPtr.Zero)
            Marshal.FreeHGlobal(pNativeData);
    }

    public void CleanUpManagedData(object managedObj)
    {
        //Free anything allocated by MarshalNativeToManaged
        //This is called after the native function call completes
    }

    public int GetNativeDataSize() => -1; // Not a value type

    public static ICustomMarshaler GetInstance(string cookie) => Instance;

    public static unsafe string? FromNative(IntPtr pNativeData, Encoding? encoding = null) => FromNative((byte*)pNativeData, encoding);

    public static unsafe string? FromNative(byte* pNativeData, Encoding? encoding = null)
    {
        if (pNativeData == null)
            return null;
        
        encoding ??= Encoding.UTF8;
        var start = pNativeData;
        var walk = start;

        // Find the end of the string
        while (*walk != 0) walk++;

        if (walk == start)
            return string.Empty;

        return new string((sbyte*)pNativeData, 0, (int)(walk - start), encoding);
    }

    public static unsafe IntPtr FromManaged(Encoding? encoding, string? value)
    {
        if (encoding == null || value == null)
            return IntPtr.Zero;

        var length = encoding.GetByteCount(value);
        var buffer = (byte*)Marshal.AllocHGlobal(length + 1).ToPointer();

        if (length > 0)
        {
            fixed (char* pValue = value)
            {
                encoding.GetBytes(pValue, value.Length, buffer, length);
            }
        }

        buffer[length] = 0;

        return new IntPtr(buffer);
    }
}
