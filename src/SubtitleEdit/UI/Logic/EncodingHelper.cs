using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Logic;

public static class EncodingHelper
{
    public static List<TextEncoding> GetEncodings()
    {
        var encodingList = new List<TextEncoding>();
        foreach (var encoding in Configuration.AvailableEncodings)
        {
            if (encoding.CodePage >= 874 && !encoding.IsEbcdic())
            {
                var item = new TextEncoding(encoding, null);
                if (encoding.CodePage.Equals(Encoding.UTF8.CodePage))
                {
                    item = new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithBom);
                    encodingList.Insert(TextEncoding.Utf8WithBomIndex, item);

                    item = new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithoutBom);
                    encodingList.Insert(TextEncoding.Utf8WithoutBomIndex, item);
                }
                else
                {
                    encodingList.Add(item);
                }
            }
        }

        return encodingList;
    }


}
