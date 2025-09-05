using Nikse.SubtitleEdit.Features.Shared.Ocr.Engines;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public class OcrEngineItem
{
    public string Name { get; set; }
    public OcrEngineType EngineType { get; set; }
    public string Description { get; set; }
    public string ApiKey { get; set; }
    public string Endpoint { get; set; }

    public OcrEngineItem(string name, OcrEngineType engineType, string description, string apiKey, string endpoint)
    {
        Name = name;
        EngineType = engineType;
        Description = description;
        ApiKey = apiKey;
        Endpoint = endpoint;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<OcrEngineItem> GetOcrEngines()
    {
        return new List<OcrEngineItem>
        {
            new("nOcr", OcrEngineType.nOcr, "nOcr is an internal OCR engine (free/open source)", "", ""),
            new("Tesseract", OcrEngineType.Tesseract, "Tesseract is an open-source OCR engine", "", ""),
            new("Paddle OCR Standalone", OcrEngineType.PaddleOcrStandalone, "Paddle OCR Standalone", "", ""),
            new("Paddle OCR Python", OcrEngineType.PaddleOcrPython, "Paddle OCR Python", "", ""),
            new("Ollama", OcrEngineType.Ollama, "Ollama e.g. via llama-vision", "", "http://localhost:11434/api/chat"),
            new("Google Vision", OcrEngineType.GoogleVision, "Google Vision is a cloud-based OCR engine by Google", "", ""),
            new("Azure Vision", OcrEngineType.AzureVision, "Azure Vision is a cloud-based OCR engine by Microsoft", "", ""),
            new("Amazon Rekognition", OcrEngineType.AmazonRekognition, "Amazon Rekognition is a cloud-based OCR engine by Amazon", "", ""),
            new("Mistral OCR", OcrEngineType.Mistral, "Mistral OCR is a cloud-based OCR engine", "", ""),
        };
    }
}