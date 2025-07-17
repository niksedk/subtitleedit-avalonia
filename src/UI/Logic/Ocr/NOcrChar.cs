using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Ocr;

public class NOcrChar
{
    public string Text { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int MarginTop { get; set; }
    public bool Italic { get; set; }
    public List<NOcrLine> LinesForeground { get; }
    public List<NOcrLine> LinesBackground { get; }
    public int ExpandCount { get; set; }
    public bool LoadedOk { get; }

    public double WidthPercent => Height * 100.0 / Width;

    public NOcrChar()
    {
        LinesForeground = new List<NOcrLine>();
        LinesBackground = new List<NOcrLine>();
        Text = string.Empty;
    }

    public NOcrChar(NOcrChar old)
    {
        LinesForeground = new List<NOcrLine>(old.LinesForeground.Count);
        LinesBackground = new List<NOcrLine>(old.LinesBackground.Count);
        Text = old.Text;
        Width = old.Width;
        Height = old.Height;
        MarginTop = old.MarginTop;
        Italic = old.Italic;
        foreach (var p in old.LinesForeground)
        {
            LinesForeground.Add(new NOcrLine(new OcrPoint(p.Start.X, p.Start.Y), new OcrPoint(p.End.X, p.End.Y)));
        }

        foreach (var p in old.LinesBackground)
        {
            LinesBackground.Add(new NOcrLine(new OcrPoint(p.Start.X, p.Start.Y), new OcrPoint(p.End.X, p.End.Y)));
        }
    }

    public NOcrChar(string text)
        : this()
    {
        Text = text;
    }

    public override string ToString()
    {
        return Text;
    }

    public bool IsSensitive => Text is "O" or "o" or "0" or "'" or "-" or ":" or "\"";

    public NOcrChar(ref int position, byte[] file)
    {
        Text = string.Empty;
        LinesForeground = new List<NOcrLine>();
        LinesBackground = new List<NOcrLine>();

        try
        {
            var buffer = new byte[4];
            if (position + buffer.Length >= file.Length)
            {
                LoadedOk = false;
                return;
            }

            var isShort = (file[position] & 0b0001_0000) > 0;
            Italic = (file[position] & 0b0010_0000) > 0;

            if (isShort)
            {
                ExpandCount = file[position++] & 0b0000_1111;
                Width = file[position++];
                Height = file[position++];
                MarginTop = file[position++];
            }
            else
            {
                ExpandCount = file[position++];
                Width = file[position++] << 8 | file[position++];
                Height = file[position++] << 8 | file[position++];
                MarginTop = file[position++] << 8 | file[position++];
            }

            var textLen = file[position++];
            if (textLen > 0)
            {
                Text = System.Text.Encoding.UTF8.GetString(file, position, textLen);
                position += textLen;
            }
            else
            {
                Text = string.Empty;
            }

            if (isShort)
            {
                LinesForeground = ReadPointsBytes(ref position, file);
                LinesBackground = ReadPointsBytes(ref position, file);
            }
            else
            {
                LinesForeground = ReadPoints(ref position, file);
                LinesBackground = ReadPoints(ref position, file);
            }

            if (Width > 0 && Height > 0 && Width <= 1920 && Height <= 1080 && Text.IndexOf('\0') < 0)
            {
                LoadedOk = true;
            }
            else
            {
                LoadedOk = false;
            }
        }
        catch
        {
            LoadedOk = false;
        }
    }

    private static List<NOcrLine> ReadPoints(ref int position, byte[] file)
    {
        var length = file[position++] << 8 | file[position++];
        var list = new List<NOcrLine>(length);
        for (var i = 0; i < length; i++)
        {
            var point = new NOcrLine
            {
                Start = new OcrPoint(file[position++] << 8 | file[position++], file[position++] << 8 | file[position++]),
                End = new OcrPoint(file[position++] << 8 | file[position++], file[position++] << 8 | file[position++])
            };
            list.Add(point);
        }

        return list;
    }

    private static List<NOcrLine> ReadPointsBytes(ref int position, byte[] file)
    {
        var length = file[position++];
        var list = new List<NOcrLine>(length);
        for (var i = 0; i < length; i++)
        {
            var point = new NOcrLine
            {
                Start = new OcrPoint(file[position++], file[position++]),
                End = new OcrPoint(file[position++], file[position++])
            };
            list.Add(point);
        }
        return list;
    }

    internal void Save(Stream stream)
    {
        if (IsAllByteValues())
        {
            SaveOneBytes(stream);
        }
        else
        {
            SaveTwoBytes(stream);
        }
    }

    private bool IsAllByteValues()
    {
        return Width <= byte.MaxValue && Height <= byte.MaxValue && ExpandCount < 16 &&
               LinesBackground.Count <= byte.MaxValue && LinesForeground.Count <= byte.MaxValue &&
               IsAllPointByteValues(LinesForeground) && IsAllPointByteValues(LinesForeground);
    }

    private static bool IsAllPointByteValues(List<NOcrLine> lines)
    {
        for (var index = 0; index < lines.Count; index++)
        {
            var point = lines[index];
            if (point.Start.X > byte.MaxValue || point.Start.Y > byte.MaxValue ||
                point.End.X > byte.MaxValue || point.End.Y > byte.MaxValue)
            {
                return false;
            }
        }

        return true;
    }

    private void SaveOneBytes(Stream stream)
    {
        var flags = 0b0001_0000;

        if (Italic)
        {
            flags |= 0b0010_0000;
        }

        if (ExpandCount > 0)
        {
            flags |= (byte)ExpandCount;
        }

        stream.WriteByte((byte)flags);

        stream.WriteByte((byte)Width);
        stream.WriteByte((byte)Height);
        stream.WriteByte((byte)MarginTop);

        if (Text == null)
        {
            stream.WriteByte(0);
        }
        else
        {
            var textBuffer = System.Text.Encoding.UTF8.GetBytes(Text);
            stream.WriteByte((byte)textBuffer.Length);
            stream.Write(textBuffer, 0, textBuffer.Length);
        }
        WritePointsAsOneByte(stream, LinesForeground);
        WritePointsAsOneByte(stream, LinesBackground);
    }

    private void SaveTwoBytes(Stream stream)
    {
        var flags = 0b0000_0000;

        if (Italic)
        {
            flags |= 0b0010_0000;
        }

        stream.WriteByte((byte)flags);
        stream.WriteByte((byte)ExpandCount);

        WriteInt16(stream, (ushort)Width);
        WriteInt16(stream, (ushort)Height);
        WriteInt16(stream, (ushort)MarginTop);

        if (Text == null)
        {
            stream.WriteByte(0);
        }
        else
        {
            var textBuffer = System.Text.Encoding.UTF8.GetBytes(Text);
            stream.WriteByte((byte)textBuffer.Length);
            stream.Write(textBuffer, 0, textBuffer.Length);
        }
        WritePoints(stream, LinesForeground);
        WritePoints(stream, LinesBackground);
    }

    private static void WritePointsAsOneByte(Stream stream, List<NOcrLine> points)
    {
        stream.WriteByte((byte)points.Count);
        foreach (var nOcrPoint in points)
        {
            stream.WriteByte((byte)nOcrPoint.Start.X);
            stream.WriteByte((byte)nOcrPoint.Start.Y);
            stream.WriteByte((byte)nOcrPoint.End.X);
            stream.WriteByte((byte)nOcrPoint.End.Y);
        }
    }

    private static void WritePoints(Stream stream, List<NOcrLine> points)
    {
        WriteInt16(stream, (ushort)points.Count);
        foreach (var nOcrPoint in points)
        {
            WriteInt16(stream, (ushort)nOcrPoint.Start.X);
            WriteInt16(stream, (ushort)nOcrPoint.Start.Y);
            WriteInt16(stream, (ushort)nOcrPoint.End.X);
            WriteInt16(stream, (ushort)nOcrPoint.End.Y);
        }
    }

    private static void WriteInt16(Stream stream, ushort val)
    {
        var buffer = new byte[2];
        buffer[0] = (byte)((val & 0xFF00) >> 8);
        buffer[1] = (byte)(val & 0x00FF);
        stream.Write(buffer, 0, buffer.Length);
    }

    public static void GenerateLineSegments(int maxNumberOfLines, bool veryPrecise, NOcrChar nOcrChar, NikseBitmap2 bitmap)
    {
        const int giveUpCount = 15_000;
        var r = new Random();

        // Generate foreground lines
        GenerateLineSegmentsForType(maxNumberOfLines, veryPrecise, nOcrChar, bitmap, r, giveUpCount, true);

        // Generate background lines (same amount as foreground)
        GenerateLineSegmentsForType(maxNumberOfLines, veryPrecise, nOcrChar, bitmap, r, giveUpCount, false);

        RemoveDuplicates(nOcrChar.LinesForeground);
        RemoveDuplicates(nOcrChar.LinesBackground);
    }

    private static void GenerateLineSegmentsForType(int maxNumberOfLines, bool veryPrecise, NOcrChar nOcrChar,
        NikseBitmap2 bitmap, Random r, int giveUpCount, bool isForeground)
    {
        var count = 0;
        var hits = 0;
        var tempVeryPrecise = veryPrecise;
        var lines = isForeground ? nOcrChar.LinesForeground : nOcrChar.LinesBackground;

        // Phase 1: Generate systematic lines (vertical and horizontal)
        hits += GenerateSystematicLines(nOcrChar, bitmap, isForeground, Math.Min(maxNumberOfLines / 3, 20));

        var usedRegions = new Dictionary<(int, int), int>();

        // Phase 2: Generate remaining lines with different strategies
        while (hits < maxNumberOfLines && count < giveUpCount)
        {
            NOcrLine line;

            if (count % 50 == 0)
            {
                RemoveDuplicates(isForeground ? nOcrChar.LinesForeground : nOcrChar.LinesBackground);
            }

            if (hits < maxNumberOfLines * 0.1) // 20% short lines
            {
                line = GenerateShortLineSimple(nOcrChar, r);
            }
            else if (hits < maxNumberOfLines * 0.65) // 45% short lines for details
            {
                line = GenerateShortLine(nOcrChar, r, usedRegions);
            }
            else if (hits < maxNumberOfLines * 0.85) // 20% medium lines
            {
                line = GenerateMediumLine(nOcrChar, r);
            }
            else // 15% long lines
            {
                line = GenerateLongLine(nOcrChar, r);
            }

            // Only check for exact duplicates and zero-length lines
            if (IsExactDuplicate(line, lines))
            {
                count++;
                continue;
            }

            // Test the line
            bool isMatch = isForeground ?
                IsMatchPointForeGround(line, !tempVeryPrecise, bitmap, nOcrChar) :
                IsMatchPointBackGround(line, !tempVeryPrecise, bitmap, nOcrChar);

            if (isMatch)
            {
                lines.Add(line);
                hits++;
            }

            count++;

            // Switch to more precise mode near the end
            if (count > giveUpCount - 1000 && !tempVeryPrecise)
            {
                tempVeryPrecise = true;
            }
        }
    }

    private static int GenerateSystematicLines(NOcrChar nOcrChar, NikseBitmap2 bitmap, bool isForeground, int maxLines)
    {
        var lines = isForeground ? nOcrChar.LinesForeground : nOcrChar.LinesBackground;
        var hits = 0;

        if (nOcrChar.Width <= 4 || nOcrChar.Height <= 4) return 0;

        // Generate vertical lines with reasonable spacing
        var verticalSpacing = Math.Max(1, nOcrChar.Width / 15);
        for (int x = 2; x < nOcrChar.Width - 2 && hits < maxLines / 2; x += verticalSpacing)
        {
            var line = new NOcrLine(new OcrPoint(x, 1), new OcrPoint(x, nOcrChar.Height - 2));

            bool isMatch = isForeground ?
                IsMatchPointForeGround(line, true, bitmap, nOcrChar) :
                IsMatchPointBackGround(line, true, bitmap, nOcrChar);

            if (isMatch)
            {
                lines.Add(line);
                hits++;
            }
        }

        // Generate horizontal lines with reasonable spacing
        var horizontalSpacing = Math.Max(1, nOcrChar.Height / 15);
        for (int y = 2; y < nOcrChar.Height - 2 && hits < maxLines; y += horizontalSpacing)
        {
            var line = new NOcrLine(new OcrPoint(1, y), new OcrPoint(nOcrChar.Width - 2, y));

            bool isMatch = isForeground ?
                IsMatchPointForeGround(line, true, bitmap, nOcrChar) :
                IsMatchPointBackGround(line, true, bitmap, nOcrChar);

            if (isMatch)
            {
                lines.Add(line);
                hits++;
            }
        }

        return hits;
    }

    private static NOcrLine GenerateLongLine(NOcrChar nOcrChar, Random r)
    {
        var minLength = Math.Max(nOcrChar.Width, nOcrChar.Height) / 3;

        for (int attempt = 0; attempt < 250; attempt++)
        {
            var start = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));

            // Generate end point in random direction
            var angle = r.NextDouble() * Math.PI * 2;
            var length = r.Next((int)minLength, Math.Max(nOcrChar.Width, nOcrChar.Height));

            var endX = (int)(start.X + Math.Cos(angle) * length);
            var endY = (int)(start.Y + Math.Sin(angle) * length);

            // Clamp to bounds
            endX = Math.Max(0, Math.Min(nOcrChar.Width - 1, endX));
            endY = Math.Max(0, Math.Min(nOcrChar.Height - 1, endY));

            var end = new OcrPoint(endX, endY);
            var actualLength = Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y);

            if (actualLength >= minLength)
            {
                return new NOcrLine(start, end);
            }
        }

        return new NOcrLine();
    }

    private static NOcrLine GenerateMediumLine(NOcrChar nOcrChar, Random r)
    {
        var minLength = 3;
        var maxLength = Math.Max(nOcrChar.Width, nOcrChar.Height) / 2;

        for (int attempt = 0; attempt < 200; attempt++)
        {
            var start = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
            var end = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));

            var length = Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y);

            if (length >= minLength && length <= maxLength)
            {
                return new NOcrLine(start, end);
            }
        }

        return new NOcrLine();
    }

    private static readonly int regionRows = 3;
    private static readonly int regionCols = 3;
    private static NOcrLine GenerateShortLine(NOcrChar nOcrChar, Random r, Dictionary<(int, int), int>? usedRegions = null)
    {
        const int maxLength = 10;
        const int minLength = 2;
        const int maxAttempts = 300;

        int cellWidth = nOcrChar.Width / regionCols;
        int cellHeight = nOcrChar.Height / regionRows;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Choose a random region, but prefer less-used ones
            var allRegions = Enumerable.Range(0, regionCols)
                .SelectMany(x => Enumerable.Range(0, regionRows).Select(y => (x, y)))
                .OrderBy(region =>
                    usedRegions?.GetValueOrDefault(region, 0) + r.Next(0, 3)) // favor less-used regions with some randomness
                .ToList();

            foreach (var region in allRegions)
            {
                int regionX = region.Item1;
                int regionY = region.Item2;

                int startX = r.Next(regionX * cellWidth, Math.Min((regionX + 1) * cellWidth, nOcrChar.Width));
                int startY = r.Next(regionY * cellHeight, Math.Min((regionY + 1) * cellHeight, nOcrChar.Height));

                int offsetX = r.Next(-maxLength, maxLength + 1);
                int offsetY = r.Next(-maxLength, maxLength + 1);

                int endX = Math.Clamp(startX + offsetX, 0, nOcrChar.Width - 1);
                int endY = Math.Clamp(startY + offsetY, 0, nOcrChar.Height - 1);

                int length = Math.Abs(endX - startX) + Math.Abs(endY - startY);
                if (length >= minLength && length <= maxLength)
                {
                    // Mark region as used
                    if (usedRegions != null)
                    {
                        usedRegions.TryGetValue(region, out int count);
                        usedRegions[region] = count + 1;
                    }

                    return new NOcrLine(new OcrPoint(startX, startY), new OcrPoint(endX, endY));
                }
            }
        }

        return new NOcrLine();
    }

    private static int ShortLineLastX = -1;
    private static NOcrLine GenerateShortLineSimple(NOcrChar nOcrChar, Random r)
    {
        for (int attempt = 0; attempt < 200; attempt++)
        {
            var start = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
            var end = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));

            var length = Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y);

            if (length > 5 || length <= 1)
            {
                continue;
            }

            if (ShortLineLastX >= 0)
            {
                var halfX = nOcrChar.Width / 2;
                if (start.X < halfX && ShortLineLastX < halfX ||
                    start.X > halfX && ShortLineLastX > halfX)
                {
                    // Avoid generating too many short lines in the same side
                    continue;
                }
            }

            ShortLineLastX = start.X;
            return new NOcrLine(start, end);
        }

        return new NOcrLine();
    }

    private static bool IsExactDuplicate(NOcrLine line, List<NOcrLine> existingLines)
    {
        // Check for zero-length lines
        if (line.Start.X == line.End.X && line.Start.Y == line.End.Y)
        {
            return true;
        }

        // Check for exact duplicates (including reverse)
        foreach (var existing in existingLines)
        {
            if ((existing.Start.X == line.Start.X && existing.Start.Y == line.Start.Y &&
                 existing.End.X == line.End.X && existing.End.Y == line.End.Y) ||
                (existing.Start.X == line.End.X && existing.Start.Y == line.End.Y &&
                 existing.End.X == line.Start.X && existing.End.Y == line.Start.Y))
            {
                return true;
            }
        }

        return false;
    }

    public static void GenerateLineSegmentsOld(int maxNumberOfLines, bool veryPrecise, NOcrChar nOcrChar, NikseBitmap2 bitmap)
    {
        const int giveUpCount = 15_000;
        var r = new Random();
        var count = 0;
        var hits = 0;
        var tempVeryPrecise = veryPrecise;
        var verticalLineX = 2;
        var horizontalLineY = 2;
        while (hits < maxNumberOfLines && count < giveUpCount)
        {
            var start = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
            var end = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));

            if (hits < 5 && count < 200 && nOcrChar.Width > 4 && nOcrChar.Height > 4) // vertical lines
            {
                start = new OcrPoint(0, 0);
                end = new OcrPoint(0, 0);
                for (; verticalLineX < nOcrChar.Width - 3; verticalLineX += 1)
                {
                    start = new OcrPoint(verticalLineX, 2);
                    end = new OcrPoint(verticalLineX, nOcrChar.Height - 3);

                    if (IsMatchPointForeGround(new NOcrLine(start, end), true, bitmap, nOcrChar))
                    {
                        verticalLineX++;
                        break;
                    }
                }
            }
            else if (hits < 10 && count < 400 && nOcrChar.Width > 4 && nOcrChar.Height > 4) // horizontal lines
            {
                start = new OcrPoint(0, 0);
                end = new OcrPoint(0, 0);
                for (; horizontalLineY < nOcrChar.Height - 3; horizontalLineY += 1)
                {
                    start = new OcrPoint(2, horizontalLineY);
                    end = new OcrPoint(nOcrChar.Width - 3, horizontalLineY);

                    if (IsMatchPointForeGround(new NOcrLine(start, end), true, bitmap, nOcrChar))
                    {
                        horizontalLineY++;
                        break;
                    }
                }
            }
            else if (hits < 20 && count < 2000) // a few large lines
            {
                for (var k = 0; k < 500; k++)
                {
                    if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) > nOcrChar.Height / 2)
                    {
                        break;
                    }

                    end = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                }
            }
            else if (hits < 30 && count < 3000) // some medium lines
            {
                for (var k = 0; k < 500; k++)
                {
                    if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) < 15)
                    {
                        break;
                    }

                    end = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                }
            }
            else // and a lot of small lines
            {
                for (var k = 0; k < 500; k++)
                {
                    if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) < 15)
                    {
                        break;
                    }

                    end = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                }
            }

            var op = new NOcrLine(start, end);
            var ok = true;
            foreach (var existingOp in nOcrChar.LinesForeground)
            {
                if (existingOp.Start.X == op.Start.X && existingOp.Start.Y == op.Start.Y &&
                    existingOp.End.X == op.End.X && existingOp.End.Y == op.End.Y)
                {
                    ok = false;
                }
            }

            if (end.X == start.X && end.Y == start.Y)
            {
                ok = false;
            }

            if (ok && IsMatchPointForeGround(op, !tempVeryPrecise, bitmap, nOcrChar))
            {
                nOcrChar.LinesForeground.Add(op);
                hits++;
            }
            count++;
            if (count > giveUpCount - 100 && !tempVeryPrecise)
            {
                tempVeryPrecise = true;
            }
        }

        count = 0;
        hits = 0;
        horizontalLineY = 2;
        tempVeryPrecise = veryPrecise;
        while (hits < maxNumberOfLines && count < giveUpCount)
        {
            var start = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
            var end = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));

            if (hits < 5 && count < 400 && nOcrChar.Width > 4 && nOcrChar.Height > 4) // horizontal lines
            {
                for (; horizontalLineY < nOcrChar.Height - 3; horizontalLineY += 1)
                {
                    start = new OcrPoint(2, horizontalLineY);
                    end = new OcrPoint(nOcrChar.Width - 2, horizontalLineY);

                    if (IsMatchPointBackGround(new NOcrLine(start, end), true, bitmap, nOcrChar))
                    {
                        horizontalLineY++;
                        break;
                    }
                }
            }
            if (hits < 10 && count < 1000) // a few large lines
            {
                for (var k = 0; k < 500; k++)
                {
                    if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) > nOcrChar.Height / 2)
                    {
                        break;
                    }
                    else
                    {
                        end = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                    }
                }
            }
            else if (hits < 30 && count < 2000) // some medium lines
            {
                for (var k = 0; k < 500; k++)
                {
                    if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) < 15)
                    {
                        break;
                    }

                    end = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                }
            }
            else // and a lot of small lines
            {
                for (var k = 0; k < 500; k++)
                {
                    if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) < 5)
                    {
                        break;
                    }
                    else
                    {
                        end = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                    }
                }
            }

            var op = new NOcrLine(start, end);
            var ok = true;
            foreach (var existingOp in nOcrChar.LinesBackground)
            {
                if (existingOp.Start.X == op.Start.X && existingOp.Start.Y == op.Start.Y &&
                    existingOp.End.X == op.End.X && existingOp.End.Y == op.End.Y)
                {
                    ok = false;
                }
            }
            if (ok && IsMatchPointBackGround(op, !tempVeryPrecise, bitmap, nOcrChar))
            {
                nOcrChar.LinesBackground.Add(op);
                hits++;
            }
            count++;

            if (count > giveUpCount - 100 && !tempVeryPrecise)
            {
                tempVeryPrecise = true;
            }
        }

        RemoveDuplicates(nOcrChar.LinesForeground);
        RemoveDuplicates(nOcrChar.LinesBackground);
    }

    private static bool IsMatchPointForeGround(NOcrLine op, bool loose, NikseBitmap2 nbmp, NOcrChar nOcrChar)
    {
        if (Math.Abs(op.Start.X - op.End.X) < 2 && Math.Abs(op.End.Y - op.Start.Y) < 2)
        {
            return false;
        }

        foreach (var point in op.ScaledGetPoints(nOcrChar, nbmp.Width, nbmp.Height))
        {
            if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
            {
                var c = nbmp.GetPixel(point.X, point.Y);
                if (c.Alpha > 150)
                {
                }
                else
                {
                    return false;
                }

                if (loose)
                {
                    if (nbmp.Width > 10 && point.X + 1 < nbmp.Width)
                    {
                        c = nbmp.GetPixel(point.X + 1, point.Y);
                        if (c.Alpha > 150)
                        {
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (nbmp.Width > 10 && point.X >= 1)
                    {
                        c = nbmp.GetPixel(point.X - 1, point.Y);
                        if (c.Alpha > 150)
                        {
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (nbmp.Height > 10 && point.Y + 1 < nbmp.Height)
                    {
                        c = nbmp.GetPixel(point.X, point.Y + 1);
                        if (c.Alpha > 150)
                        {
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (nbmp.Height > 10 && point.Y >= 1)
                    {
                        c = nbmp.GetPixel(point.X, point.Y - 1);
                        if (c.Alpha > 150)
                        {
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }

    private static bool IsMatchPointBackGround(NOcrLine op, bool loose, NikseBitmap2 nbmp, NOcrChar nOcrChar)
    {
        foreach (var point in op.ScaledGetPoints(nOcrChar, nbmp.Width, nbmp.Height))
        {
            if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
            {
                var c = nbmp.GetPixel(point.X, point.Y);
                if (c.Alpha > 150)
                {
                    return false;
                }

                if (nbmp.Width > 10 && point.X + 1 < nbmp.Width)
                {
                    c = nbmp.GetPixel(point.X + 1, point.Y);
                    if (c.Alpha > 150)
                    {
                        return false;
                    }
                }

                if (loose)
                {
                    if (nbmp.Width > 10 && point.X >= 1)
                    {
                        c = nbmp.GetPixel(point.X - 1, point.Y);
                        if (c.Alpha > 150)
                        {
                            return false;
                        }
                    }

                    if (nbmp.Height > 10 && point.Y + 1 < nbmp.Height)
                    {
                        c = nbmp.GetPixel(point.X, point.Y + 1);
                        if (c.Alpha > 150)
                        {
                            return false;
                        }
                    }

                    if (nbmp.Height > 10 && point.Y >= 1)
                    {
                        c = nbmp.GetPixel(point.X, point.Y - 1);
                        if (c.Alpha > 150)
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private static void RemoveDuplicates(List<NOcrLine> lines)
    {
        var indicesToDelete = new List<int>();
        for (var index = 0; index < lines.Count; index++)
        {
            var outerPoint = lines[index];
            for (var innerIndex = 0; innerIndex < lines.Count; innerIndex++)
            {
                var innerPoint = lines[innerIndex];
                if (innerPoint != outerPoint)
                {
                    if (innerPoint.Start.X == innerPoint.End.X && outerPoint.Start.X == outerPoint.End.X && innerPoint.Start.X == outerPoint.Start.X)
                    {
                        // same y
                        if (Math.Max(innerPoint.Start.Y, innerPoint.End.Y) <= Math.Max(outerPoint.Start.Y, outerPoint.End.Y) &&
                            Math.Min(innerPoint.Start.Y, innerPoint.End.Y) >= Math.Min(outerPoint.Start.Y, outerPoint.End.Y))
                        {
                            if (!indicesToDelete.Contains(innerIndex))
                            {
                                indicesToDelete.Add(innerIndex);
                            }
                        }
                    }
                    else if (innerPoint.Start.Y == innerPoint.End.Y && outerPoint.Start.Y == outerPoint.End.Y && innerPoint.Start.Y == outerPoint.Start.Y)
                    {
                        // same x
                        if (Math.Max(innerPoint.Start.X, innerPoint.End.X) <= Math.Max(outerPoint.Start.X, outerPoint.End.X) &&
                            Math.Min(innerPoint.Start.X, innerPoint.End.X) >= Math.Min(outerPoint.Start.X, outerPoint.End.X))
                        {
                            if (!indicesToDelete.Contains(innerIndex))
                            {
                                indicesToDelete.Add(innerIndex);
                            }
                        }
                    }
                }
            }
        }

        foreach (var i in indicesToDelete.OrderByDescending(p => p))
        {
            lines.RemoveAt(i);
        }
    }
}