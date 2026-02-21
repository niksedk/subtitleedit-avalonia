using System;
using System.IO;

namespace Benchmarks;

/// <summary>
/// Generates synthetic PCM WAV files for benchmarking.
/// Creates valid RIFF/WAV with sine wave data.
/// </summary>
public static class WavFileGenerator
{
    private const int SampleRate = 44100;
    private const int BitsPerSample = 16;
    private const int NumChannels = 2;
    private const double Frequency = 440.0; // A4 note

    /// <summary>
    /// Generates a WAV file with the specified duration.
    /// </summary>
    public static string Generate(string duration, string? directory = null)
    {
        var seconds = duration switch
        {
            "1min" => 60,
            "10min" => 600,
            "60min" => 3600,
            _ => int.Parse(duration.Replace("min", "")) * 60
        };

        return Generate(seconds, directory);
    }

    /// <summary>
    /// Generates a WAV file with the specified duration in seconds.
    /// </summary>
    public static string Generate(int durationSeconds, string? directory = null)
    {
        directory ??= Path.GetTempPath();
        var filePath = Path.Combine(directory, $"bench_{durationSeconds}s_{Guid.NewGuid():N}.wav");

        int bytesPerSample = BitsPerSample / 8;
        int blockAlign = NumChannels * bytesPerSample;
        long totalSamples = (long)SampleRate * durationSeconds;
        long dataSize = totalSamples * blockAlign;

        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize: 1024 * 1024, FileOptions.SequentialScan);
        using var writer = new BinaryWriter(stream);

        // RIFF header
        writer.Write("RIFF"u8);
        writer.Write((uint)(36 + dataSize)); // ChunkSize
        writer.Write("WAVE"u8);

        // fmt sub-chunk
        writer.Write("fmt "u8);
        writer.Write(16); // Subchunk1Size (16 for PCM)
        writer.Write((short)1); // AudioFormat (1 = PCM)
        writer.Write((short)NumChannels);
        writer.Write(SampleRate);
        writer.Write(SampleRate * blockAlign); // ByteRate
        writer.Write((short)blockAlign);
        writer.Write((short)BitsPerSample);

        // data sub-chunk
        writer.Write("data"u8);
        writer.Write((uint)dataSize);

        // Generate sine wave data in chunks
        const int bufferSamples = 65536;
        var buffer = new byte[bufferSamples * blockAlign];
        long samplesWritten = 0;
        double twoPiFreqOverRate = 2.0 * Math.PI * Frequency / SampleRate;

        while (samplesWritten < totalSamples)
        {
            int samplesToWrite = (int)Math.Min(bufferSamples, totalSamples - samplesWritten);
            int bufferOffset = 0;

            for (int i = 0; i < samplesToWrite; i++)
            {
                short sample = (short)(short.MaxValue * 0.5 * Math.Sin(twoPiFreqOverRate * (samplesWritten + i)));

                for (int ch = 0; ch < NumChannels; ch++)
                {
                    buffer[bufferOffset++] = (byte)(sample & 0xFF);
                    buffer[bufferOffset++] = (byte)((sample >> 8) & 0xFF);
                }
            }

            stream.Write(buffer, 0, samplesToWrite * blockAlign);
            samplesWritten += samplesToWrite;
        }

        return filePath;
    }
}
