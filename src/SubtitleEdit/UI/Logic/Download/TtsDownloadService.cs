using System.Globalization;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Linq;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic.Download;

public class TtsDownloadService : ITtsDownloadService
{
    private readonly HttpClient _httpClient;
    private const string WindowsPiperUrl = "https://github.com/rhasspy/piper/releases/download/2023.11.14-2/piper_windows_amd64.zip";
    private const string MacPiperUrl = "https://github.com/rhasspy/piper/releases/download/2023.11.14-2/piper_macos_x64.tar.gz";

    public TtsDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadPiper(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = OperatingSystem.IsWindows() ? WindowsPiperUrl : MacPiperUrl;
        await DownloadHelper.DownloadFileAsync(_httpClient, url, destinationFileName, progress, cancellationToken);
    }

    public async Task DownloadPiper(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = OperatingSystem.IsWindows() ? WindowsPiperUrl : MacPiperUrl;
        await DownloadHelper.DownloadFileAsync(_httpClient, url, stream, progress, cancellationToken);
    }

    public async Task DownloadPiperModel(string destinationFileName, PiperVoice voice, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, voice.Model, destinationFileName, progress, cancellationToken);
        await DownloadHelper.DownloadFileAsync(_httpClient, voice.Config, destinationFileName, progress, cancellationToken);
    }

    public async Task DownloadPiperVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = "https://huggingface.co/rhasspy/piper-voices/resolve/main/voices.json?download=true";
        await DownloadHelper.DownloadFileAsync(_httpClient, url, stream, progress, cancellationToken);
    }

    public async Task DownloadPiperVoice(string url, MemoryStream stream, Progress<float> progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, url, stream, progress, cancellationToken);
    }

    public async Task DownloadAllTalkVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = Se.Settings.Video.TextToSpeech.AllTalkUrl.TrimEnd('/') + "/api/voices";
        await DownloadHelper.DownloadFileAsync(_httpClient, url, stream, progress, cancellationToken);
    }

    public async Task<string> AllTalkVoiceSpeak(string inputText, AllTalkVoice voice, string language)
    {
        var multipartContent = new MultipartFormDataContent();
        var text = Utilities.UnbreakLine(inputText);
        multipartContent.Add(new StringContent(Json.EncodeJsonText(text)), "text_input");
        multipartContent.Add(new StringContent("standard"), "text_filtering");
        multipartContent.Add(new StringContent(voice.Voice), "character_voice_gen");
        multipartContent.Add(new StringContent("false"), "narrator_enabled");
        multipartContent.Add(new StringContent(voice.Voice), "narrator_voice_gen");
        multipartContent.Add(new StringContent("character"), "text_not_inside");
        multipartContent.Add(new StringContent(language), "language");
        multipartContent.Add(new StringContent("output"), "output_file_name");
        multipartContent.Add(new StringContent("false"), "output_file_timestamp");
        multipartContent.Add(new StringContent("false"), "autoplay");
        multipartContent.Add(new StringContent("1.0"), "autoplay_volume");
        var result = await _httpClient.PostAsync(Se.Settings.Video.TextToSpeech.AllTalkUrl.TrimEnd('/') + "/api/tts-generate", multipartContent);
        var bytes = await result.Content.ReadAsByteArrayAsync();
        var resultJson = Encoding.UTF8.GetString(bytes);

        if (!result.IsSuccessStatusCode)
        {
            SeLogger.Error($"All Talk TTS failed calling API as base address {_httpClient.BaseAddress} : Status code={result.StatusCode}" + Environment.NewLine + resultJson);
        }

        var jsonParser = new SeJsonParser();
        var allTalkOutput = jsonParser.GetFirstObject(resultJson, "output_file_path");
        return allTalkOutput.Replace("\\\\", "\\");
    }

    public async Task<bool> AllTalkIsInstalled()
    {
        var timeout = Task.Delay(2000); // 2 seconds timeout
        var request = _httpClient.GetAsync(Se.Settings.Video.TextToSpeech.AllTalkUrl);

        await Task.WhenAny(timeout, request); // wait for either timeout or the request

        if (timeout.IsCompleted) // if the timeout ended first, then handle it
        {
            return false;
        }

        return true;
    }

    public async Task DownloadElevenLabsVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = "https://api.elevenlabs.io/v1/voices";
        await DownloadHelper.DownloadFileAsync(_httpClient, url, stream, progress, cancellationToken);
    }

    public async Task DownloadMurfVoiceList(MemoryStream ms, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = "https://api.murf.ai/v1/speech/voices";

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        requestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        requestMessage.Headers.TryAddWithoutValidation("Accept", "audio/mpeg");
        requestMessage.Headers.TryAddWithoutValidation("api-key", Se.Settings.Video.TextToSpeech.MurfApiKey);

        var result = await _httpClient.SendAsync(requestMessage, cancellationToken);
        await result.Content.CopyToAsync(ms, cancellationToken);

        if (!result.IsSuccessStatusCode)
        {
            var error = Encoding.UTF8.GetString(ms.ToArray()).Trim();
            SeLogger.Error($"Murf TTS failed calling API address {url} : Status code={result.StatusCode} {error}");
        }
    }

    public async Task DownloadAzureVoiceList(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = "https://api.elevenlabs.io/v1/voices";
        await DownloadHelper.DownloadFileAsync(_httpClient, url, stream, progress, cancellationToken);
    }

    public async Task<bool> DownloadElevenLabsVoiceSpeak(
        string inputText,
        ElevenLabVoice voice,
        string model,
        string apiKey,
        string languageCode,
        MemoryStream stream,
        IProgress<float>? progress,
        CancellationToken cancellationToken)
    {
        var url = "https://api.elevenlabs.io/v1/text-to-speech/" + voice.VoiceId;
        var text = Utilities.UnbreakLine(inputText);

        var language = string.Empty;
        if (model == "eleven_turbo_v2_5")
        {
            language = $", \"language_code\": \"{languageCode}\"";
        }

        var stability = Se.Settings.Video.TextToSpeech.ElevenLabsStability.ToString(CultureInfo.InvariantCulture);
        var similarityBoost = Se.Settings.Video.TextToSpeech.ElevenLabsSimilarity.ToString(CultureInfo.InvariantCulture);
        var data = "{ \"text\": \"" + Json.EncodeJsonText(text) + $"\", \"model_id\": \"{model}\"{language}, \"voice_settings\": {{ \"stability\": {stability}, \"similarity_boost\": {similarityBoost} }} }}";
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        requestMessage.Content = new StringContent(data, Encoding.UTF8);
        requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        requestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        requestMessage.Headers.TryAddWithoutValidation("Accept", "audio/mpeg");
        requestMessage.Headers.TryAddWithoutValidation("xi-api-key", apiKey.Trim());

        var result = await _httpClient.SendAsync(requestMessage, cancellationToken);
        await result.Content.CopyToAsync(stream, cancellationToken);
        if (!result.IsSuccessStatusCode)
        {
            var error = Encoding.UTF8.GetString(stream.ToArray()).Trim();
            SeLogger.Error($"ElevenLabs TTS failed calling API as base address {_httpClient.BaseAddress} : Status code={result.StatusCode} {error}" + Environment.NewLine + "Data=" + data);
            return false;
        }

        return true;
    }

    public async Task<bool> DownloadAzureVoiceSpeak(
        string inputText,
        AzureVoice voice,
        string model,
        string apiKey,
        string languageCode,
        string region,
        MemoryStream stream,
        IProgress<float>? progress,
        CancellationToken cancellationToken)
    {
        var url = $"https://{region}.tts.speech.microsoft.com/cognitiveservices/v1";

        var text = Utilities.UnbreakLine(inputText);

        var data = $"<speak version='1.0' xml:lang='en-US'><voice xml:lang='en-US' xml:gender='{voice.Gender}' name='{voice.ShortName}'>{System.Net.WebUtility.HtmlEncode(text)}</voice></speak>";
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        requestMessage.Content = new StringContent(data, Encoding.UTF8);

        requestMessage.Headers.TryAddWithoutValidation("Content-Type", "ssml+xml");
        requestMessage.Headers.TryAddWithoutValidation("accept", "audio/mpeg");
        requestMessage.Headers.TryAddWithoutValidation("X-Microsoft-OutputFormat", "audio-16khz-32kbitrate-mono-mp3");
        requestMessage.Headers.TryAddWithoutValidation("User-Agent", "SubtitleEdit");
        requestMessage.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", apiKey.Trim());

        var result = await _httpClient.SendAsync(requestMessage, cancellationToken);
        await result.Content.CopyToAsync(stream, cancellationToken);
        if (!result.IsSuccessStatusCode)
        {
            var error = Encoding.UTF8.GetString(stream.ToArray()).Trim();
            SeLogger.Error($"ElevenLabs TTS failed calling API as base address {_httpClient.BaseAddress} : Status code={result.StatusCode} {error}" + Environment.NewLine + "Data=" + data);
            return false;
        }

        return true;
    }

    public async Task<bool> DownloadMurfSpeak(
        string text,
        MurfVoice voice,
        string? overrideStyle,
        string murfApiKey,
        MemoryStream ms,
        CancellationToken cancellationToken)
    {
        var url = "https://api.murf.ai/v1/speech/generate";

        if (string.IsNullOrEmpty(overrideStyle))
        {
            overrideStyle = "Conversational";
        }

        var body = new
        {
            voiceId = voice.VoiceId,
            style = voice.AvailableStyles.Contains(overrideStyle)
                ? overrideStyle
                : voice.AvailableStyles.FirstOrDefault(),
            text,
            rate = 0,
            pitch = 0,
            sampleRate = 48000,
            format = "MP3",
            channelType = "MONO",
            pronunciationDictionary = new { },
            encodeAsBase64 = false,
            modelVersion = "GEN2"
        };
        var json = System.Text.Json.JsonSerializer.Serialize(body);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

        requestMessage.Content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
        requestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        requestMessage.Headers.TryAddWithoutValidation("Accept", "application/json");
        requestMessage.Headers.TryAddWithoutValidation("api-key", murfApiKey);

        var result = await _httpClient.SendAsync(requestMessage, cancellationToken);
        await result.Content.CopyToAsync(ms, cancellationToken);
        if (!result.IsSuccessStatusCode)
        {
            var error = Encoding.UTF8.GetString(ms.ToArray()).Trim();
            SeLogger.Error($"Murf TTS failed calling API as base address {_httpClient.BaseAddress} : Status code={result.StatusCode} {error}" + Environment.NewLine + "Data=" + body);
            return false;
        }

        var parser = new SeJsonParser();
        var fileUrl = parser.GetFirstObject(Encoding.UTF8.GetString(ms.ToArray()), "audioFile");
        var audioResult = await _httpClient.GetAsync(fileUrl, cancellationToken);
        if (!audioResult.IsSuccessStatusCode)
        {
            SeLogger.Error($"Murf TTS failed calling API as base address {fileUrl} : Status code={audioResult.StatusCode}");
            return false;
        }
        await audioResult.Content.CopyToAsync(ms, cancellationToken);

        return true;
    }
}