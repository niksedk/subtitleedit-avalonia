using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Main.MainHelpers;

public class SubtitleFileService : ISubtitleFileService
{
    public async Task<SubtitleFileOpenResult> OpenSubtitleAsync(string fileName, TextEncoding encoding)
    {
        var result = new SubtitleFileOpenResult
        {
            Success = false,
            Subtitle = new Subtitle(),
            Format = null,
            FileName = fileName,
            Encoding = encoding
        };

        if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
        {
            result.ErrorMessage = "File not found";
            return result;
        }

        try
        {
            // Check if it's a Matroska file
            if (fileName.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".mks", StringComparison.OrdinalIgnoreCase))
            {
                return await OpenMatroskaFileAsync(fileName);
            }

            // Check if it's an MP4 file
            if (fileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                return await OpenMp4FileAsync(fileName);
            }

            // Check if it's a Transport Stream file
            if (fileName.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".m2ts", StringComparison.OrdinalIgnoreCase))
            {
                return await OpenTransportStreamFileAsync(fileName);
            }

            // Check if it's a DivX file
            if (IsDivXFile(fileName))
            {
                return await OpenDivXFileAsync(fileName);
            }

            // Try to open as regular subtitle file
            return await OpenRegularSubtitleFileAsync(fileName, encoding);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Error opening subtitle: {ex.Message}";
            return result;
        }
    }

    private static async Task<SubtitleFileOpenResult> OpenMatroskaFileAsync(string fileName)
    {
        var result = new SubtitleFileOpenResult
        {
            FileName = fileName,
            Subtitle = new Subtitle()
        };

        await Task.Run(() =>
        {
            try
            {
                using var matroska = new MatroskaFile(fileName);
                if (!matroska.IsValid)
                {
                    result.ErrorMessage = "Invalid Matroska file";
                    return;
                }

                var tracks = matroska.GetTracks(true);
                if (tracks.Count == 0)
                {
                    result.ErrorMessage = "No subtitle tracks found in Matroska file";
                    return;
                }

                // Use first subtitle track for now
                var track = tracks[0];
                var subtitles = matroska.GetSubtitle(track.TrackNumber, null);
                result.Format = Utilities.LoadMatroskaTextSubtitle(track, matroska, subtitles, result.Subtitle);
                result.Success = result.Subtitle.Paragraphs.Count > 0;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error reading Matroska file: {ex.Message}";
            }
        });

        return result;
    }

    private async Task<SubtitleFileOpenResult> OpenMp4FileAsync(string fileName)
    {
        var result = new SubtitleFileOpenResult
        {
            FileName = fileName,
            Subtitle = new Subtitle()
        };

        await Task.Run(() =>
        {
            try
            {
                var mp4 = new MP4Parser(fileName);
                var tracks = mp4.GetSubtitleTracks();
                if (tracks.Count == 0)
                {
                    result.ErrorMessage = "No subtitle tracks found in MP4 file";
                    return;
                }

                // Use first subtitle track
                var track = tracks[0];
                if (track.Mdia?.Minf?.Stbl != null)
                {
                    result.Subtitle.Paragraphs.AddRange(track.Mdia.Minf.Stbl.GetParagraphs());
                    result.Format = new SubRip();
                    result.Success = result.Subtitle.Paragraphs.Count > 0;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error reading MP4 file: {ex.Message}";
            }
        });

        return result;
    }

    private async Task<SubtitleFileOpenResult> OpenTransportStreamFileAsync(string fileName)
    {
        var result = new SubtitleFileOpenResult
        {
            FileName = fileName,
            Subtitle = new Subtitle()
        };

        await Task.Run(() =>
        {
            try
            {
                var tsParser = new TransportStreamParser();
                tsParser.Parse(fileName, null);

                if (tsParser.SubtitlePacketIds.Count == 0)
                {
                    result.ErrorMessage = "No subtitle tracks found in Transport Stream file";
                    return;
                }

                // Use first subtitle track
                var subtitles = tsParser.GetDvbSubtitles(tsParser.SubtitlePacketIds[0]);
                if (subtitles != null && subtitles.Count > 0)
                {
                    foreach (var sub in subtitles)
                    {
                        result.Subtitle.Paragraphs.Add(new Paragraph(
                            string.Empty, 
                            sub.StartMilliseconds, 
                            sub.EndMilliseconds));
                    }
                    result.Format = new SubRip();
                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error reading Transport Stream file: {ex.Message}";
            }
        });

        return result;
    }

    private static async Task<SubtitleFileOpenResult> OpenDivXFileAsync(string fileName)
    {
        var result = new SubtitleFileOpenResult
        {
            FileName = fileName,
            Subtitle = new Subtitle()
        };

        await Task.Run(() =>
        {
            try
            {
                var subs = DivXSubParser.ImportSubtitleFromDivX(fileName);
                if (subs.Count > 0)
                {
                    foreach (var xsub in subs)
                    {
                        result.Subtitle.Paragraphs.Add(new Paragraph(
                            string.Empty,
                            xsub.Start.TotalMilliseconds,
                            xsub.End.TotalMilliseconds));
                    }
                    result.Format = new SubRip();
                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error reading DivX file: {ex.Message}";
            }
        });

        return result;
    }

    private static async Task<SubtitleFileOpenResult> OpenRegularSubtitleFileAsync(string fileName, TextEncoding encoding)
    {
        var result = new SubtitleFileOpenResult
        {
            FileName = fileName,
            Subtitle = new Subtitle()
        };

        await Task.Run(() =>
        {
            try
            {
                // Detect encoding if not specified
                var enc = encoding.Encoding;
                if (encoding.UseSourceEncoding || enc == null)
                {
                    enc = LanguageAutoDetect.GetEncodingFromFile(fileName);
                }

                // Read file content
                var lines = FileUtil.ReadAllLinesShared(fileName, enc);

                // Try to detect format and load subtitle
                var format = result.Subtitle.ReloadLoadSubtitle(lines, fileName, null);
                
                if (format != null && result.Subtitle.Paragraphs.Count > 0)
                {
                    result.Format = format;
                    result.Success = true;
                    result.Encoding = new TextEncoding(enc, enc.WebName);
                }
                else
                {
                    result.ErrorMessage = "Unable to detect subtitle format or no subtitles found";
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error reading subtitle file: {ex.Message}";
            }
        });

        return result;
    }

    private static bool IsDivXFile(string fileName)
    {
        try
        {
            var buffer = FileUtil.ReadBytesShared(fileName, 16);
            if (buffer.Length < 4)
            {
                return false;
            }

            // Check for DivX subtitle marker
            return buffer[0] == 0x5B && buffer[1] == 0x00; // Simplified check
        }
        catch
        {
            return false;
        }
    }
}

public interface ISubtitleFileService
{
    Task<SubtitleFileOpenResult> OpenSubtitleAsync(string fileName, TextEncoding encoding);
}

public class SubtitleFileOpenResult
{
    public bool Success { get; set; }
    public Subtitle Subtitle { get; set; } = new();
    public SubtitleFormat? Format { get; set; }
    public string FileName { get; set; } = string.Empty;
    public TextEncoding? Encoding { get; set; }
    public string? ErrorMessage { get; set; }
}