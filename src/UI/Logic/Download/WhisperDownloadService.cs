using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public class WhisperDownloadService : IWhisperDownloadService
{
    private readonly HttpClient _httpClient;
    private const string WindowsUrl = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-182/whisper-blas-bin-x64.zip";
    private const string MacArmUrl = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-182/whisper-cpp-182-mac-arm.zip";
    private const string MacX64Url = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-182/whisper-cpp-182-mac-x64.zip";
    private const string LinuxUrl = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-182/whisper-182-linux.zip";

    private const string DownloadUrlConstMe = "https://github.com/Const-me/Whisper/releases/download/1.12.0/cli.zip";

    private const string DownloadUrlPurfviewFasterWhisperXxl = "https://github.com/Purfview/whisper-standalone-win/releases/download/Faster-Whisper-XXL/Faster-Whisper-XXL_r245.4_windows.7z";
    private const string DownloadUrlPurfviewFasterWhisperXxlLinux = "https://github.com/Purfview/whisper-standalone-win/releases/download/Faster-Whisper-XXL/Faster-Whisper-XXL_r245.4_linux.7z";

    public WhisperDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadFile(string url, string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, url, destinationFileName, progress, cancellationToken);
    }

    public async Task DownloadWhisperCpp(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(), stream, progress, cancellationToken);
    }

    public async Task DownloadWhisperConstMe(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, DownloadUrlConstMe, stream, progress, cancellationToken);
    }

    public async Task DownloadWhisperPurfviewFasterWhisperXxl(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = DownloadUrlPurfviewFasterWhisperXxl;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            url = DownloadUrlPurfviewFasterWhisperXxlLinux;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new PlatformNotSupportedException("MacOS not supported.");
        }

        await DownloadHelper.DownloadFileAsync(_httpClient, url, destinationFileName, progress, cancellationToken);
    }

    private static string GetUrl()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return WindowsUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return LinuxUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.Arm64:
                    return MacArmUrl; // e.g., for M1, M2, M3, M4 chips
                case Architecture.X64:
                    return MacX64Url;
                default:
                    throw new PlatformNotSupportedException("Unsupported macOS architecture.");
            }
        }

        throw new PlatformNotSupportedException();
    }
}