using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchDownloader.Extensions;

[SuppressMessage("ReSharper", "LocalizableElement")]
public static class StreamExtensions
{
    private const int DEFAULT_BUFFER_SIZE = 81920;

    public static void CopyTo(
        this Stream source,
        Stream destination,
        int bufferSize = DEFAULT_BUFFER_SIZE,
        IProgress<long>? progress = null
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!source.CanRead)
            throw new ArgumentException("Has to be readable", nameof(source));
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
            throw new ArgumentException("Has to be writable", nameof(destination));
        ArgumentOutOfRangeException.ThrowIfNegative(bufferSize);

        using var buffer = MemoryPool<byte>.Shared.Rent(bufferSize);
        long totalBytesRead = 0;
        int bytesRead;
        while ((bytesRead = source.Read(buffer.Memory.Span)) != 0)
        {
            destination.Write(buffer.Memory.Span[..bytesRead]);
            totalBytesRead += bytesRead;
            progress?.Report(totalBytesRead);
        }
    }

    public static async Task CopyToAsync(
        this Stream source,
        Stream destination,
        int bufferSize = DEFAULT_BUFFER_SIZE,
        IProgress<long>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!source.CanRead)
            throw new ArgumentException("Has to be readable", nameof(source));
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
            throw new ArgumentException("Has to be writable", nameof(destination));
        ArgumentOutOfRangeException.ThrowIfNegative(bufferSize);

        using var buffer = MemoryPool<byte>.Shared.Rent(bufferSize);
        long totalBytesRead = 0;
        int bytesRead;
        while (
            (
                bytesRead = await source
                    .ReadAsync(buffer.Memory, cancellationToken)
                    .ConfigureAwait(false)
            ) != 0
        )
        {
            await destination
                .WriteAsync(buffer.Memory[..bytesRead], cancellationToken)
                .ConfigureAwait(false);
            totalBytesRead += bytesRead;
            progress?.Report(totalBytesRead);
        }
    }
}
