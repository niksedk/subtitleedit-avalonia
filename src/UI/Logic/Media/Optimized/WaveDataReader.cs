using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;

namespace Nikse.SubtitleEdit.Logic.Media.Optimized;

/// <summary>
/// Shared helper functions for reading WAV sample data efficiently.
/// Eliminates code duplication across peak and spectrogram generators.
/// </summary>
public static class WaveDataReader
{
    /// <summary>
    /// Reads a sample value from the buffer based on bytes per sample.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadSampleValue(byte[] data, ref int index, int bytesPerSample)
    {
        return bytesPerSample switch
        {
            1 => ReadValue8Bit(data, ref index),
            2 => ReadValue16Bit(data, ref index),
            3 => ReadValue24Bit(data, ref index),
            4 => ReadValue32Bit(data, ref index),
            _ => throw new InvalidDataException("Cannot read bytes per sample of " + bytesPerSample)
        };
    }
    
    /// <summary>
    /// Reads a sample value from a Span based on bytes per sample.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadSampleValue(ReadOnlySpan<byte> data, ref int index, int bytesPerSample)
    {
        return bytesPerSample switch
        {
            1 => ReadValue8BitSpan(data, ref index),
            2 => ReadValue16BitSpan(data, ref index),
            3 => ReadValue24BitSpan(data, ref index),
            4 => ReadValue32BitSpan(data, ref index),
            _ => throw new InvalidDataException("Cannot read bytes per sample of " + bytesPerSample)
        };
    }
    
    /// <summary>
    /// Gets the scale factor for normalizing samples to [-1, 1] range and mixing channels.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double GetSampleAndChannelScale(int bytesPerSample, int numberOfChannels)
    {
        return (1.0 / Math.Pow(2.0, bytesPerSample * 8 - 1)) / numberOfChannels;
    }
    
    /// <summary>
    /// Rents a buffer from the shared array pool.
    /// </summary>
    public static byte[] RentBuffer(int minimumSize)
    {
        return ArrayPool<byte>.Shared.Rent(minimumSize);
    }
    
    /// <summary>
    /// Returns a buffer to the shared array pool.
    /// </summary>
    public static void ReturnBuffer(byte[] buffer)
    {
        ArrayPool<byte>.Shared.Return(buffer);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadValue8Bit(byte[] data, ref int index)
    {
        int result = sbyte.MinValue + data[index];
        index += 1;
        return result;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadValue16Bit(byte[] data, ref int index)
    {
        int result = (short)(data[index] | (data[index + 1] << 8));
        index += 2;
        return result;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadValue24Bit(byte[] data, ref int index)
    {
        int result = ((data[index] << 8) | (data[index + 1] << 16) | (data[index + 2] << 24)) >> 8;
        index += 3;
        return result;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadValue32Bit(byte[] data, ref int index)
    {
        int result = data[index] | (data[index + 1] << 8) | (data[index + 2] << 16) | (data[index + 3] << 24);
        index += 4;
        return result;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadValue8BitSpan(ReadOnlySpan<byte> data, ref int index)
    {
        int result = sbyte.MinValue + data[index];
        index += 1;
        return result;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadValue16BitSpan(ReadOnlySpan<byte> data, ref int index)
    {
        int result = (short)(data[index] | (data[index + 1] << 8));
        index += 2;
        return result;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadValue24BitSpan(ReadOnlySpan<byte> data, ref int index)
    {
        int result = ((data[index] << 8) | (data[index + 1] << 16) | (data[index + 2] << 24)) >> 8;
        index += 3;
        return result;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadValue32BitSpan(ReadOnlySpan<byte> data, ref int index)
    {
        int result = data[index] | (data[index + 1] << 8) | (data[index + 2] << 16) | (data[index + 3] << 24);
        index += 4;
        return result;
    }
}
