﻿using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper.Engines;

public class WhisperEngineCpp : IWhisperEngine
{
    public static string StaticName => "Whisper CPP";
    public string Name => StaticName;
    public string Choice => WhisperChoice.Cpp;
    public string Url => "https://github.com/ggerganov/whisper.cpp";

    public List<WhisperLanguage> Languages => WhisperLanguage.Languages.OrderBy(p => p.Name).ToList();

    public List<WhisperModel> Models
    {
        get
        {
            var models = new WhisperCppModel().Models;
            return models.ToList();
        }
    }

    public string Extension => ".bin";
    public string UnpackSkipFolder => string.Empty;

    public bool IsEngineInstalled()
    {
        var executableFile = GetExecutable();
        return File.Exists(executableFile);
    }

    public override string ToString()
    {
        return Name;
    }

    public string GetAndCreateWhisperFolder()
    {
        var baseFolder = Se.WhisperFolder;
        if (!Directory.Exists(baseFolder))
        {
            Directory.CreateDirectory(baseFolder);
        }

        var folder = System.IO.Path.Combine(baseFolder, "Cpp");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        var baseFolder = GetAndCreateWhisperFolder();

        var folder = System.IO.Path.Combine(baseFolder, "Models");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public string GetExecutable()
    {
        var fullPath = System.IO.Path.Combine(GetAndCreateWhisperFolder(), GetExecutableFileName());

        if (!File.Exists(fullPath) && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string[] paths = ["/usr/bin/whisper-cli", "usr/local/bin/"];
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
        }

        return fullPath;
    }

    public bool IsModelInstalled(WhisperModel model)
    {
        var baseFolder = GetAndCreateWhisperFolder();
        var folder = System.IO.Path.Combine(baseFolder, "Models");
        if (!Directory.Exists(folder))
        {
            return false;
        }

        var modelFileName = System.IO.Path.Combine(folder, model.Name);
        if (Extension.Length > 0 && !modelFileName.EndsWith(Extension))
        {
            modelFileName += Extension;
        }

        if (!File.Exists(modelFileName))
        {
            return false;
        }

        var fileInfo = new FileInfo(modelFileName);
        return fileInfo.Length > 10_000_000;
    }

    public string GetModelForCmdLine(string modelName)
    {
        var modelFileName = System.IO.Path.Combine(GetAndCreateWhisperModelFolder(null), modelName);
        if (Extension.Length > 0 && !modelFileName.EndsWith(Extension))
        {
            modelFileName += Extension;
        }

        return modelFileName;
    }

    public async Task<string> GetHelpText()
    {
        var assetName = $"{StaticName.Replace(" ", string.Empty)}.txt";
        var uri = new Uri($"avares://SubtitleEdit/Assets/Whisper/{assetName}");

        await using var stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);

        var contents = await reader.ReadToEndAsync();
        return contents;
    }

    public string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url)
    {
        var folder = GetAndCreateWhisperModelFolder(whisperModel);
        var fileName = System.IO.Path.Combine(folder, whisperModel.Name + Extension);
        return fileName;
    }

    private static string GetExecutableFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "whisper-cli.exe";
        }

        return "whisper-cli";
    }
}