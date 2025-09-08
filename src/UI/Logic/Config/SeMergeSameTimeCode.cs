namespace Nikse.SubtitleEdit.Logic.Config;

public class SeMergeSameTimeCode
{
    public int MaxMillisecondsDifference { get; internal set; }
    public bool MergeDialog { get; internal set; }
    public bool AutoBreak { get; internal set; }

    public SeMergeSameTimeCode()
    {
        MaxMillisecondsDifference = 500;
        AutoBreak = true;
    }
}