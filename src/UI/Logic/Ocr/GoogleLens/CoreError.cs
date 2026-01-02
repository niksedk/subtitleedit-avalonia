using System;
using System.Net.Http.Headers;

namespace Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;

public class CoreError(string message, int code, HttpResponseHeaders headers, string body) : Exception(message)
{
}
