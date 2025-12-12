using Avalonia.Controls;
using Avalonia.Media;
using static Nikse.SubtitleEdit.Features.Main.Layout.InitLayout;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeAppearance
{
    public string Theme { get; set; }
    public string FontName { get; set; }
    public double SubtitleGridFontSize { get; set; }
    public bool SubtitleGridTextSingleLine { get; set; }
    public string SubtitleGridTextSingleLineSeparator { get; set; }
    public double SubtitleTextBoxFontSize { get; set; }
    public string SubtitleTextBoxAndGridFontName { get; set; }
    public bool SubtitleTextBoxFontBold { get; set; }
    public bool SubtitleTextBoxColorTags { get; set; }
    public bool SubtitleGridColorTags { get; set; }

    public bool SubtitleTextBoxCenterText { get; set; }
    public bool ShowHints { get; set; }
    public bool GridCompactMode { get; set; }
    public string BookmarkColor { get; set; }
    public string DarkModeBackgroundColor { get; set; }
    public string GridLinesAppearance { get; set; }
    public bool ShowHorizontalLineAboveToolbar { get; set; }
    public bool ShowHorizontalLineBelowToolbar { get; set; }

    public bool ToolbarShowFileNew { get; set; }
    public bool ToolbarShowFileOpen { get; set; }
    public bool ToolbarShowVideoFileOpen { get; set; }
    public bool ToolbarShowSave { get; set; }
    public bool ToolbarShowSaveAs { get; set; }
    public bool ToolbarShowFind { get; set; }
    public bool ToolbarShowReplace { get; set; }
    public bool ToolbarShowFixCommonErrors { get; set; }
    public bool ToolbarShowSpellCheck { get; set; }
    public bool ToolbarShowSettings { get; set; }
    public bool ToolbarShowLayout { get; set; }
    public bool ToolbarShowHelp { get; set; }
    public bool ToolbarShowEncoding { get; set; }
    public bool RightToLeft { get; set; }
    public bool ShowLayer { get; set; }
    public bool ShowUpDownStartTime { get; set; }
    public bool ShowUpDownEndTime { get; set; }
    public bool ShowUpDownDuration { get; set; }
    public bool ShowUpDownLabels { get; set; }

    public LayoutPositions CurrentLayoutPositions { get; set; }

    public SeAppearance()
    {
        CurrentLayoutPositions = new LayoutPositions();
        Theme = "System";
        FontName = "Default";
        SubtitleTextBoxAndGridFontName = "Default";
        SubtitleGridFontSize = 13d;
        SubtitleGridTextSingleLineSeparator = "<br />";
        SubtitleTextBoxFontSize = 15d;
        SubtitleTextBoxFontBold = true;
        ShowHints = true;
        SubtitleTextBoxCenterText = false;
        GridLinesAppearance = DataGridGridLinesVisibility.None.ToString();
        DarkModeBackgroundColor = new Color(255, 33, 33, 33).FromColorToHex();
        BookmarkColor = Colors.Gold.FromColorToHex();
        GridCompactMode = true;
        ShowLayer = true;
        ShowUpDownStartTime = true;
        ShowUpDownEndTime = false;
        ShowUpDownDuration = true;
        ShowUpDownLabels = true;

        ToolbarShowFileNew = true;
        ToolbarShowFileOpen = true;
        ToolbarShowVideoFileOpen = true;
        ToolbarShowSave = true;
        ToolbarShowSaveAs = false;
        ToolbarShowFind = true;
        ToolbarShowReplace = true;
        ToolbarShowFixCommonErrors = true;
        ToolbarShowSpellCheck = true;
        ToolbarShowSettings = true;
        ToolbarShowLayout = true;
        ToolbarShowHelp = true;
        ToolbarShowEncoding = false;
    }
}