using System;
using System.Diagnostics;

namespace Nikse.SubtitleEdit.Logic;

public class MacHelper
{
    public static bool MakeExecutable(string filePath)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/chmod",
                Arguments = $"+x \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine("chmod failed:");
            Console.WriteLine(error);
            return false;
        }

        return true;
    }
}