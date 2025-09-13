namespace Nikse.SubtitleEdit.Features.Video.ShotChanges;

public class TimeItem
{
    public double Time { get; set; }
    public string TimeText { get; set; }
    public int Number { get; set; }

    public TimeItem(double time, int number)
    {
        Time = time;
        TimeText = time.ToString("#,###,000.000");
        Number = number;
    }

    public override string ToString() => TimeText;
}
