using HanumanInstitute.LibMpv;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Media;

public class MpvReloader : IMpvReloader
{
    public bool SmpteMode { get; set; }
    public int VideoWidth { get; set; } = 1280;
    public int VideoHeight { get; set; } = 720;

    private Subtitle? _subtitlePrev;
    private string _mpvTextOld = string.Empty;
    private int _mpvSubOldHash = -1;
    private string? _mpvTextFileName;
    private int _retryCount = 3;
    private string? _mpvPreviewStyleHeader;

    public void RefreshMpv(MpvContext mpvContext, Subtitle subtitle, SubtitleFormat uiFormat)
    {
        if (subtitle == null)
        {
            return;
        }

        try
        {
            subtitle = new Subtitle(subtitle, false);
            if (Se.Settings.General.CurrentVideoOffsetInMs != 0)
            {
                subtitle.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(-Se.Settings.General.CurrentVideoOffsetInMs));
            }

            if (SmpteMode)
            {
                foreach (var paragraph in subtitle.Paragraphs)
                {
                    paragraph.StartTime.TotalMilliseconds *= 1.001;
                    paragraph.EndTime.TotalMilliseconds *= 1.001;
                }
            }

            SubtitleFormat format = new AdvancedSubStationAlpha();
            string text;

            var uiFormatType = uiFormat.GetType();
            //if (uiFormatType == typeof(NetflixImsc11Japanese))
            //{
            //    text = NetflixImsc11JapaneseToAss.Convert(subtitle, VideoWidth, VideoHeight);
            //}
            //else
            if (uiFormatType == typeof(WebVTT) || uiFormatType == typeof(WebVTTFileWithLineNumber))
            {
                //TODO: add some caching!?
                var defaultStyle = GetMpvPreviewStyle(Configuration.Settings.General);
                defaultStyle.BorderStyle = "3";
                subtitle = new Subtitle(subtitle);
                subtitle = WebVttToAssa.Convert(subtitle, defaultStyle, VideoWidth, VideoHeight);
                format = new AdvancedSubStationAlpha();
                text = subtitle.ToText(format);
                //    File.WriteAllText(@"c:\data\__a.ass", text);
            }
            else
            {
                if (subtitle.Header == null || !subtitle.Header.Contains("[V4+ Styles]") || uiFormatType != typeof(AdvancedSubStationAlpha))
                {
                    if (string.IsNullOrEmpty(subtitle.Header) && uiFormatType == typeof(SubStationAlpha))
                    {
                        subtitle.Header = SubStationAlpha.DefaultHeader;
                    }

                    if (subtitle.Header != null && subtitle.Header.Contains("[V4 Styles]"))
                    {
                        subtitle.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromSubStationAlpha(subtitle.Header);
                    }

                    var oldSub = subtitle;
                    subtitle = new Subtitle(subtitle);
                    if (Se.Settings.Appearance.RightToLeft)
                    {
                        for (var index = 0; index < subtitle.Paragraphs.Count; index++)
                        {
                            var paragraph = subtitle.Paragraphs[index];
                            if (LanguageAutoDetect.ContainsRightToLeftLetter(paragraph.Text))
                            {
                                paragraph.Text = Utilities.FixRtlViaUnicodeChars(paragraph.Text);
                            }
                        }
                    }

                    if (subtitle.Header == null || !(subtitle.Header.Contains("[V4+ Styles]") && uiFormatType == typeof(SubStationAlpha)))
                    {
                        subtitle.Header = MpvPreviewStyleHeader;
                    }

                    if (oldSub.Header != null && oldSub.Header.Length > 20 && oldSub.Header.Substring(3, 3) == "STL")
                    {
                        subtitle.Header = subtitle.Header.Replace("Style: Default,", "Style: Box," +
                            Configuration.Settings.General.VideoPlayerPreviewFontName + "," +
                            Configuration.Settings.General.VideoPlayerPreviewFontSize + ",&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000," +
                            (Configuration.Settings.General.VideoPlayerPreviewFontBold ? "-1" : "0") + ",0,0,0,100,100,0,0,3,2,0,2,10,10,10,1" +
                                                                   Environment.NewLine + "Style: Default,");

                        var useBox = false;
                        if (Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox)
                        {
                            try
                            {
                                var encoding = Ebu.GetEncoding(oldSub.Header.Substring(0, 3));
                                var buffer = encoding.GetBytes(oldSub.Header);
                                var header = Ebu.ReadHeader(buffer);
                                if (header.DisplayStandardCode != "0")
                                {
                                    useBox = true;
                                }
                            }
                            catch
                            {
                                // ignore
                            }
                        }

                        for (var index = 0; index < subtitle.Paragraphs.Count; index++)
                        {
                            var p = subtitle.Paragraphs[index];

                            p.Extra = useBox ? "Box" : "Default";

                            if (p.Text.Contains("<box>"))
                            {
                                p.Extra = "Box";
                                p.Text = p.Text.Replace("<box>", string.Empty).Replace("</box>", string.Empty);
                            }
                        }
                    }
                }

                var hash = subtitle.GetFastHashCode(null);
                if (hash != _mpvSubOldHash || string.IsNullOrEmpty(_mpvTextOld))
                {
                    text = subtitle.ToText(new AdvancedSubStationAlpha());
                    _mpvSubOldHash = hash;
                }
                else
                {
                    text = _mpvTextOld;
                }
            }


            if (text != _mpvTextOld || _mpvTextFileName == null || _retryCount > 0)
            {
                if (_retryCount >= 0 || string.IsNullOrEmpty(_mpvTextFileName) || _subtitlePrev == null || _subtitlePrev.FileName != subtitle.FileName || !_mpvTextFileName.EndsWith(format.Extension, StringComparison.Ordinal))
                {
                    mpvContext.SubRemove().Invoke();
                    DeleteTempMpvFileName();
                    _mpvTextFileName = FileUtil.GetTempFileName(format.Extension);
                    File.WriteAllText(_mpvTextFileName, text);
                    mpvContext.SubAdd(_mpvTextFileName).Invoke();
                    mpvContext.SetOptionString("sid", "auto");
                    _retryCount--;
                }
                else
                {
                    mpvContext.SubRemove().Invoke();
                    File.WriteAllText(_mpvTextFileName, text);
                    mpvContext.SubAdd(_mpvTextFileName).Invoke();
                }
                _mpvTextOld = text;
            }
            _subtitlePrev = subtitle;
        }
        catch (Exception exception)
        {
            Se.LogError(exception);
        }
    }

    private string MpvPreviewStyleHeader
    {
        get
        {
            if (_mpvPreviewStyleHeader is null)
            {
                UpdateMpvStyle();
            }

            return _mpvPreviewStyleHeader ?? string.Empty;
        }
        set => _mpvPreviewStyleHeader = value;
    }

    public void UpdateMpvStyle()
    {
        var gs = Configuration.Settings.General;
        var mpvStyle = GetMpvPreviewStyle(gs);

        MpvPreviewStyleHeader = string.Format(AdvancedSubStationAlpha.HeaderNoStyles, "MPV preview file", mpvStyle.ToRawAss(SsaStyle.DefaultAssStyleFormat));
    }

    private static SsaStyle GetMpvPreviewStyle(GeneralSettings gs)
    {
        return new SsaStyle
        {
            Name = "Default",
            FontName = gs.VideoPlayerPreviewFontName,
            FontSize = gs.VideoPlayerPreviewFontSize,
            Bold = gs.VideoPlayerPreviewFontBold,
            Primary = gs.MpvPreviewTextPrimaryColor,
            Outline = gs.MpvPreviewTextOutlineColor,
            Background = gs.MpvPreviewTextBackgroundColor,
            OutlineWidth = gs.MpvPreviewTextOutlineWidth,
            ShadowWidth = gs.MpvPreviewTextShadowWidth,
            BorderStyle = gs.MpvPreviewTextOpaqueBoxStyle,
            Alignment = gs.MpvPreviewTextAlignment,
            MarginVertical = gs.MpvPreviewTextMarginVertical
        };
    }

    private void DeleteTempMpvFileName()
    {
        try
        {
            if (File.Exists(_mpvTextFileName))
            {
                File.Delete(_mpvTextFileName);
                _mpvTextFileName = null;
            }
        }
        catch
        {
            // ignored
        }
    }

    public void Reset()
    {
        _mpvTextFileName = null;
        _mpvTextOld = string.Empty;
        _retryCount = 3;
    }
}
