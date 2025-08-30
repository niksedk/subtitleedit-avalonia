namespace Nikse.SubtitleEdit.Features.Assa;

public class AttachmentItem
{
    public string FileName { get; set; }
    public string Type { get; set; }
    public string Size { get; set; }
    public string FullFileName { get; set; }


    public AttachmentItem(string fileName)
    {
        FullFileName = fileName;
        FileName = System.IO.Path.GetFileName(fileName);
        Type = System.IO.Path.GetExtension(fileName).TrimStart('.').ToUpper();
        var fileInfo = new System.IO.FileInfo(fileName);
        Size = fileInfo.Length.ToString("N0") + " bytes";
    }

    public override string ToString()
    {
        return FileName;
    }
}
