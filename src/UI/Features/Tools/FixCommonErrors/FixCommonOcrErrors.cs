using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixCommonOcrErrors : IFixCommonError
    {
        public static IOcrFixEngine2? OcrFixEngine { get; internal set; }

        public static class Language
        {
            public static string FixText { get; set; } = "Fix common OCR errors";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            if (OcrFixEngine == null)
            {
                return;
            }

            var language = callbacks.Language;
            var threeLetterCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(language);
            if (string.IsNullOrEmpty(threeLetterCode))
            {
                return;
            }

            var ocrSubtitle = new OcrSubtitleDummy(subtitle);
            var spellChecker = new SpellCheckDictionaryDisplay();
            spellChecker.DictionaryFileName = ""; //TODO: find

            if (string.IsNullOrEmpty(spellChecker.DictionaryFileName))
            {
                return;
            }

            OcrFixEngine.Initialize(ocrSubtitle.MakeOcrSubtitleItems(), threeLetterCode, spellChecker);

            var fixAction = Language.FixText;
            var fixCount = 0;
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];


            }

            callbacks.UpdateFixStatus(fixCount, fixAction);
        }
    }
}