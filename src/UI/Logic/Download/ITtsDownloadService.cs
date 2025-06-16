using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ITtsDownloadService
{
    Task DownloadPiper(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadPiper(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadPiperModel(string destinationFileName, PiperVoice voice, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadPiperVoice(string modelUrl, MemoryStream downloadStream, Progress<float> downloadProgress, CancellationToken token);
    Task DownloadPiperVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadAllTalkVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task<string> AllTalkVoiceSpeak(string text, AllTalkVoice voice, string language);
    Task<bool> AllTalkIsInstalled();
    Task DownloadElevenLabsVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadAzureVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadMurfVoiceList(MemoryStream stream, IProgress<float>? progress, CancellationToken cancellationToken);

    Task<bool> DownloadElevenLabsVoiceSpeak(
        string inputText,
        ElevenLabVoice voice,
        string model,
        string apiKey,
        string languageCode,
        MemoryStream stream,
        IProgress<float>? progress,
        CancellationToken cancellationToken);

    Task<bool> DownloadAzureVoiceSpeak(
        string inputText,
        AzureVoice voice,
        string model,
        string apiKey,
        string languageCode,
        string region,
        MemoryStream stream,
        IProgress<float>? progress,
        CancellationToken cancellationToken);

    Task<bool> DownloadMurfSpeak(
        string text,
        MurfVoice murfVoice,
        string? overrideStyle,
        string murfApiKey,
        MemoryStream ms,
        CancellationToken cancellationToken);

    Task<bool> DownloadGoogleVoiceList(string googleKeyFile, MemoryStream ms, CancellationToken cancellationToken);
    Task<bool> DownloadGoogleVoiceSpeak(string text, GoogleVoice googleVoice, string model, string googleKeyFile, MemoryStream ms, CancellationToken cancellationToken);
}