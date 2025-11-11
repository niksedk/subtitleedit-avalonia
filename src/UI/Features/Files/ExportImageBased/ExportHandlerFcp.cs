using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ExportHandlerFcp : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.Fcp;
    public string Extension => "";
    public bool UseFileName => false;
    public string Title => "Export to FCP/image";

    private string _fileOrFolderName = string.Empty; 

    public void WriteHeader(string fileOrFolderName, ImageParameter imageParameter)
    {
        _fileOrFolderName = fileOrFolderName;
    }

    public void CreateParagraph(ImageParameter param)
    {

    }

    public void WriteParagraph(ImageParameter param)
    {
    }

    public void WriteFooter()
    {
       
    }
}