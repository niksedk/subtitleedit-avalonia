using Google.Cloud.TextToSpeech.V1;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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
            SeLogger.Error($"Azure TTS failed calling API as base address {_httpClient.BaseAddress} : Status code={result.StatusCode} {error}" + Environment.NewLine + "Data=" + data);
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

    /// <summary>
    /// Downloads a list of available Google Text-to-Speech voices in raw JSON format.
    /// Authenticates using the service account JSON key file.
    /// </summary>
    /// <param name="googleJsonKeyFileName">The full path to the Google service account JSON key file.</param>
    /// <param name="ms">The MemoryStream to write the JSON content to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the voice list is successfully retrieved and written to the stream, false otherwise.</returns>
    public async Task<bool> DownloadGoogleVoiceList(string googleJsonKeyFileName, MemoryStream ms, CancellationToken cancellationToken)
    {
        // 1. Set the GOOGLE_APPLICATION_CREDENTIALS environment variable for authentication.
        if (string.IsNullOrEmpty(googleJsonKeyFileName) || !File.Exists(googleJsonKeyFileName))
        {
            SeLogger.Error("Google Service Account JSON key file path is invalid or file does not exist.");
            return false;
        }

        System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", googleJsonKeyFileName);

        TextToSpeechClient client;
        try
        {
            // 2. Create the TextToSpeechClient. It will pick up credentials from the environment variable.
            client = await TextToSpeechClient.CreateAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            SeLogger.Error($"Failed to create Google Text-to-Speech client for voice list. Ensure JSON key file is valid and has 'Cloud Text-to-Speech API User' permissions: {ex.Message}");
            return false;
        }

        try
        {
            // 3. Call the ListVoicesAsync method from the client library.
            // This method returns a strongly-typed ListVoicesResponse object.
            ListVoicesResponse response = await client.ListVoicesAsync(new ListVoicesRequest(), cancellationToken: cancellationToken);

            // 4. Serialize the ListVoicesResponse object back into JSON format.
            // The ListVoicesResponse object automatically has a 'Voices' property which will be serialized
            // as the "voices" array in the JSON, matching your desired output.
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true, // To get the pretty-printed, indented format you showed
                // You might need other options depending on specific serialization needs,
                // e.g., PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            };

            string jsonContent = JsonSerializer.Serialize(response, jsonSerializerOptions);

            // 5. Write the JSON string content to the MemoryStream.
            byte[] buffer = Encoding.UTF8.GetBytes(jsonContent);
            await ms.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
            ms.Position = 0; // Reset position for reading from the beginning of the stream

            return true;
        }
        catch (Google.GoogleApiException gex)
        {
            // Catch specific Google API exceptions for detailed error messages.
            SeLogger.Error($"Google TTS API error getting voice list: {gex.HttpStatusCode} - {gex.Message} (Details: {gex.Error?.Message})");
            return false;
        }
        catch (Exception ex)
        {
            // Catch any other unexpected exceptions.
            SeLogger.Error($"An unexpected exception occurred while getting Google TTS voice list: {ex}");
            return false;
        }
        finally
        {
            // As discussed before, for a desktop app, setting GOOGLE_APPLICATION_CREDENTIALS
            // once at user config is usually sufficient. No need to unset it here unless
            // your app's lifecycle demands strict credential management per call.
        }
    }

    /// <summary>
    /// Downloads speech audio from Google Text-to-Speech API using a service account JSON key file for authentication.
    /// </summary>
    /// <param name="inputText">The text to synthesize.</param>
    /// <param name="googleVoice">Object containing LanguageCode and Name for the voice.</param>
    /// <param name="model">Currently unused, but kept for signature compatibility.</param>
    /// <param name="googleJsonKeyFileName">The full path to the Google service account JSON key file.</param>
    /// <param name="ms">The MemoryStream to write the audio content to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if speech synthesis is successful, false otherwise.</returns>
    public async Task<bool> DownloadGoogleVoiceSpeak(string inputText, GoogleVoice googleVoice, string model, string googleJsonKeyFileName, MemoryStream ms, CancellationToken cancellationToken)
    {
        // 1. Set the GOOGLE_APPLICATION_CREDENTIALS environment variable.
        // This is the CRITICAL step for authentication with the service account JSON key file.
        // The Google Cloud client libraries automatically look for this environment variable.
        // Set it before creating the TextToSpeechClient.
        if (string.IsNullOrEmpty(googleJsonKeyFileName) || !File.Exists(googleJsonKeyFileName))
        {
            SeLogger.Error("Google Service Account JSON key file path is invalid or file does not exist.");
            return false;
        }

        // Setting this variable tells the Google Cloud client library where to find the credentials.
        // This setting applies to the current process.
        System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", googleJsonKeyFileName);

        TextToSpeechClient client;
        try
        {
            // 2. Create the TextToSpeechClient.
            // This will automatically use the credentials specified by GOOGLE_APPLICATION_CREDENTIALS.
            client = await TextToSpeechClient.CreateAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            SeLogger.Error($"Failed to create Google Text-to-Speech client. Ensure the JSON key file is valid and has 'Cloud Text-to-Speech API User' permissions: {ex.Message}");
            // It's good practice to clear the environment variable if creation fails,
            // or if you want to explicitly control its lifetime.
            // System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", null);
            return false;
        }

        try
        {
            // 3. Define the synthesis request using the client library's types
            SynthesisInput input = new SynthesisInput
            {
                Text = inputText
            };

            VoiceSelectionParams voice = new VoiceSelectionParams
            {
                LanguageCode = googleVoice.LanguageCode,
                Name = googleVoice.Name
            };

            // You can specify more audio settings here, e.g., Pitch, SpeakingRate
            AudioConfig audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3,
                // Uncomment and use if your GoogleVoice class has SpeakingRate:
                // SpeakingRate = googleVoice.SpeakingRate ?? 1.0f // Ensure this is a float/double
            };

            // 4. Perform the speech synthesis asynchronously
            var response = await client.SynthesizeSpeechAsync(input, voice, audioConfig, cancellationToken: cancellationToken);

            // 5. Write the audio content to the MemoryStream
            if (response?.AudioContent?.Length > 0)
            {
                // The client library provides the audio content as a ByteString,
                // which can be converted to a byte array.
                await ms.WriteAsync(response.AudioContent.ToByteArray(), cancellationToken);
                ms.Position = 0; // Reset position for reading from the beginning of the stream
                return true;
            }
            else
            {
                SeLogger.Error("Google TTS: Audio content is null or empty in the response.");
                return false;
            }
        }
        catch (Google.GoogleApiException gex)
        {
            // Catch specific Google API exceptions for more detailed error handling.
            // This provides status codes and messages directly from the API.
            SeLogger.Error($"Google TTS API error: {gex.HttpStatusCode} - {gex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            // Catch any other unexpected exceptions.
            SeLogger.Error($"An unexpected exception occurred during Google TTS synthesis: {ex}");
            return false;
        }
        finally
        {
            // IMPORTANT:
            // For a desktop application where the user selects a file,
            // it's generally fine to leave GOOGLE_APPLICATION_CREDENTIALS set
            // as long as the application is running and using the same credentials.
            // If you foresee users switching accounts or if you want to be
            // extremely strict about credential lifetime, you could unset it here.
            // However, setting it once upon successful file selection is typically sufficient.
            // Example: System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", null);
        }
    }

    private class GoogleTtsResponse
    {
        public string? audioContent { get; set; }
    }
}