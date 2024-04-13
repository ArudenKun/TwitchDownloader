using System.Collections.Generic;
using System.IO;

namespace TwitchDownloader.Helpers;

public static class FileHelper
{
    public static void EnsureDirectoryExists(string path)
    {
        if (Path.IsPathRooted(path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        }
    }

    public static string SanitizeFileName(string fileName, char replacementChar = '_')
    {
        var blackList = new HashSet<char>(Path.GetInvalidFileNameChars()) { '"' }; // '"' not invalid in Linux, but causes problems
        var output = fileName.ToCharArray();
        for (int i = 0, ln = output.Length; i < ln; i++)
        {
            if (blackList.Contains(output[i]))
            {
                output[i] = replacementChar;
            }
        }
        return new string(output);
    }
}
