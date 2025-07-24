using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Media;

public static class FindVideoFileName
{
    public static bool TryFindVideoFileName(string inputFileName, out string videoFileName)
    {
        return TryFindVideoFileNameInner(inputFileName, out videoFileName, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
    }

    public static bool TryFindVideoFileNameInner(string inputFileName, out string videoFileName, HashSet<string> videoFileNamesTried)
    {
        videoFileName = string.Empty;

        if (string.IsNullOrEmpty(inputFileName))
        {
            return false;
        }

        if (videoFileNamesTried.Contains(inputFileName))
        {
            return false;
        }
        videoFileNamesTried.Add(inputFileName);

        foreach (var extension in Utilities.VideoFileExtensions.Concat(Utilities.AudioFileExtensions))
        {
            var fileName = inputFileName + extension;
            if (File.Exists(fileName))
            {
                videoFileName = fileName;
                return true;
            }
        }

        var index = inputFileName.LastIndexOf('.');
        if (index > 0 && TryFindVideoFileName(inputFileName.Remove(index), out videoFileName))
        {
            return true;
        }

        index = inputFileName.LastIndexOf('_');
        if (index > 0 && TryFindVideoFileName(inputFileName.Remove(index), out videoFileName))
        {
            return true;
        }

        return false;
    }
}
