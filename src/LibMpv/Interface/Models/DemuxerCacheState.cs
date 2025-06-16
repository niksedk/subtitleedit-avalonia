namespace HanumanInstitute.LibMpv;

/// <summary>
/// Model to deserialize demuxer-cache-state property.
/// </summary>
public class DemuxerCacheState
{
    // {"cache-end":0.920000,"reader-pts":-0.080000,"cache-duration":1.000000,"eof":false,"underrun":false,"idle":true,
    // "total-bytes":30256,"fw-bytes":30256,"raw-input-rate":6571,"debug-low-level-seeks":0,"debug-byte-level-seeks":2,
    // "debug-ts-last":0.920000,"bof-cached":false,"eof-cached":false,"seekable-ranges":[]}

    /// <summary>
    /// Returns demuxer-cache-time. Missing if unavailable.
    /// </summary>
    public double? CacheEnd { get; set; }
    /// <summary>
    /// The approximate timestamp of the start of the buffered range. Missing if unavailable.
    /// </summary>
    public double? ReaderPts { get; set; }
    /// <summary>
    /// Returns demuxer-cache-duration. Missing if unavailable.
    /// </summary>
    public double? CacheDuration { get; set; }
    /// <summary>
    /// The number of bytes of packets buffered in the range starting from the current decoding position. This is a rough estimate (may not account correctly for various overhead), and stops at the demuxer position (it ignores seek ranges after it).
    /// </summary>
    public long FwBytes { get; set; }
    /// <summary>
    /// The estimated input rate of the network layer (or any other byte-oriented input layer) in bytes per second. May be inaccurate or missing.
    /// </summary>
    public long? RawInputRate { get; set; }
    public int DebugLowLevelSeeks { get; set; }
    public int DebugByteLevelSeeks { get; set; }
    public double DebugTsLast { get; set; }
    /// <summary>
    /// Indicates whether the seek range with the lowest timestamp points to the beginning of the stream (BOF). This implies you cannot seek before this position at all. eof-cached indicates whether the seek range with the highest timestamp points to the end of the stream (EOF). If both bof-cached and eof-cached are true, and there's only 1 cache range, the entire stream is cached.
    /// </summary>
    public bool BofCached { get; set; }
    public bool EndCached { get; set; }
    /// <summary>
    /// Each entry represents a region in the demuxer cache that can be seeked to, with a start and end fields containing the respective timestamps. If there are multiple demuxers active, this only returns information about the "main" demuxer, but might be changed in future to return unified information about all demuxers. The ranges are in arbitrary order. Often, ranges will overlap for a bit, before being joined. In broken corner cases, ranges may overlap all over the place.
    /// </summary>
    public IList<DemuxerCacheRange> SeekableRanges { get; } = new List<DemuxerCacheRange>();
    /// <summary>
    /// The number of bytes stored in the file cache. This includes all overhead, and possibly unused data (like pruned data). This member is missing if the file cache wasn't enabled with --cache-on-disk=yes.
    /// </summary>
    public int? FileCacheBytes { get; set; }

    /// <summary>
    /// Whether the reader thread has hit the end of the file.
    /// </summary>
    public bool Eof { get; set; }
    /// <summary>
    /// Whether the reader thread could not satisfy a decoder's request for a new packet.
    /// </summary>
    public bool Underrun { get; set; }
    /// <summary>
    /// Whether the thread is currently not reading.
    /// </summary>
    public bool Idle { get; set; }
    /// <summary>
    /// Sum of packet bytes (plus some overhead estimation) of the entire packet queue, including cached seekable ranges.
    /// </summary>
    public long TotalBytes { get; set; }
}

public class DemuxerCacheRange
{
    public double Start { get; set; }
    public double End { get; set; }
}