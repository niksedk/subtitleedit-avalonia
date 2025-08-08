using System.IO;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ExportHandlerVobSub : IExportHandler
{
    public ExportImageType ExportImageType { get; set; }
    public string Extension => ".sub";
    public bool UseFileName => true;    
    public string Title => "Export to VobSub/idx";

    private int _width;
    private int _height;
    private FileStream? _fileStream;

    public void WriteHeader(string fileOrFolderName, int width, int height)
    {
        _width = width;
        _height = height;
        _fileStream = new FileStream(fileOrFolderName, FileMode.Create);
    }

    public void CreateParagraph(ImageParameter param)
    {
    }

    public void WriteParagraph(ImageParameter param)
    {
        _fileStream?.Write(param.Buffer, 0, param.Buffer.Length);    
    }

    public void WriteFooter()
    {
        _fileStream!.Close();
    }
}