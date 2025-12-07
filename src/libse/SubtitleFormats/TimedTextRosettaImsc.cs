using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// https://github.com/imsc-rosetta/imsc-rosetta-specification
    /// </summary>
    public class TimedTextRosettaImsc : SubtitleFormat
    {
        public override string Name => "Timed Text Rosetta IMSC";

        private static string GetXmlStructure()
        {
            return @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
<tt xmlns:rosetta=""https://github.com/imsc-rosetta/specification"" xmlns:ttp=""http://www.w3.org/ns/ttml#parameter"" xmlns:itts=""http://www.w3.org/ns/ttml/profile/imsc1#styling"" xmlns:ebutts=""urn:ebu:tt:style"" xmlns:ttm=""http://www.w3.org/ns/ttml#metadata"" xmlns:tts=""http://www.w3.org/ns/ttml#styling"" xml:lang=""fr"" xml:space=""preserve"" xmlns:xml=""http://www.w3.org/XML/1998/namespace"" ttp:cellResolution=""30 15"" ttp:frameRateMultiplier=""1000 1001"" ttp:timeBase=""media"" ttp:frameRate=""24"" xmlns=""http://www.w3.org/ns/ttml"">
  <head>
    <metadata>
      <rosetta:format>imsc-rosetta</rosetta:format>
      <rosetta:version>0.0.0</rosetta:version>
    </metadata>
    <styling>
      <style xml:id=""d_default"" style=""_d_default"" />
      <style xml:id=""r_default"" style=""_r_default"" tts:backgroundColor=""#00000000"" tts:fontFamily=""proportionalSansSerif"" tts:fontStyle=""normal"" tts:fontWeight=""normal"" tts:overflow=""visible"" tts:showBackground=""whenActive"" tts:wrapOption=""noWrap"" />
      <style xml:id=""_d_default"" style=""d_outline"" />
      <style xml:id=""_r_quantisationregion"" tts:fontSize=""6.182rh"" tts:lineHeight=""125%"" tts:origin=""10% 10%"" tts:extent=""80% 85%"" />
      <style xml:id=""_r_default"" style=""s_fg_white p_al_center"" tts:fontSize=""5.1rh"" tts:lineHeight=""125%"" tts:luminanceGain=""1.0"" itts:fillLineGap=""false"" ebutts:linePadding=""0.25c"" />
      <style xml:id=""p_al_center"" tts:textAlign=""center"" ebutts:multiRowAlign=""center"" />
      <style xml:id=""s_fg_white"" tts:color=""#FFFFFF"" />
      <style xml:id=""s_outlineblack"" tts:textOutline=""#000000 0.05em"" />
      <style xml:id=""d_outline"" style=""s_outlineblack"" />
      <style xml:id=""p_font1"" tts:fontFamily=""proportionalSansSerif"" tts:fontSize=""100%"" tts:lineHeight=""125%"" />
      <style xml:id=""s_italic"" tts:fontStyle=""italic"" />
    </styling>
    <layout>
      <region xml:id=""R0"" style=""r_default"" tts:displayAlign=""after"" tts:origin=""10% 10%"" tts:extent=""80% 85%"" />
    </layout>
  </head>
  <body>
  </body>
</tt>";
        }

        public override string Extension => Configuration.Settings.SubtitleSettings.TimedTextImsc11FileExtension;

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !(fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var text = sb.ToString();
            if (text.Contains("lang=\"ja\"", StringComparison.Ordinal) && text.Contains("bouten-", StringComparison.Ordinal))
            {
                return false;
            }

            return text.Contains("<rosetta:format>imsc-rosetta</rosetta:format>") && base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return string.Empty; 
            //TODO: implement

        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            //TODO: implement
        }
    }
}
