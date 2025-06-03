using Nikse.SubtitleEdit.Core.Common;
using System.Net.Http.Headers;
using System.Text;
using SkiaSharp;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Threading;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Common.Ocr;

public class OllamaOcr
{
    private readonly HttpClient _httpClient;

    public string Error { get; set; }

    public OllamaOcr()
    {
        Error = string.Empty;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
        _httpClient.Timeout = TimeSpan.FromMinutes(25);
    }

    public async Task<string> Ocr(SKBitmap bitmap, string model, string language, CancellationToken cancellationToken)
    {
        try
        {
            var modelJson = "\"model\": \"" + model + "\",";

            var prompt = string.Format("Get the text (use '\\n' for new line) from this image in {0}. Return only the text - no commnts or notes. For new line, use '\\n'.", language);
            var input = "{ " + modelJson + "  \"messages\": [ { \"role\": \"user\", \"content\": \"" + prompt + "\", \"images\": [ \"" + bitmap.ToBase64String() + "\"] } ] }";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = await _httpClient.PostAsync("http://localhost:11434/api/chat", content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("Error calling Ollama for OCR: Status code=" + result.StatusCode + Environment.NewLine + json);
            }

            result.EnsureSuccessStatusCode();

            var parser = new SeJsonParser();
            var outputTexts = parser.GetAllTagsByNameAsStrings(json, "content");
            var resultText = string.Join(string.Empty, outputTexts).Trim();

            // sanitize
            resultText = resultText.Trim();
            resultText = resultText.Replace("\\n", Environment.NewLine);
            resultText = resultText.Replace(" ,", ",");
            resultText = resultText.Replace(" .", ".");
            resultText = resultText.Replace(" !", "!");
            resultText = resultText.Replace(" ?", "?");
            resultText = resultText.Replace("( ", "(");
            resultText = resultText.Replace(" )", ")");
            resultText = resultText.Replace("\\\"", "\"");
            if (resultText.EndsWith("!'"))
            {
                resultText = resultText.TrimEnd('\'');
            }

            return resultText.Trim();
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, "Error calling Ollama for OCR");
            return string.Empty;
        }
    }
}
