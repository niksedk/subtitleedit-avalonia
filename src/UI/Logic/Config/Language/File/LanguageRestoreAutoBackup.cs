namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageRestoreAutoBackup
{
    public string Title { get; set; }
    public string OpenAutoBackupFolder { get; set; }
    public string RestoreAutoBackupFile { get; set; }

    public LanguageRestoreAutoBackup()
    {
        Title = "Restore auto-backup";
        OpenAutoBackupFolder = "Open auto-backup folder";
        RestoreAutoBackupFile = "Restore auto-backup file";
    }
}
