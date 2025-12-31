using Nikse.SubtitleEdit.Core.Common;
using System;
using System.IO;
using System.IO.Compression;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Matroska
{
    public class MatroskaSubtitle
    {
        internal byte[] Data { get; set; }
        public long Start { get; set; }
        public long Duration { get; set; }

        public MatroskaSubtitle(byte[] data, long start, long duration)
        {
            Data = data;
            Start = start;
            Duration = duration;
        }

        /// <summary>
        /// Get data, if contentEncodingType == 0, then data is compressed with zlib
        /// </summary>
        /// <param name="matroskaTrackInfo"></param>
        /// <returns>Data byte array (uncompressed)</returns>
        public byte[] GetData(MatroskaTrackInfo matroskaTrackInfo)
        {
            // Check if compression is used and if it applies to the frame data (Scope)
            if (matroskaTrackInfo.ContentEncodingType != 0 || // 0 = Compression
                (matroskaTrackInfo.ContentEncodingScope & 1) == 0) // 1 = All frame data
            {
                return Data;
            }

            // Ensure we have enough data to check for zlib header
            if (Data == null || Data.Length < 2)
            {
                return Data;
            }

            try
            {
                // Matroska zlib blocks usually start with 0x78 0x9C (zlib header).
                // DeflateStream needs raw data, so we skip the first 2 bytes.
                int headerOffset = (Data[0] == 0x78 && (Data[1] == 0x9C || Data[1] == 0x01 || Data[1] == 0xDA)) ? 2 : 0;

                using var inStream = new MemoryStream(Data, headerOffset, Data.Length - headerOffset);
                using var outStream = new MemoryStream();
                using (var deflateStream = new DeflateStream(inStream, CompressionMode.Decompress))
                {
                    deflateStream.CopyTo(outStream);
                }
                return outStream.ToArray();
            }
            catch
            {
                // If decompression fails, return raw data as a fallback
                return Data;
            }
        }

        public MatroskaSubtitle(byte[] data, long start)
            : this(data, start, 0)
        {
        }

        public long End => Start + Duration;

        public string GetText(MatroskaTrackInfo matroskaTrackInfo)
        {
            var data = GetData(matroskaTrackInfo);

            if (data != null)
            {
                // terminate string at first binary zero - https://github.com/Matroska-Org/ebml-specification/blob/master/specification.markdown#terminating-elements
                // https://github.com/SubtitleEdit/subtitleedit/issues/2732
                int max = data.Length;
                for (int i = 0; i < max; i++)
                {
                    if (data[i] == 0)
                    {
                        max = i;
                        break;
                    }
                }
                var text = System.Text.Encoding.UTF8.GetString(data, 0, max);

                // normalize "new line" to current OS default - also see https://github.com/SubtitleEdit/subtitleedit/issues/2838
                text = text.Replace("\\N", Environment.NewLine);
                text = string.Join(Environment.NewLine, text.SplitToLines());

                return text;
            }
            return string.Empty;
        }
    }
}
