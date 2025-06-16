using System.Text.Json.Serialization;

namespace HanumanInstitute.LibMpv;

/// <summary>
/// Select RTSP transport method (default: tcp). This selects the underlying network transport when playing rtsp://... URLs. The value lavf leaves the decision to libavformat.
/// </summary>
public enum RtspTransportMode
{
    Lavf,
    Udp,
    [JsonPropertyName("udp_multicast")]
    UdpMulticast,
    Tcp,
    Http
}