using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

public class AutoBackup : IAutoBackup
{
    private static readonly Regex RegexFileNamePattern = new Regex(@"^\d\d\d\d-\d\d-\d\d_\d\d-\d\d-\d\d", RegexOptions.Compiled);

    public bool SaveAutoBackup(Subtitle subtitle, SubtitleFormat saveFormat)
    {
        if (subtitle.Paragraphs.Count == 0)
        {
            return false;
        }

        var folder = Se.AutoBackupFolder;
        if (!Directory.Exists(folder))
        {
            try
            {
                Directory.CreateDirectory(folder);
            }
            catch 
            {
                return false;
            }
        }

        var title = string.Empty;
        if (!string.IsNullOrEmpty(subtitle.FileName))
        {
            title = "_" + Path.GetFileNameWithoutExtension(subtitle.FileName);
        }

        var fileName = Path.Combine(folder, $"{DateTime.Now.Year:0000}-{DateTime.Now.Month:00}-{DateTime.Now.Day:00}_{DateTime.Now.Hour:00}-{DateTime.Now.Minute:00}-{DateTime.Now.Second:00}{title}{saveFormat.Extension}");
        try
        {
            var format = saveFormat.IsTextBased ? saveFormat : new SubRip();
            File.WriteAllText(fileName, format.ToText(subtitle, string.Empty));
            return true;
        }
        catch 
        {
            return false;
        }
    }

    public List<string> GetAutoBackupFiles()
    {
        var result = new List<string>();
        var folder = Se.AutoBackupFolder;
        if (Directory.Exists(folder))
        {
            var files = Directory.GetFiles(folder, "*.*");
            foreach (var fileName in files)
            {
                var path = Path.GetFileName(fileName);
                if (RegexFileNamePattern.IsMatch(path))
                {
                    result.Add(fileName);
                }
            }
        }

        return result;
    }
}

public interface IAutoBackup
{
    bool SaveAutoBackup(Subtitle subtitle, SubtitleFormat saveFormat);
    List<string> GetAutoBackupFiles();
}
