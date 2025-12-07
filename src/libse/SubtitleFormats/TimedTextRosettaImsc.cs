using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
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
<tt xmlns:rosetta=""https://github.com/imsc-rosetta/specification"" xmlns:ttp=""http://www.w3.org/ns/ttml#parameter"" xmlns:itts=""http://www.w3.org/ns/ttml/profile/imsc1#styling"" xmlns:ebutts=""urn:ebu:tt:style"" xmlns:ttm=""http://www.w3.org/ns/ttml#metadata"" xmlns:tts=""http://www.w3.org/ns/ttml#styling"" xml:lang=""[language]"" xml:space=""preserve"" xmlns:xml=""http://www.w3.org/XML/1998/namespace"" ttp:cellResolution=""30 15"" ttp:frameRateMultiplier=""[frameRateMultiplier]"" ttp:timeBase=""media"" ttp:frameRate=""[frameRate]"" xmlns=""http://www.w3.org/ns/ttml"">
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
      <style xml:id=""s_bold"" tts:fontWeight=""bold"" />
      <style xml:id=""s_underline"" tts:textDecoration=""underline"" />
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
            var xml = new XmlDocument { XmlResolver = null };
            var xmlStructure = GetXmlStructure();
            xmlStructure = xmlStructure.Replace("[frameRate]", ((int)Math.Round(Configuration.Settings.General.CurrentFrameRate, MidpointRounding.AwayFromZero)).ToString());

            var frameDiff = Configuration.Settings.General.CurrentFrameRate % 1.0;
            if (frameDiff < 0.001)
            {
                xmlStructure = xmlStructure.Replace("[frameRateMultiplier]", "1 1");
            }
            else
            {
                xmlStructure = xmlStructure.Replace("[frameRateMultiplier]", "1000 1001");
            }

            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            xmlStructure = xmlStructure.Replace("[language]", language);

            xml.LoadXml(xmlStructure);
            var namespaceManager = new XmlNamespaceManager(xml.NameTable);
            namespaceManager.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
            var body = xml.DocumentElement.SelectSingleNode("ttml:body", namespaceManager);

            var divCounter = 1;
            foreach (var p in subtitle.Paragraphs)
            {
                var divNode = MakeDiv(xml, p, divCounter);
                body.AppendChild(divNode);
                divCounter++;
            }

            var xmlString = ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty);
            subtitle.Header = xmlString;
            return xmlString;
        }

        private static XmlNode MakeDiv(XmlDocument xml, Paragraph p, int divCounter)
        {
            var timeCodeFormat = Configuration.Settings.SubtitleSettings.TimedTextImsc11TimeCodeFormat;
            if (string.IsNullOrEmpty(timeCodeFormat))
            {
                timeCodeFormat = "hh:mm:ss.ms";
            }

            XmlNode div = xml.CreateElement("div", "http://www.w3.org/ns/ttml");

            XmlAttribute id = xml.CreateAttribute("xml", "id", "http://www.w3.org/XML/1998/namespace");
            id.InnerText = $"e_{divCounter}";
            div.Attributes.Append(id);

            XmlAttribute region = xml.CreateAttribute("region");
            region.InnerText = "R0";
            div.Attributes.Append(region);

            XmlAttribute style = xml.CreateAttribute("style");
            style.InnerText = "d_default";
            div.Attributes.Append(style);

            XmlAttribute begin = xml.CreateAttribute("begin");
            begin.InnerText = TimedText10.ConvertToTimeString(p.StartTime, timeCodeFormat);
            div.Attributes.Append(begin);

            XmlAttribute end = xml.CreateAttribute("end");
            end.InnerText = TimedText10.ConvertToTimeString(p.EndTime, timeCodeFormat);
            div.Attributes.Append(end);

            XmlNode paragraph = xml.CreateElement("p", "http://www.w3.org/ns/ttml");
            XmlAttribute pStyle = xml.CreateAttribute("style");
            pStyle.InnerText = "p_font1";
            paragraph.Attributes.Append(pStyle);

            var text = p.Text.RemoveControlCharactersButWhiteSpace();

            try
            {
                text = Utilities.RemoveSsaTags(text);
                var lines = text.SplitToLines();
                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    var paragraphContent = new XmlDocument();
                    paragraphContent.LoadXml($"<root>{line.Replace("&", "&amp;")}</root>");
                    ConvertParagraphNodeToTtmlNode(paragraphContent.DocumentElement, xml, paragraph);

                    if (i < lines.Count - 1)
                    {
                        XmlNode span = xml.CreateElement("span", "http://www.w3.org/ns/ttml");
                        XmlNode br = xml.CreateElement("br", "http://www.w3.org/ns/ttml");
                        span.AppendChild(br);
                        paragraph.AppendChild(span);
                    }
                }
            }
            catch
            {
                text = Regex.Replace(text, "[<>]", "");
                XmlNode span = xml.CreateElement("span", "http://www.w3.org/ns/ttml");
                span.AppendChild(xml.CreateTextNode(text));
                paragraph.AppendChild(span);
            }

            div.AppendChild(paragraph);
            return div;
        }

        internal static void ConvertParagraphNodeToTtmlNode(XmlNode node, XmlDocument ttmlXml, XmlNode ttmlNode)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child is XmlText)
                {
                    XmlNode span = ttmlXml.CreateElement("span", "http://www.w3.org/ns/ttml");
                    span.AppendChild(ttmlXml.CreateTextNode(child.Value));
                    ttmlNode.AppendChild(span);
                }
                else if (child.Name == "br")
                {
                    XmlNode span = ttmlXml.CreateElement("span", "http://www.w3.org/ns/ttml");
                    XmlNode br = ttmlXml.CreateElement("br", "http://www.w3.org/ns/ttml");
                    span.AppendChild(br);
                    ttmlNode.AppendChild(span);
                }
                else if (child.Name == "i")
                {
                    XmlNode span = ttmlXml.CreateElement("span", "http://www.w3.org/ns/ttml");
                    XmlAttribute attr = ttmlXml.CreateAttribute("tts", "fontStyle", "http://www.w3.org/ns/ttml#styling");
                    attr.InnerText = "italic";
                    span.Attributes.Append(attr);
                    ttmlNode.AppendChild(span);
                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, span);
                }
                else if (child.Name == "b")
                {
                    XmlNode span = ttmlXml.CreateElement("span", "http://www.w3.org/ns/ttml");
                    XmlAttribute attr = ttmlXml.CreateAttribute("tts", "fontWeight", "http://www.w3.org/ns/ttml#styling");
                    attr.InnerText = "bold";
                    span.Attributes.Append(attr);
                    ttmlNode.AppendChild(span);
                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, span);
                }
                else if (child.Name == "u")
                {
                    XmlNode span = ttmlXml.CreateElement("span", "http://www.w3.org/ns/ttml");
                    XmlAttribute attr = ttmlXml.CreateAttribute("tts", "textDecoration", "http://www.w3.org/ns/ttml#styling");
                    attr.InnerText = "underline";
                    span.Attributes.Append(attr);
                    ttmlNode.AppendChild(span);
                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, span);
                }
                else
                {
                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, ttmlNode);
                }
            }
        }

        private static List<string> GetStyles()
        {
            return TimedText10.GetStylesFromHeader(GetXmlStructure());
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
            try
            {
                xml.LoadXml(sb.ToString().RemoveControlCharactersButWhiteSpace().Trim());
            }
            catch
            {
                xml.LoadXml(sb.ToString().Replace(" & ", " &amp; ").Replace("Q&A", "Q&amp;A").RemoveControlCharactersButWhiteSpace().Trim());
            }

            var frameRateAttr = xml.DocumentElement.Attributes["ttp:frameRate"];
            if (frameRateAttr != null)
            {
                if (double.TryParse(frameRateAttr.Value, out var fr))
                {
                    if (fr > 20 && fr < 100)
                    {
                        Configuration.Settings.General.CurrentFrameRate = fr;
                    }

                    var frameRateMultiplier = xml.DocumentElement.Attributes["ttp:frameRateMultiplier"];
                    if (frameRateMultiplier != null)
                    {
                        if ((frameRateMultiplier.InnerText == "999 1000" ||
                             frameRateMultiplier.InnerText == "1000 1001") && Math.Abs(fr - 30) < 0.01)
                        {
                            Configuration.Settings.General.CurrentFrameRate = 29.97;
                        }
                        else if ((frameRateMultiplier.InnerText == "999 1000" ||
                                  frameRateMultiplier.InnerText == "1000 1001") && Math.Abs(fr - 24) < 0.01)
                        {
                            Configuration.Settings.General.CurrentFrameRate = 23.976;
                        }
                        else
                        {
                            var arr = frameRateMultiplier.InnerText.Split();
                            if (arr.Length == 2 && Utilities.IsInteger(arr[0]) && Utilities.IsInteger(arr[1]) && int.Parse(arr[1]) > 0)
                            {
                                fr = double.Parse(arr[0]) / double.Parse(arr[1]);
                                if (fr > 20 && fr < 100)
                                {
                                    Configuration.Settings.General.CurrentFrameRate = fr;
                                }
                            }
                        }
                    }
                }
            }

            if (BatchSourceFrameRate.HasValue)
            {
                Configuration.Settings.General.CurrentFrameRate = BatchSourceFrameRate.Value;
            }

            Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource = null;
            subtitle.Header = sb.ToString();

            var namespaceManager = new XmlNamespaceManager(xml.NameTable);
            namespaceManager.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
            var body = xml.DocumentElement.SelectSingleNode("ttml:body", namespaceManager);

            foreach (XmlNode divNode in body.SelectNodes("ttml:div", namespaceManager))
            {
                TimedText10.ExtractTimeCodes(divNode, subtitle, out var begin, out var end);

                var pNode = divNode.SelectSingleNode("ttml:p", namespaceManager);
                if (pNode != null)
                {
                    var text = ReadParagraph(pNode, xml);
                    var p = new Paragraph(begin, end, text);
                    subtitle.Paragraphs.Add(p);
                }
            }

            subtitle.Renumber();
        }

        private static string ReadParagraph(XmlNode node, XmlDocument xml)
        {
            var pText = new StringBuilder();
            var styles = GetStyles();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Text)
                {
                    pText.Append(child.Value);
                }
                else if (child.Name == "br" || child.Name == "tt:br")
                {
                    pText.AppendLine();
                }
                else if (child.Name == "#significant-whitespace" || child.Name == "tt:#significant-whitespace")
                {
                    pText.Append(child.InnerText);
                }
                else if (child.Name == "span" || child.Name == "tt:span")
                {
                    var isItalic = false;
                    var isBold = false;
                    var isUnderlined = false;
                    string fontFamily = null;
                    string color = null;

                    if (child.Attributes["style"] != null)
                    {
                        var styleName = child.Attributes["style"].Value;

                        if (styles.Contains(styleName))
                        {
                            try
                            {
                                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                                nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                                XmlNode head = xml.DocumentElement.SelectSingleNode("ttml:head", nsmgr);
                                foreach (XmlNode styleNode in head.SelectNodes("//ttml:style", nsmgr))
                                {
                                    string currentStyle = null;
                                    if (styleNode.Attributes["xml:id"] != null)
                                    {
                                        currentStyle = styleNode.Attributes["xml:id"].Value;
                                    }
                                    else if (styleNode.Attributes["id"] != null)
                                    {
                                        currentStyle = styleNode.Attributes["id"].Value;
                                    }

                                    if (currentStyle == styleName)
                                    {
                                        if (styleNode.Attributes["tts:fontStyle"] != null && styleNode.Attributes["tts:fontStyle"].Value == "italic")
                                        {
                                            isItalic = true;
                                        }

                                        if (styleNode.Attributes["tts:fontWeight"] != null && styleNode.Attributes["tts:fontWeight"].Value == "bold")
                                        {
                                            isBold = true;
                                        }

                                        if (styleNode.Attributes["tts:textDecoration"] != null && styleNode.Attributes["tts:textDecoration"].Value == "underline")
                                        {
                                            isUnderlined = true;
                                        }

                                        if (styleNode.Attributes["tts:fontFamily"] != null)
                                        {
                                            fontFamily = styleNode.Attributes["tts:fontFamily"].Value;
                                        }

                                        if (styleNode.Attributes["tts:color"] != null)
                                        {
                                            color = styleNode.Attributes["tts:color"].Value;
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                System.Diagnostics.Debug.WriteLine(e);
                            }
                        }
                    }

                    if (child.Attributes["tts:fontStyle"] != null && child.Attributes["tts:fontStyle"].Value == "italic")
                    {
                        isItalic = true;
                    }

                    if (child.Attributes["tts:fontWeight"] != null && child.Attributes["tts:fontWeight"].Value == "bold")
                    {
                        isBold = true;
                    }

                    if (child.Attributes["tts:textDecoration"] != null && child.Attributes["tts:textDecoration"].Value == "underline")
                    {
                        isUnderlined = true;
                    }

                    if (child.Attributes["tts:fontFamily"] != null)
                    {
                        fontFamily = child.Attributes["tts:fontFamily"].Value;
                    }

                    if (child.Attributes["tts:color"] != null)
                    {
                        color = child.Attributes["tts:color"].Value;
                    }

                    if (isItalic)
                    {
                        pText.Append("<i>");
                    }

                    if (isBold)
                    {
                        pText.Append("<b>");
                    }

                    if (isUnderlined)
                    {
                        pText.Append("<u>");
                    }

                    if (!string.IsNullOrEmpty(fontFamily) || !string.IsNullOrEmpty(color))
                    {
                        pText.Append("<font");

                        if (!string.IsNullOrEmpty(fontFamily))
                        {
                            pText.Append($" face=\"{fontFamily}\"");
                        }

                        if (!string.IsNullOrEmpty(color))
                        {
                            pText.Append($" color=\"{color}\"");
                        }

                        pText.Append(">");
                    }

                    pText.Append(ReadParagraph(child, xml));

                    if (!string.IsNullOrEmpty(fontFamily) || !string.IsNullOrEmpty(color))
                    {
                        pText.Append("</font>");
                    }

                    if (isUnderlined)
                    {
                        pText.Append("</u>");
                    }

                    if (isBold)
                    {
                        pText.Append("</b>");
                    }

                    if (isItalic)
                    {
                        pText.Append("</i>");
                    }
                }
            }

            return pText.ToString().TrimEnd();
        }
    }
}
