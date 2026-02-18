---
layout: default
title: Third-Party Components
---

# Third-Party Components

Subtitle Edit uses several third-party tools for features like video playback, audio extraction, and OCR. While Subtitle Edit includes built-in downloaders for these components, you might want to use a specific version or a custom build.

> **⚠️ Warning**
> Subtitle Edit is tested with specific versions of these components. Using other versions is **not officially supported** and may cause instability.
>

## Where are the files located?

Subtitle Edit stores these components in its **Data Folder**.

*   **Portable Version:** The folder containing `SubtitleEdit.exe` (Windows) or the executable.
*   **Installed Version (Windows):** `%APPDATA%\Subtitle Edit`
    *   (Press `Win+R`, type `%APPDATA%\Subtitle Edit`, and hit Enter)
*   **Linux:** `~/.config/Subtitle Edit` (or `$XDG_CONFIG_HOME/Subtitle Edit`).
*   **macOS:** `~/Library/Application Support/Subtitle Edit`.

> **Tip:** You can open the Data Folder directly from Subtitle Edit by pressing `Ctrl+Alt+Shift+D` (Windows/Linux) or `Cmd+Alt+Shift+D` (macOS).

---

## 1. FFmpeg
Used for reading media info, extracting audio, and generating waveforms.

*   **Download files from:** [ffmpeg.org](https://ffmpeg.org/download.html)
*   **Destination Folder:** `[Data Folder]/ffmpeg`

### Windows
*   Look for builds from `gyan.dev` or `BtbN`. Use "release-essentials" or "release-full".
*   Extract `ffmpeg.exe` (usually found in the `bin` folder of the download) into the `ffmpeg` folder.
*   (Optional) `ffprobe.exe` can also be placed here.

### Linux / macOS
*   Install via package manager (e.g., `sudo apt install ffmpeg`, `brew install ffmpeg`) or download static builds.
*   Subtitle Edit will look for `ffmpeg` in system paths (e.g. `/usr/bin/ffmpeg`, `/opt/homebrew/bin/ffmpeg`).
*   Alternatively, place the `ffmpeg` binary in `[Data Folder]/ffmpeg`.

## 2. MPV Media Player (libmpv)
Used as a video player engine.

### Windows
*   **Download:** [mpv-winbuild-cmake Releases](https://github.com/shinchiro/mpv-winbuild-cmake/releases)
    *   Look for files starting with **`mpv-dev-...`** (e.g., `mpv-dev-x86_64-...`).
*   **Destination:** `[Data Folder]` (The root data folder)
*   **Files:** Extract `libmpv-2.dll` to the **root** of the Data Folder.

### Linux
*   **Install:** Use your package manager to install `libmpv` (e.g., `sudo apt install libmpv2` or `libmpv-dev`).
*   **Files:** Subtitle Edit looks for `libmpv.so.2` or `libmpv.so` in standard library paths (`/usr/lib`, `/usr/local/lib`, etc.).

### macOS
*   **Install:** Use Homebrew (e.g., `brew install mpv`).
*   **Files:** Subtitle Edit looks for `libmpv.dylib` or `libmpv.2.dylib` in standard library paths (`/opt/homebrew/lib`, `/usr/local/lib`, etc.).

## 3. Tesseract OCR
Used for converting image-based subtitles (Sup/VobSub) to text.

### Windows
*   **Download:** [UB-Mannheim Tesseract](https://github.com/UB-Mannheim/tesseract/wiki).
*   **Destination:** `[Data Folder]/Tesseract550`
*   **Files:** The content of the installation folder (containing `tesseract.exe` and `tessdata` folder) should be placed here.

### Linux / macOS
*   **Install:** Use package manager (e.g., `sudo apt install tesseract-ocr`, `brew install tesseract`).
*   **Files:** Subtitle Edit will detect the system installation. Ensure language data (`tessdata`) is also installed (often separate packages on Linux).

## 4. Whisper (Speech-to-Text)
Used for AI-based speech recognition.

*   **Destination Folder:** `[Data Folder]/Whisper`
*   It is generally recommended to use the internal downloader for Whisper due to the complexity of model and library dependencies.

### 4.1 Whisper CPP
*   **Download files from:** [ggerganov/whisper.cpp releases](https://github.com/ggerganov/whisper.cpp/releases)
*   **Destination Folder:** `[Data Folder]/Whisper/Cpp`
*   **Files to place:**
    *   **Windows:** Download the Windows zip, extract `main.exe` and rename to **`whisper-cli.exe`**.
    *   **Linux/macOS:** Download or build `main` binary and rename to **`whisper-cli`**.
    *   **All Platforms:** Models (`.bin` files) go into a `Models` subfolder: `[Data Folder]/Whisper/Cpp/Models`.

### 4.2 Purfview Faster-Whisper (GPU)
*   **Download files from:** [Purfview/whisper-standalone-win releases](https://github.com/Purfview/whisper-standalone-win/releases)
*   **Destination Folder:** `[Data Folder]/Whisper/Purfview-Whisper-Faster`
*   **Files to place:**
    *   **Windows:** Download the Standalone Archive, extract contents so `faster-whisper-xxl.exe` is in the folder root.
    *   **Linux:** Download the Linux Archive, extract so `faster-whisper-xxl` binary is present.
    *   **Models:** Place model directories (e.g., `faster-whisper-medium`) inside the `_models` folder.
