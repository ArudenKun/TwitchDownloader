﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetPath = System.IO.Path;

namespace TwitchDownloader.Utilities;

public record FilePath
{
    public string Path { get; }
    public bool IsDirectory { get; }

    public FilePath(string path, bool? isDirectory = null)
    {
        Path = path;
        IsDirectory =
            isDirectory
            ?? (Directory.Exists(path) && !File.Exists(path))
                || string.IsNullOrWhiteSpace(DotNetPath.GetExtension(path));
    }

    public FilePath Resolve(string subPath, bool? isDirectory = null) =>
        new(
            !Path.EndsWith('/') && !subPath.StartsWith('/') ? $"{Path}/{subPath}" : Path + subPath,
            isDirectory
        );

    public static FilePath Create(string path, bool? isDirectory = null) => new(path, isDirectory);

    public static FilePath operator /(FilePath left, string right) => left.Resolve(right);

    public static FilePath operator --(FilePath curr) =>
        curr.TryGetParent(out var parentDir) ? parentDir : curr;

    public static implicit operator FilePath(string path) => new(path);

    public static implicit operator string(FilePath filePath) => filePath.Path;

    public static implicit operator FilePath(DirectoryInfo path) => new(path.FullName);

    public static implicit operator DirectoryInfo(FilePath filePath)
    {
        if (filePath.IsDirectory)
        {
            return new DirectoryInfo(filePath.Path);
        }

        throw new InvalidOperationException("The path is not a directory");
    }

    public static implicit operator FilePath(FileInfo path) => new(path.FullName);

    public static implicit operator FileInfo(FilePath filePath)
    {
        if (!filePath.IsDirectory)
        {
            return new FileInfo(filePath.Path);
        }

        throw new InvalidOperationException("The path is not a file");
    }

    public bool TryGetParent(out FilePath filePath)
    {
        var parentDir = Directory.GetParent(Path);
        if (parentDir == null)
        {
            filePath = new FilePath(string.Empty, false);
            return false;
        }

        filePath = new FilePath(parentDir.FullName);
        return true;
    }

    public string Extension
    {
        get
        {
            if (IsDirectory)
                return string.Empty;

            var ext = DotNetPath.GetExtension(Path);
            if (ext.StartsWith('.'))
                ext = ext.TrimStart('.');

            return ext;
        }
    }

    public bool ExistsAsFile => File.Exists(Path);
    public bool ExistsAsDirectory => Directory.Exists(Path);

    public void Create()
    {
        if (IsDirectory)
            Directory.CreateDirectory(Path);
        else if (!ExistsAsFile)
            File.Create(Path).Dispose();
    }

    public List<FilePath> GetFiles() =>
        IsDirectory
            ? Directory.GetFiles(Path).Select(path => new FilePath(path, false)).ToList()
            : [];

    public List<FilePath> GetSubdirectories() =>
        IsDirectory
            ? Directory.GetDirectories(Path).Select(path => new FilePath(path, true)).ToList()
            : [];

    public void WriteAllText(string text) => File.WriteAllText(Path, text);

    public Task WriteAllTextAsync(string text) => File.WriteAllTextAsync(Path, text);

    public void WriteAllBytes(byte[] bytes) => File.WriteAllBytes(Path, bytes);

    public Task WriteAllBytesAsync(byte[] bytes) => File.WriteAllBytesAsync(Path, bytes);

    public void WriteAllLines(IEnumerable<string> lines) => File.WriteAllLines(Path, lines);

    public Task WriteAllLinesAsync(IEnumerable<string> lines) =>
        File.WriteAllLinesAsync(Path, lines);

    public void AppendAllText(string text) => File.AppendAllText(Path, text);

    public Task AppendAllTextAsync(string text) => File.AppendAllTextAsync(Path, text);

    public void AppendAllLines(IEnumerable<string> lines) => File.AppendAllLines(Path, lines);

    public Task AppendAllLinesAsync(IEnumerable<string> lines) =>
        File.AppendAllLinesAsync(Path, lines);

    public string ReadAllText(Encoding? encoding = null) =>
        File.ReadAllText(Path, encoding ?? Encoding.UTF8);

    public Task<string> ReadAllTextAsync(Encoding? encoding = null) =>
        File.ReadAllTextAsync(Path, encoding ?? Encoding.UTF8);

    public string[] ReadAllLines(Encoding? encoding = null) =>
        File.ReadAllLines(Path, encoding ?? Encoding.UTF8);

    public Task<string[]> ReadAllLinesAsync(Encoding? encoding = null) =>
        File.ReadAllLinesAsync(Path, encoding ?? Encoding.UTF8);

    public byte[] ReadAllBytes() => File.ReadAllBytes(Path);

    public Task<byte[]> ReadAllBytesAsync() => File.ReadAllBytesAsync(Path);

    public FileStream OpenRead() => File.OpenRead(Path);

    public FileStream OpenWrite() => File.OpenWrite(Path);

    public FileStream? OpenCreate() => !ExistsAsFile ? File.Create(Path) : null;

    public override string ToString() => Path;
}
