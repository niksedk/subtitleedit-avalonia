using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

public interface IAutoBackupService
{
    void StartAutoBackup(MainViewModel mainViewModel);
    void StopAutobackup();
    public List<string> GetAutoBackupFiles();
}

public class AutoBackupService : IAutoBackupService
{
    private MainViewModel? _mainViewModel;
    private System.Timers.Timer? _timerAutoBackup;
    private static readonly Regex RegexFileNamePattern = new Regex(@"^\d\d\d\d-\d\d-\d\d_\d\d-\d\d-\d\d", RegexOptions.Compiled);

    public void StartAutoBackup(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        
        if (!Se.Settings.General.AutoBackupOn)
        {
            return;
        }
        
        var minutes = Se.Settings.General.AutoBackupIntervalMinutes;
        if (minutes < 1)
        {
            minutes = 1;
        }
        _timerAutoBackup = new System.Timers.Timer(TimeSpan.FromMinutes(minutes));
        _timerAutoBackup.Elapsed += (_, _) =>
        {
            if (!_mainViewModel.HasChanges())
            {
                return;
            }

            var saveFormat = _mainViewModel.SelectedSubtitleFormat;
            var subtitle =  _mainViewModel.GetUpdateSubtitle(); 
            SaveAutoBackup(subtitle, saveFormat);   
        };
        _timerAutoBackup.Start();
    }

    public void StopAutobackup()
    {
        _timerAutoBackup?.Stop();
    }

    private static void SaveAutoBackup(Subtitle subtitle, SubtitleFormat saveFormat)
    {
        if (subtitle.Paragraphs.Count == 0)
        {
            return;
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
                return;
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
        }
        catch
        {
            // ignore
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
