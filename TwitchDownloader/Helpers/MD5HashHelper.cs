using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TwitchDownloader.Helpers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class MD5HashHelper
{
    [ThreadStatic]
    private static MD5? _threadInstance;

    private static MD5 HashAlgorithm => _threadInstance ??= MD5.Create();

    public static string GetMD5Hash(this string value) => ComputeHash(value);

    public static Guid GetGuidHash(this string value) => new(value.GetMD5Hash());

    public static string GetMD5Hash(this int value) => ComputeHash(value);

    public static string GetMD5Hash(this long value) => ComputeHash(value);

    public static string GetMD5Hash(this double value) => ComputeHash(value);

    public static string GetMD5Hash(this float value) => ComputeHash(value);

    public static string GetMD5Hash(this Stream stream) => ComputeHash(stream);

    public static string ComputeHash(string value)
    {
        var encoding = Encoding.UTF8;
        var bytesRequired = encoding.GetByteCount(value);
        Span<byte> bytes = stackalloc byte[bytesRequired];
        encoding.GetBytes(value, bytes);

        return ComputeHash(bytes);
    }

    public static string ComputeHash(int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);

        return ComputeHash(bytes);
    }

    public static string ComputeHash(long value)
    {
        // Convert long to byte array (little-endian)
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(bytes, value);

        return ComputeHash(bytes);
    }

    public static string ComputeHash(double value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(double)];
        BinaryPrimitives.WriteDoubleLittleEndian(bytes, value);

        return ComputeHash(bytes);
    }

    public static string ComputeHash(float value)
    {
        // Convert float to byte array (little-endian)
        Span<byte> bytes = stackalloc byte[sizeof(float)];
        BinaryPrimitives.WriteSingleLittleEndian(bytes, value);

        return ComputeHash(bytes);
    }

    public static string ComputeHash(Stream stream)
    {
        // Buffer to read from the stream
        using var buffer = MemoryPool<byte>.Shared.Rent(4096);
        using var memoryStream = new MemoryStream();
        int bytesRead;
        // Read the stream content into memory
        while ((bytesRead = stream.Read(buffer.Memory.Span)) > 0)
        {
            memoryStream.Write(buffer.Memory.Span[..bytesRead]);
        }

        // Pass the memoryStream's buffer to the hashing method
        return ComputeHash(memoryStream.ToArray());
    }

    public static unsafe string ComputeHash(ReadOnlySpan<byte> bytes)
    {
        Span<byte> hashBytes = stackalloc byte[16];
        HashAlgorithm.TryComputeHash(bytes, hashBytes, out _);

        const int charArrayLength = 32;
        var charArrayPtr = stackalloc char[charArrayLength];

        var charPtr = charArrayPtr;
        for (var i = 0; i < 16; i++)
        {
            var hashByte = hashBytes[i];
            *charPtr++ = GetHexValue(hashByte >> 4);
            *charPtr++ = GetHexValue(hashByte & 0xF);
        }

        return new string(charArrayPtr, 0, charArrayLength);
    }

    //Based on byte conversion implementation in BitConverter (but with the dash stripped)
    //https://github.com/dotnet/coreclr/blob/fbc11ea6afdaa2fe7b9377446d6bb0bd447d5cb5/src/mscorlib/shared/System/BitConverter.cs#L409-L440
    private static char GetHexValue(int i)
    {
        if (i < 10)
            return (char)(i + '0');

        return (char)(i - 10 + 'A');
    }
}
