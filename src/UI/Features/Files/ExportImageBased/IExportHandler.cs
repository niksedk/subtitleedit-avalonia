namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public interface IExportHandler
{
    ExportImageType ExportImageType { get; set; }
    string Extension { get; }
    string Title { get; }
    bool UseFileName { get; }
    public void WriteHeader(string fileOrFolderName, int width, int height);
    void CreateParagraph(ImageParameter param);
    void WriteParagraph(ImageParameter param);
    public void WriteFooter();
}