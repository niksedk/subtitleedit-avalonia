using System.IO;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ExportHandlerBdnXml : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.BdnXml;
    public string Extension => ".xml";
    public bool UseFileName => true;
    public string Title => "Export to BDN XML";

    private int _width;
    private int _height;
    private FileStream? _fileStream;


    public void WriteHeader(string fileOrFolderName, ImageParameter imageParameter)
    {
        _width = imageParameter.ScreenWidth;
        _height = imageParameter.ScreenHeight;
        _fileStream = new FileStream(fileOrFolderName, FileMode.Create);
    }

    public void CreateParagraph(ImageParameter param)
    {
    }

    public void WriteParagraph(ImageParameter param)
    {
        _fileStream!.Write(param.Buffer, 0, param.Buffer.Length);
    }

    public void WriteFooter()
    {
        _fileStream!.Close();
    }
}