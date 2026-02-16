# OCR (Optical Character Recognition)

Subtitle Edit can convert image-based subtitle formats to text using OCR.

- **Menu:** File → Import image subtitle for edit (OCR)...

<!-- Screenshot: OCR window -->
![OCR Window](../screenshots/ocr.png)

## Supported Image Formats

- Blu-ray SUP (.sup)
- VobSub (.sub/.idx)
- DVD subtitles
- BDN XML
- Transport stream (DVB) subtitles
- Matroska embedded image subtitles (PGS, VobSub, DVB)
- MP4 embedded VobSub
- WebVTT with embedded images

## OCR Engines

### Tesseract
Open-source OCR engine with language packs.
- Download language packs via the OCR window
- Good general-purpose accuracy

### nOCR (Nikse OCR)
Built-in trainable OCR engine.
- Train character databases for specific fonts
- Very accurate once trained
- Best for consistent fonts (like DVD/Blu-ray subtitles)

### Binary OCR
Binary image comparison engine.
- Compares against a database of known character images
- Fast and accurate for known fonts

### Google Lens OCR
Cloud-based OCR using Google Lens.
- Requires internet connection

### Google Vision OCR
Cloud-based OCR using Google Cloud Vision API.
- Requires API key

### Ollama OCR
Local LLM-based OCR using Ollama.
- Requires Ollama installation

### Mistral OCR
Cloud-based OCR using Mistral API.

### PaddleOCR
Local OCR engine.
- Download required

## How to Use

1. Open an image-based subtitle file
2. The OCR window opens automatically
3. Select an OCR engine
4. Configure engine-specific settings
5. Click **Start OCR**
6. Review and correct any errors
7. Click **OK** to import the text subtitles

## Pre-processing

Before OCR, you can apply image pre-processing:
- Crop
- Binarize (convert to black/white)
- Invert colors
- Resize

<!-- Screenshot: OCR pre-processing -->
![OCR Pre-processing](../screenshots/ocr-preprocessing.png)

## Unknown Words

When the OCR engine encounters uncertain characters, you can:
- Choose from suggested alternatives
- Type the correct text
- Add to the OCR fix dictionary for automatic correction
