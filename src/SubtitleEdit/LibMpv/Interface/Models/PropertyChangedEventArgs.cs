namespace HanumanInstitute.LibMpv;

public class PropertyChangedEventArgs : EventArgs
{
    public int Id { get; set; }
    public string Data { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}