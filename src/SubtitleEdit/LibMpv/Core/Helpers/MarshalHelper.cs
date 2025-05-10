namespace HanumanInstitute.LibMpv.Core;

public class MarshalHelper : IDisposable
{
    bool _disposed;
    public static TStruct PtrToStructure<TStruct>(IntPtr ptr) where TStruct : struct
    {
        if (ptr == IntPtr.Zero)
            throw new ArgumentException("Invalid pointer.");

        return (TStruct)Marshal.PtrToStructure(ptr, typeof(TStruct));
    }
    
    public static string PtrToStringUtf8OrEmpty(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return String.Empty;
        return Utf8Marshaler.FromNative(ptr, Encoding.UTF8)!;
    }

    public static string? PtrToStringUtf8OrNull(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return null;
        return Utf8Marshaler.FromNative(ptr, Encoding.UTF8);
    }

    struct AllocBlock
    {
        public readonly bool IsHGlobal;
        public readonly IntPtr IntPtr;
        public AllocBlock(IntPtr intPtr, bool isHGlobal=true)
        {
            this.IntPtr = intPtr;
            this.IsHGlobal = isHGlobal;
        }
    }

    private readonly List<AllocBlock> _toBeFree = new List<AllocBlock>();

    public nint CStringFromManagedUtf8String(string @string)
    {
        @string += '\0';

        var stringBytes = Encoding.UTF8.GetBytes(@string);
        var stringBytesCount = stringBytes.Length;

        var stringPtr = Marshal.AllocCoTaskMem(stringBytesCount);
        Marshal.Copy(stringBytes, 0, stringPtr, stringBytesCount);

        _toBeFree.Add(new AllocBlock(stringPtr, false));
        return stringPtr;
    }

    public nint CStringArrayForManagedUtf8StringArray(string[] inArray)
    {
        var numberOfStrings = inArray.Length + 1;

        nint[] outArray = new IntPtr[numberOfStrings];

        var rootPointer = Marshal.AllocCoTaskMem(IntPtr.Size * numberOfStrings);
        _toBeFree.Add(new AllocBlock(rootPointer, false));

        for (var index = 0; index < inArray.Length; index++)
        {
            var currentString = inArray[index];
            var currentStringPtr = CStringFromManagedUtf8String(currentString);

            outArray[index] = currentStringPtr;
        }

        Marshal.Copy(outArray, 0, rootPointer, numberOfStrings);

        return rootPointer;
    }

    public nint StringToHGlobalAnsi(string value)
    {
        var ptr = Marshal.StringToHGlobalAnsi(value);
        _toBeFree.Add(new AllocBlock(ptr));
        return ptr;
    }

    public nint AllocHGlobal<T>()
    {
        return this.AllocHGlobal(Marshal.SizeOf<T>());
    }

    public nint AllocHGlobal<T>(T instance) where T : struct
    {
        var ptr = this.AllocHGlobal(Marshal.SizeOf<T>());
        Marshal.StructureToPtr(instance, ptr, false);
        return ptr;
    }

    public nint AllocHGlobalValue(int value)
    {
        var ptr = this.AllocHGlobal(sizeof(int));
        Marshal.WriteInt32(ptr, value);
        return ptr;
    }
    
    public nint AllocHGlobal(int cb)
    {
        var ptr = Marshal.AllocHGlobal(cb);
        _toBeFree.Add(new AllocBlock(ptr));
        return ptr;
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        foreach(var item in _toBeFree)
        {
            if (item.IsHGlobal)
                Marshal.FreeHGlobal(item.IntPtr);
            else
                Marshal.FreeCoTaskMem(item.IntPtr);
        }
        _disposed = true;
    }
}
