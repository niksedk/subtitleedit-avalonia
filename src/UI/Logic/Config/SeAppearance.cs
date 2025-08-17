namespace Nikse.SubtitleEdit.Logic.Config;

public class SeAppearance
{
    public string Theme { get; set; }

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
    public double SubtitleTextBoxFontSize { get; set; }
    public bool SubtitleTextBoxFontBold { get; set; }
    public bool ToolbarShowHints { get; set; }

    public SeAppearance()
    {
        Theme = "System";

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
        SubtitleTextBoxFontSize = 15d;
        SubtitleTextBoxFontBold = true;
    }
}