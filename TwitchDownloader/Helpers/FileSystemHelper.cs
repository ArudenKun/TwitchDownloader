using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TwitchDownloader.Helpers;

public static class FileSystemHelper
{
    public const int DEFAULT_BUFFER_SIZE = 4096;

    public static string JoinPath(this string path, params string[] parts)
    {
        var paths = new List<string> { path };
        paths.AddRange(parts);
        return Path.Combine([.. paths]);
    }

    /// <summary>
    ///     Returns the absolute path for the specified path string.
    ///     Also searches the environment's PATH variable.
    /// </summary>
    /// <param name="fileName">The relative path string.</param>
    /// <returns>The absolute path or null if the file was not found.</returns>
    public static string? GetFullPath(string fileName)
    {
        if (File.Exists(fileName))
            return Path.GetFullPath(fileName);

        var env = Environment.GetEnvironmentVariable("PATH");

        return env
            ?.Split(Path.PathSeparator)
            .Select(p => Path.Combine(p, fileName))
            .FirstOrDefault(File.Exists);
    }

    public static string GetParent(string currentPath, int level = 1)
    {
        if (level == 0)
        {
            level = 1;
        }

        var path = "";
        for (var i = 0; i < level; i++)
            path += $"..{Path.DirectorySeparatorChar}";

        return Path.GetFullPath(Path.Combine(currentPath, path));
    }

    /// <summary>
    ///     Gets the path of the command executable.
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static string? GetFromEnvironment(string cmd)
    {
        string? result = null;
        var whichCommand = OperatingSystem.IsWindows() ? "where" : "which";

        using Process process = new();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = whichCommand,
            Arguments = cmd,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        process.EnableRaisingEvents = true;
        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data != null)
                result = args.Data;
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        return result;
    }

    /// <summary>
    ///     Gets the path of the command executable.
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static ValueTask<string?> GetFromEnvironmentAsync(string cmd) =>
        ValueTask.FromResult(GetFromEnvironment(cmd));

    public static string SanitizeFileName(this string source, char replacementChar = '_')
    {
        var blackList = new HashSet<char>(Path.GetInvalidFileNameChars()) { '"' }; // '"' not invalid in Linux, but causes problems
        var output = source.ToCharArray();
        for (int i = 0, ln = output.Length; i < ln; i++)
            if (blackList.Contains(output[i]))
                output[i] = replacementChar;

        return new string(output);
    }

    public static FileStream OpenRead(string path, int bufferSize = DEFAULT_BUFFER_SIZE) =>
        new(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);

    public static ValueTask<FileStream> OpenReadAsync(
        string path,
        int bufferSize = DEFAULT_BUFFER_SIZE
    ) => ValueTask.FromResult(OpenRead(path, bufferSize));

    public static FileStream OpenWrite(
        string filePath,
        bool overwrite = true,
        int bufferSize = DEFAULT_BUFFER_SIZE
    ) =>
        new(
            filePath,
            overwrite ? FileMode.Create : FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize,
            true
        );

    public static ValueTask<FileStream> OpenWriteAsync(
        string filePath,
        bool overwrite = true,
        int bufferSize = DEFAULT_BUFFER_SIZE
    ) => ValueTask.FromResult(OpenWrite(filePath, overwrite, bufferSize));
}
