using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchDownloaderCore.Extensions
{
    public record struct StreamCopyProgress(long SourceLength, long BytesCopied);

    public static class StreamExtensions
    {
        // The default size from Stream.GetCopyBufferSize() is 81,920.
        private const int STREAM_DEFAULT_BUFFER_LENGTH = 81_920;

        public static async Task ProgressCopyToAsync(this Stream source, Stream destination, long? sourceLength, IProgress<StreamCopyProgress> progress = null,
            CancellationToken cancellationToken = default)
        {
            if (!sourceLength.HasValue || progress is null)
            {
                await source.CopyToAsync(destination, cancellationToken);
                return;
            }

            var rentedBuffer = ArrayPool<byte>.Shared.Rent(STREAM_DEFAULT_BUFFER_LENGTH);
            var buffer = rentedBuffer.AsMemory(0, STREAM_DEFAULT_BUFFER_LENGTH);

            long totalBytesRead = 0;
            try
            {
                int bytesRead;
                while ((bytesRead = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0)
                {
                    await destination.WriteAsync(buffer[..bytesRead], cancellationToken).ConfigureAwait(false);

                    totalBytesRead += bytesRead;
                    progress.Report(new StreamCopyProgress(sourceLength.Value, totalBytesRead));
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentedBuffer);
            }
        }

        public static async Task CopyBytesToAsync(this Stream source, Stream destination, long byteCount, CancellationToken cancellationToken = default)
        {
            var rentedBuffer = ArrayPool<byte>.Shared.Rent(STREAM_DEFAULT_BUFFER_LENGTH);

            try
            {
                long totalBytesRead = 0;
                while (totalBytesRead < byteCount)
                {
                    var bytesToCopy = (int)Math.Min(byteCount - totalBytesRead, STREAM_DEFAULT_BUFFER_LENGTH);

                    var bytesRead = await source.ReadAsync(rentedBuffer, 0, bytesToCopy, cancellationToken).ConfigureAwait(false);
                    await destination.WriteAsync(rentedBuffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);

                    totalBytesRead += bytesRead;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentedBuffer);
            }
        }
    }
}