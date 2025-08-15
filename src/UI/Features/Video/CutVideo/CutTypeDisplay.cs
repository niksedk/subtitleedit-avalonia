using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.CutVideo;

public class CutTypeDisplay
{
    public string Name { get; set; }
    public CutType CutType { get; set; }

    public CutTypeDisplay(string name, CutType cutType)
    {
        Name = name;
        CutType = cutType;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<CutTypeDisplay> GetCutTypes()
    {
        return new List<CutTypeDisplay>
        {
            new CutTypeDisplay(Se.Language.Video.CutVideoCutSegments, CutType.CutSegments),
            new CutTypeDisplay(Se.Language.Video.CutVideoMergeSegments, CutType.MergeSegments),
            new CutTypeDisplay(Se.Language.Video.CutVideoSplitSegments, CutType.SplitSegments)
        };
    }
}
