using Nikse.SubtitleEdit.Core.ContainerFormats.Ebml;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Matroska
{
    public sealed class MatroskaFile : IDisposable
    {
        public delegate void LoadMatroskaCallback(long position, long total);

        private readonly Stream _stream;
        private int _pixelWidth, _pixelHeight;
        private double _frameRate;
        private string _videoCodecId;

        private int _subtitleRipTrackNumber;
        private readonly List<MatroskaSubtitle> _subtitleRip = new List<MatroskaSubtitle>();
        private List<MatroskaTrackInfo> _tracks = new List<MatroskaTrackInfo>();
        private readonly List<MatroskaChapter> _chapters = new List<MatroskaChapter>();

        private readonly Element _segmentElement;
        private long _timeCodeScale = 1000000;
        private double _duration;

        private const int SmallSeekThreshold = 256;
        private static readonly byte[] _skipBuffer = new byte[SmallSeekThreshold];

        private static readonly byte[] VintLengthTable = CreateVintTable();

        private static byte[] CreateVintTable()
        {
            var table = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                if (i >= 128) table[i] = 1;
                else if (i >= 64) table[i] = 2;
                else if (i >= 32) table[i] = 3;
                else if (i >= 16) table[i] = 4;
                else if (i >= 8) table[i] = 5;
                else if (i >= 4) table[i] = 6;
                else if (i >= 2) table[i] = 7;
                else if (i == 1) table[i] = 8;
                else table[i] = 0;
            }
            return table;
        }

        public bool IsValid { get; }
        public string Path { get; }

        public MatroskaFile(string path)
        {
            Path = path;
            _stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536, FileOptions.SequentialScan);

            var headerElement = ReadElement();
            if (headerElement != null && headerElement.Id == ElementId.Ebml)
            {
                SkipBytes(headerElement.DataSize);
                _segmentElement = ReadElement();
                if (_segmentElement != null && _segmentElement.Id == ElementId.Segment)
                {
                    IsValid = true;
                }
            }
        }

        public List<MatroskaTrackInfo> GetTracks(bool subtitleOnly = false)
        {
            ReadSegmentInfoAndTracks();
            if (_tracks == null) return new List<MatroskaTrackInfo>();
            return subtitleOnly ? _tracks.Where(t => t.IsSubtitle).ToList() : _tracks;
        }

        public void GetInfo(out double fr, out int pw, out int ph, out double dur, out string vc)
        {
            ReadSegmentInfoAndTracks();
            fr = _frameRate;
            pw = _pixelWidth;
            ph = _pixelHeight;
            dur = _duration;
            vc = _videoCodecId;
        }

        public List<MatroskaSubtitle> GetSubtitle(int trackNumber, LoadMatroskaCallback progressCallback)
        {
            _subtitleRip.Clear();
            _subtitleRipTrackNumber = trackNumber;
            ReadSegmentCluster(progressCallback);

            // Handle decompression if the track info indicates zlib
            var track = _tracks.FirstOrDefault(t => t.TrackNumber == trackNumber);
            if (track != null && track.ContentCompressionAlgorithm == 0) // 0 = zlib/deflate
            {
                DecompressSubtitles();
            }

            return _subtitleRip;
        }

        private void DecompressSubtitles()
        {
            for (int i = 0; i < _subtitleRip.Count; i++)
            {
                var sub = _subtitleRip[i];
                if (sub.Data == null || sub.Data.Length < 2) continue;

                try
                {
                    // EBML/Matroska zlib often includes the 2-byte header (0x78 0x9C)
                    // DeflateStream needs raw data, so we skip the header.
                    using var ms = new MemoryStream(sub.Data, 2, sub.Data.Length - 2);
                    using var ds = new DeflateStream(ms, CompressionMode.Decompress);
                    using var outStream = new MemoryStream();
                    ds.CopyTo(outStream);
                    sub.Data = outStream.ToArray();
                }
                catch
                {
                    // If decompression fails, we keep the original data as fallback
                }
            }
        }

        public long GetAudioTrackDelayMilliseconds(int audioTrackNumber)
        {
            var tracks = GetTracks();
            var videoTrack = tracks.Find(p => p.IsVideo && p.IsDefault) ?? tracks.Find(p => p.IsVideo);
            long videoDelay = videoTrack != null ? GetTrackStartTime(videoTrack.TrackNumber) : 0;
            return GetTrackStartTime(audioTrackNumber) - videoDelay;
        }

        public long GetTrackStartTime(int trackNumber)
        {
            if (_segmentElement == null) return 0;
            _stream.Seek(_segmentElement.DataPosition, SeekOrigin.Begin);
            const int maxClustersToSeek = 100;
            var clusterNo = 0;

            Element element;
            while (_stream.Position < _stream.Length && clusterNo < maxClustersToSeek && (element = ReadElement()) != null)
            {
                switch (element.Id)
                {
                    case ElementId.Info: ReadInfoElement(element); break;
                    case ElementId.Tracks: ReadTracksElement(element); break;
                    case ElementId.Cluster:
                        clusterNo++;
                        var startTime = FindTrackStartInCluster(element, trackNumber, out var found);
                        if (found) return startTime;
                        break;
                }
                _stream.Seek(element.EndPosition, SeekOrigin.Begin);
            }
            return 0;
        }

        public List<MatroskaChapter> GetChapters()
        {
            ReadChapters();
            return _chapters.Distinct().ToList();
        }

        public void Dispose() => _stream?.Dispose();

        private void SkipBytes(long count)
        {
            if (count <= 0) return;
            if (count <= SmallSeekThreshold)
            {
                while (count > 0)
                {
                    var read = _stream.Read(_skipBuffer, 0, (int)Math.Min(count, _skipBuffer.Length));
                    if (read <= 0) break;
                    count -= read;
                }
            }
            else
            {
                _stream.Seek(count, SeekOrigin.Current);
            }
        }

        private long FindTrackStartInCluster(Element cluster, int targetTrackNumber, out bool found)
        {
            found = false;
            var clusterTimeCode = 0L;
            while (_stream.Position < cluster.EndPosition)
            {
                var element = ReadElement();
                if (element == null || element.Id == ElementId.None) break;

                switch (element.Id)
                {
                    case ElementId.Timecode: clusterTimeCode = (long)ReadUInt((int)element.DataSize); break;
                    case ElementId.BlockGroup: ReadBlockGroupElement(element, clusterTimeCode); break;
                    case ElementId.SimpleBlock:
                        if (ReadVariableLengthInt() == targetTrackNumber)
                        {
                            var trackStartTime = ReadInt16();
                            found = true;
                            return (long)Math.Round(GetTimeScaledToMilliseconds(clusterTimeCode + trackStartTime));
                        }
                        break;
                }
                _stream.Seek(element.EndPosition, SeekOrigin.Begin);
            }
            return 0;
        }

        private void ReadSegmentCluster(LoadMatroskaCallback progressCallback)
        {
            _stream.Seek(_segmentElement.DataPosition, SeekOrigin.Begin);
            while (_stream.Position < _segmentElement.EndPosition)
            {
                var pos = _stream.Position;
                var id = (ElementId)ReadVariableLengthUInt(false);
                if (id == ElementId.None && pos + 1000 < _stream.Length)
                {
                    int errors = 0;
                    while (id != ElementId.Cluster && pos + 1000 < _stream.Length && errors++ < 5_000_000)
                    {
                        _stream.Seek(++pos, SeekOrigin.Begin);
                        id = (ElementId)ReadVariableLengthUInt(false);
                    }
                }

                var size = (long)ReadVariableLengthUInt();
                var element = new Element(id, _stream.Position, size);

                if (element.Id == ElementId.Cluster) ReadCluster(element);
                else SkipBytes(element.DataSize);

                progressCallback?.Invoke(element.EndPosition, _stream.Length);
            }
        }

        private void ReadCluster(Element clusterElement)
        {
            long clusterTimeCode = 0;
            Element element;
            while (_stream.Position < clusterElement.EndPosition && (element = ReadElement()) != null)
            {
                switch (element.Id)
                {
                    case ElementId.Timecode: clusterTimeCode = (long)ReadUInt((int)element.DataSize); break;
                    case ElementId.BlockGroup: ReadBlockGroupElement(element, clusterTimeCode); break;
                    case ElementId.SimpleBlock:
                        var sub = ReadSubtitleBlock(element, clusterTimeCode);
                        if (sub != null) _subtitleRip.Add(sub);
                        break;
                    default: SkipBytes(element.DataSize); break;
                }
            }
        }

        private MatroskaSubtitle ReadSubtitleBlock(Element blockElement, long clusterTimeCode)
        {
            if (ReadVariableLengthInt() != _subtitleRipTrackNumber)
            {
                _stream.Seek(blockElement.EndPosition, SeekOrigin.Begin);
                return null;
            }

            var timeCode = ReadInt16();
            var flags = (byte)_stream.ReadByte();
            int lacing = (flags >> 1) & 3;
            if (lacing > 0)
            {
                int frames = _stream.ReadByte() + 1;
                if (lacing == 2) SkipBytes(frames);
            }

            var data = new byte[(int)(blockElement.EndPosition - _stream.Position)];
            _stream.Read(data, 0, data.Length);
            return new MatroskaSubtitle(data, (long)Math.Round(GetTimeScaledToMilliseconds(clusterTimeCode + timeCode)));
        }

        private void ReadBlockGroupElement(Element clusterElement, long clusterTimeCode)
        {
            MatroskaSubtitle subtitle = null;
            Element element;
            while (_stream.Position < clusterElement.EndPosition && (element = ReadElement()) != null)
            {
                switch (element.Id)
                {
                    case ElementId.Block:
                        subtitle = ReadSubtitleBlock(element, clusterTimeCode);
                        if (subtitle == null) return;
                        _subtitleRip.Add(subtitle);
                        break;
                    case ElementId.BlockDuration:
                        if (subtitle != null) subtitle.Duration = (long)Math.Round(GetTimeScaledToMilliseconds((long)ReadUInt((int)element.DataSize)));
                        break;
                    default: SkipBytes(element.DataSize); break;
                }
            }
        }

        private void ReadTracksElement(Element tracksElement)
        {
            _tracks = new List<MatroskaTrackInfo>();
            Element element;
            while (_stream.Position < tracksElement.EndPosition && (element = ReadElement()) != null)
            {
                if (element.Id == ElementId.TrackEntry) ReadTrackEntryElement(element);
                else SkipBytes(element.DataSize);
            }
        }

        private void ReadTrackEntryElement(Element trackEntryElement)
        {
            var info = new MatroskaTrackInfo { Language = "eng", IsDefault = true };
            long defaultDuration = 0;

            Element element;
            while (_stream.Position < trackEntryElement.EndPosition && (element = ReadElement()) != null)
            {
                switch (element.Id)
                {
                    case ElementId.DefaultDuration: defaultDuration = (long)ReadUInt((int)element.DataSize); break;
                    case ElementId.Video: ReadVideoElement(element); info.IsVideo = true; break;
                    case ElementId.Audio: info.IsAudio = true; break;
                    case ElementId.TrackNumber: info.TrackNumber = (int)ReadUInt((int)element.DataSize); break;
                    case ElementId.Name: info.Name = ReadString((int)element.DataSize, Encoding.UTF8); break;
                    case ElementId.Language: info.Language = ReadString((int)element.DataSize, Encoding.ASCII); break;
                    case ElementId.CodecId: info.CodecId = ReadString((int)element.DataSize, Encoding.ASCII); break;
                    case ElementId.TrackType:
                        int type = _stream.ReadByte();
                        if (type == 1) info.IsVideo = true; else if (type == 2) info.IsAudio = true; else if (type == 17) info.IsSubtitle = true;
                        break;
                    case ElementId.CodecPrivate:
                        info.CodecPrivateRaw = new byte[element.DataSize]; _stream.Read(info.CodecPrivateRaw, 0, info.CodecPrivateRaw.Length);
                        break;
                    case ElementId.ContentEncodings:
                        info.ContentCompressionAlgorithm = 0; info.ContentEncodingType = 0;
                        var enc = ReadElement();
                        if (enc?.Id == ElementId.ContentEncoding) ReadContentEncodingElement(element, info);
                        break;
                    case ElementId.FlagDefault: info.IsDefault = ReadUInt((int)element.DataSize) == 1; break;
                    case ElementId.FlagForced: info.IsForced = ReadUInt((int)element.DataSize) == 1; break;
                }
                _stream.Seek(element.EndPosition, SeekOrigin.Begin);
            }

            _tracks.Add(info);
            if (info.IsVideo)
            {
                if (defaultDuration > 0) _frameRate = 1.0 / (defaultDuration / 1_000_000_000.0);
                _videoCodecId = info.CodecId;
            }
        }

        private void ReadVideoElement(Element videoElement)
        {
            Element element;
            while (_stream.Position < videoElement.EndPosition && (element = ReadElement()) != null)
            {
                if (element.Id == ElementId.PixelWidth) _pixelWidth = (int)ReadUInt((int)element.DataSize);
                else if (element.Id == ElementId.PixelHeight) _pixelHeight = (int)ReadUInt((int)element.DataSize);
                else SkipBytes(element.DataSize);
            }
        }

        private void ReadContentEncodingElement(Element element, MatroskaTrackInfo info)
        {
            Element sub;
            while (_stream.Position < element.EndPosition && (sub = ReadElement()) != null)
            {
                switch (sub.Id)
                {
                    case ElementId.ContentEncodingScope: info.ContentEncodingScope = (uint)ReadUInt((int)sub.DataSize); break;
                    case ElementId.ContentEncodingType: info.ContentEncodingType = (int)ReadUInt((int)sub.DataSize); break;
                    case ElementId.ContentCompression:
                        Element comp;
                        while (_stream.Position < sub.EndPosition && (comp = ReadElement()) != null)
                        {
                            if (comp.Id == ElementId.ContentCompAlgo) info.ContentCompressionAlgorithm = (int)ReadUInt((int)comp.DataSize);
                            else SkipBytes(comp.DataSize);
                        }
                        break;
                    default: SkipBytes(sub.DataSize); break;
                }
            }
        }

        private void ReadInfoElement(Element infoElement)
        {
            Element element;
            while (_stream.Position < infoElement.EndPosition && (element = ReadElement()) != null)
            {
                if (element.Id == ElementId.TimecodeScale) _timeCodeScale = (long)ReadUInt((int)element.DataSize);
                else if (element.Id == ElementId.Duration)
                {
                    _duration = GetTimeScaledToMilliseconds(element.DataSize == 4 ? ReadFloat32() : ReadFloat64());
                }
                else SkipBytes(element.DataSize);
            }
        }

        private ulong ReadVariableLengthUInt(bool unsetFirstBit = true)
        {
            int first = _stream.ReadByte();
            if (first < 0) return 0;

            int length = VintLengthTable[first];
            if (length == 0) return 0;

            ulong result = unsetFirstBit ? (ulong)(first & (0xFF >> length)) : (ulong)first;

            // Check for EBML unknown length (all bits 1 except marker)
            bool isUnknown = unsetFirstBit && (first == (0xFF >> (8 - length)));

            for (int i = 1; i < length; i++)
            {
                int b = _stream.ReadByte();
                if (b < 0) break;

                // Fix for CS0675: Cast to byte first to avoid sign extension
                result = (result << 8) | (ulong)(byte)b;

                if (isUnknown && b != 0xFF) isUnknown = false;
            }

            // In EBML, if all data bits are 1, it's an "unknown length" (max ulong for us)
            return isUnknown ? ulong.MaxValue : result;
        }

        private int ReadVariableLengthInt(bool unsetFirstBit = true) => (int)ReadVariableLengthUInt(unsetFirstBit);

        private ulong ReadUInt(int length)
        {
            if (length <= 0 || length > 8) return 0;
            Span<byte> buffer = stackalloc byte[length];
            if (_stream.Read(buffer) < length) return 0;
            ulong result = 0;
            for (int i = 0; i < length; i++) result = (result << 8) | buffer[i];
            return result;
        }

        private short ReadInt16()
        {
            Span<byte> buffer = stackalloc byte[2];
            return _stream.Read(buffer) < 2 ? (short)0 : BinaryPrimitives.ReadInt16BigEndian(buffer);
        }

        private float ReadFloat32()
        {
            Span<byte> buffer = stackalloc byte[4];
            if (_stream.Read(buffer) < 4) return 0f;
            return BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(buffer));
        }

        private double ReadFloat64()
        {
            Span<byte> buffer = stackalloc byte[8];
            if (_stream.Read(buffer) < 8) return 0d;
            return BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(buffer));
        }

        private Element ReadElement()
        {
            var id = (ElementId)ReadVariableLengthUInt(false);
            if (id == ElementId.None) return null;
            var size = (long)ReadVariableLengthUInt();
            return new Element(id, _stream.Position, size);
        }

        private string ReadString(int length, Encoding encoding)
        {
            if (length <= 0) return string.Empty;
            if (length <= 256)
            {
                Span<byte> buffer = stackalloc byte[length];
                _stream.Read(buffer);
                return encoding.GetString(buffer).TrimEnd('\0');
            }
            var largeBuffer = new byte[length];
            _stream.Read(largeBuffer, 0, length);
            return encoding.GetString(largeBuffer).TrimEnd('\0');
        }

        private void ReadSegmentInfoAndTracks()
        {
            if (_segmentElement == null) return;
            _stream.Seek(_segmentElement.DataPosition, SeekOrigin.Begin);
            Element element;
            while (_stream.Position < _segmentElement.EndPosition && (element = ReadElement()) != null)
            {
                if (element.Id == ElementId.Info) ReadInfoElement(element);
                else if (element.Id == ElementId.Tracks) { ReadTracksElement(element); break; }
                else SkipBytes(element.DataSize);
            }
        }

        private double GetTimeScaledToMilliseconds(double time) => time * _timeCodeScale / 1_000_000.0;

        private void ReadChapters()
        {
            if (_segmentElement == null) return;
            _stream.Seek(_segmentElement.DataPosition, SeekOrigin.Begin);
            Element element;
            while (_stream.Position < _segmentElement.EndPosition && (element = ReadElement()) != null)
            {
                if (element.Id == ElementId.Chapters) ReadChaptersElement(element);
                else SkipBytes(element.DataSize);
            }
        }

        private void ReadChaptersElement(Element chaptersElement)
        {
            Element element;
            while (_stream.Position < chaptersElement.EndPosition && (element = ReadElement()) != null)
            {
                SkipBytes(element.DataSize);
            }
        }
    }
}