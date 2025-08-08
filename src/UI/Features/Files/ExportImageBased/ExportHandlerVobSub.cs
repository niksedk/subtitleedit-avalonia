using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;
using System;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ExportHandlerVobSub : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.VobSub;
    public string Extension => ".sub";
    public bool UseFileName => true;    
    public string Title => "Export to VobSub/idx";

    private int _width;
    private int _height;
    VobSubWriter? _vobSubWriter;

    public void WriteHeader(string fileOrFolderName, int width, int height)
    {
        _width = width;
        _height = height;
        _vobSubWriter = new VobSubWriter(fileOrFolderName, width, height, 10, 10, 32, SKColors.Wheat, SKColors.Black, true, DvdSubtitleLanguage.English);
//        _vobSubWriter = new VobSubWriter(fileOrFolderName, width, height, GetBottomMarginInPixels(p), GetLeftMarginInPixels(p), 32, _subtitleColor, _borderColor, !checkBoxTransAntiAliase.Checked, (DvdSubtitleLanguage)comboBoxLanguage.SelectedItem);
    }

    public void CreateParagraph(ImageParameter param)
    {
    }

    public void WriteParagraph(ImageParameter param)
    {
        if (_vobSubWriter == null)
        {
            throw new InvalidOperationException("VobSubWriter is not initialized. Call WriteHeader first.");
        }

        var p = new Paragraph(param.Text, param.StartTime.TotalMilliseconds, param.EndTime.TotalMilliseconds);
        BluRayContentAlignment alignment = BluRayContentAlignment.BottomCenter;
        _vobSubWriter.WriteParagraph(p, param.Bitmap,  alignment);
    }

    public void WriteFooter()
    {
        if (_vobSubWriter == null)
        {
            throw new InvalidOperationException("VobSubWriter is not initialized. Call WriteHeader first.");
        }

        _vobSubWriter.WriteIdxFile();
        _vobSubWriter.Dispose();
    }
}