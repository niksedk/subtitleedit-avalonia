# Subtitle Edit (Avalonia)

Welcome to **subtitleedit-avalonia** — the early preview of the next major release of **Subtitle Edit**!

This version is being rebuilt using [Avalonia UI](https://avaloniaui.net/) to enable true cross-platform support.

> ⚙️ **Note:** Some features may still be missing, incomplete, or experimental as we actively develop this version.  
> Your feedback is invaluable in shaping the future of Subtitle Edit.

Thank you for testing and supporting Subtitle Edit! 😊

---

## 🚀 Automated Builds
You can find the latest cross-platform builds here:  
👉 [Releases](https://github.com/niksedk/subtitleedit-avalonia/releases)

---

## 💻 System Requirements

### Windows
- Minimum: Windows 10 or newer

### macOS

- **Minimum macOS version**: 10.15 (Catalina) or newer
- **Dependencies for Intel macs** (install via [MacPorts](https://www.macports.org/)):
    - `mpv`
        - MacPorts: `sudo port install mpv`
    - `ffmpeg`
        - Homebrew: `sudo port install ffmpeg`



#### Installing Subtitle Edit on macOS (Unsigned App)

Because *Subtitle Edit* is not signed with an Apple developer certificate, macOS will block it by default. You can still install and run it by following these steps:

1. **Download** and **double-click** the `.dmg` file to mount it.
2. In the window that appears, **drag `Subtitle Edit.app` into your `Applications` folder**.
3. Open the **Terminal** app (you can find it via Spotlight or in `/Applications/Utilities/`).
4. In Terminal, run the following command to remove macOS’s security quarantine flag and add adhoc code signature:
   ```bash
   sudo xattr -rd com.apple.quarantine "/Applications/Subtitle Edit.app"

   ```bash
   sudo codesign --force --deep --sign - "/Applications/Subtitle Edit.app"


### Linux
- Requires **libmpv** (install via package manager, e.g. `sudo apt install mpv libmpv-dev`)
- Requires **ffmpeg** (e.g. `sudo apt install ffmpeg`)

> ⚙️ Note: The provided builds are self-contained and do not require a separate .NET installation.

---

## ❤️ Support the Project
If you’d like to support the continued development of Subtitle Edit, please consider donating:

- [GitHub Sponsors](https://github.com/sponsors/niksedk)
- [Donate via PayPal](https://www.paypal.com/donate/?hosted_button_id=4XEHVLANCQBCU)

---
