using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper.Engines;

namespace Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper;

public class WhisperModelDisplay
{
    public WhisperModel Model { get; set; } = new WhisperModel();
    public string? Display { get; set; }
    public IWhisperEngine Engine { get; set; } = new WhisperEngineCpp();

    public override string ToString()
    {
        if (Display == null)
        {
            RefreshDownloadStatus(); ;
        }

        return Display!;
    }

    private string IsInstalled()
    {
        if (!Engine.IsModelInstalled(Model))
        {
            return ", not installed";
        }

        return string.Empty;
    }

    public void RefreshDownloadStatus()
    {
        Display = Model.Name;

        if (!string.IsNullOrEmpty(Model.Size))
        {
            Display += $" ({Model.Size}{IsInstalled()})";
        }
        else
        {
            Display += $" ({IsInstalled().TrimStart(',').TrimStart()})";
        }
    }
}
