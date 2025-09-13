namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageShotChanges
{
    public string TitleGenerateOrImport { get; set; }
    public string GenerateShotChanges { get; set; }
    public string ImportShotChanges { get; set; }
    public string GenerateShotChangesWithFfmpeg { get; set; }
    public string ShotChangeTimeCode { get; set; }

    public LanguageShotChanges()
    {
        TitleGenerateOrImport = "Generate/import shot changes";
        GenerateShotChanges = "Generate shot changes";
        ImportShotChanges = "Import shot changes";
        GenerateShotChangesWithFfmpeg = "Generate shot changes with ffmpeg";
        ShotChangeTimeCode = "Shot change time code";
    }
}