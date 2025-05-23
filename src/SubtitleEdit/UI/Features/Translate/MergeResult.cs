using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Translate;

public static partial class MergeAndSplitHelper
{
    public class MergeResult
    {
        public string Text { get; set; } = string.Empty;
        public int ParagraphCount { get; set; }
        public List<MergeResultItem> MergeResultItems { get; set; } = [];
        public bool HasError { get; set; }
        public bool NoSentenceEndingSource { get; set; }
        public bool NoSentenceEndingTarget { get; set; }
    }
}