namespace Nikse.SubtitleEdit.Logic.Config.Language.File;

public class LanguageFile
{

    public LanguageEbuSaveOptions EbuSaveOptions { get; set; } = new();
    public LanguageExport Export { get; set; } = new();

    public LanguageFile()
    {

    }
}