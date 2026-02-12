# SeConv - Subtitle Converter Command Line Tool

A modern command-line utility for batch converting subtitle files between various formats.

## Features

- ✅ Batch conversion of subtitle files
- ✅ Support for 300+ subtitle formats
- ✅ Colored console output for better readability
- ✅ Extensive subtitle processing operations
- ✅ Modern command-line interface with Spectre.Console

## Installation

Build the project:
```bash
dotnet build SeConv/SeConv.csproj
```

## Usage

### Basic Syntax

```bash
SubtitleEdit convert <pattern> <format> [options]
```

### Examples

**Convert all SRT files to SAMI format:**
```bash
SubtitleEdit convert *.srt sami
```

**Convert with specific encoding:**
```bash
SubtitleEdit convert *.srt subrip --encoding:windows-1252
```

**Convert frame-based subtitles to time-based:**
```bash
SubtitleEdit convert *.sub subrip --fps:25 --outputfolder:C:\Temp
```

**List all available formats:**
```bash
SubtitleEdit formats
```

## Command Line Options

### File Options
- `--inputfolder:<path>` - Input folder path
- `--outputfolder:<path>` - Output folder path
- `--outputfilename:<name>` - Output file name (for single file only)
- `--overwrite` - Overwrite existing files

### Format Options
- `--encoding:<name>` - Character encoding (e.g., utf-8, windows-1252)
- `--fps:<rate>` - Frame rate for conversion
- `--targetfps:<rate>` - Target frame rate

### Processing Operations

Operations are applied in the order specified on the command line:

- `--ApplyDurationLimits` - Apply duration limits
- `--BalanceLines` - Balance line lengths
- `--BeautifyTimeCodes` - Beautify time codes
- `--FixCommonErrors` - Fix common subtitle errors
- `--MergeSameTexts` - Merge entries with same text
- `--MergeSameTimeCodes` - Merge entries with same time codes
- `--RemoveFormatting` - Remove formatting tags
- `--RemoveTextForHI` - Remove text for hearing impaired
- `--SplitLongLines` - Split long lines

And many more...

## Supported Formats

Some popular formats include:
- SubRip (.srt)
- SAMI (.smi)
- Advanced Sub Station Alpha (.ass)
- WebVTT (.vtt)
- Adobe Encore (.txt)
- MicroDVD (.sub)
- Timed Text (.xml)

Run `SubtitleEdit formats` to see the complete list of 300+ supported formats.

## Getting Help

```bash
SubtitleEdit --help
SubtitleEdit /?
```

## Project Structure

```
SeConv/
├── Commands/
│   ├── ConvertCommand.cs     # Main conversion command
│   └── FormatsCommand.cs     # List available formats
├── Core/
│   └── SubtitleConverter.cs  # Core conversion logic
├── Helpers/
│   └── HelpDisplay.cs        # Help text display
└── Program.cs                # Application entry point
```

## Dependencies

- **Spectre.Console** - Modern console UI framework
- **Spectre.Console.Cli** - Command-line parsing
- **LibSE** - Subtitle Edit core library (referenced from main project)

## Development

The project uses .NET 10.0 and C# 14.0.

To add new features:
1. The conversion logic should be implemented in `Core/SubtitleConverter.cs`
2. Commands can be added to the `Commands/` folder
3. Register new commands in `Program.cs`

## License

Part of the Subtitle Edit Avalonia project.
