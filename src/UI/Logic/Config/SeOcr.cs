using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeOcr
{
    public string Engine { get; set; }
    public string NOcrDatabase { get; set; }
    public int NOcrMaxWrongPixels { get; set; }
    public int NOcrPixelsAreSpace { get; set; }
    public bool NOcrDrawUnknownText { get; set; }
    public List<string> OllamaModels { get; set; }
    public string OllamaModel { get; set; }
    public string OllamaUrl { get; set; }
    public string OllamaLanguage { get; set; }
    public string GoogleVisionApiKey { get; set; }
    public string GoogleVisionLanguage { get; set; }
    public string MistralApiKey { get; set; }
    public bool IsNewLetterItalic { get; set; }
    public bool SubmitOnFirstLetter { get; set; }
    public int NOcrNoOfLinesToAutoDraw { get; set; }
    public string PaddleOcrMode { get; set; }
    public string PaddleOcrLastLanguage { get; set; }

    public SeOcr()
    {
        Engine = "nOCR";

        NOcrDatabase = "Latin";
        NOcrMaxWrongPixels = 25;
        NOcrPixelsAreSpace = 12;
        NOcrDrawUnknownText = true;
        NOcrNoOfLinesToAutoDraw = 60;

        OllamaModels = new List<string> { "llama3.2-vision", "llava-phi3", "moondream", "minicpm-v" };
        OllamaLanguage = "English";
        OllamaModel = OllamaModels.First();
        OllamaUrl = "http://localhost:11434/api/chat/";

        GoogleVisionApiKey = string.Empty;
        GoogleVisionLanguage = "en";

        PaddleOcrMode = "mobile";
        PaddleOcrLastLanguage = "en";

        MistralApiKey = string.Empty;
    }
}