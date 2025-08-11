using System.Buffers;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Windows.Media.Imaging;

namespace MacroscopImageDownloader.Models
{
    internal class BitmapDownload : IDownload
    {
        private const int BufferSize = (1 << 10) * 64;
        private const double MaxWaitSeconds = 3d;

        private async Task<BitmapSource> DownloadAsync(Uri uri, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
        {
            using (Stream loadedStream = await Load(uri, progress, cancellationToken))
            {
                return await Decode(loadedStream, progress, cancellationToken);
            }
        }

        private Task<BitmapSource> Decode(Stream loadedStream, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report(new ProgressInfo(92, DownloadStatus.Active));
                BitmapDecoder decoder = BitmapDecoder.Create(loadedStream, BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
                progress?.Report(new ProgressInfo(98, DownloadStatus.Active));
                BitmapSource result = decoder.Frames[0];
                return result;
            }, cancellationToken);
        }

        private async Task<Stream> Load(Uri uri, IProgress<ProgressInfo>? progress, CancellationToken token, int retryCount = 5)
        {
            token.ThrowIfCancellationRequested();
            progress?.Report(new ProgressInfo(0, DownloadStatus.Active));
            Stream resultStream = Stream.Synchronized(new MemoryStream());

            using (var response = await Http.Client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token))
            {
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(Random.Shared.NextDouble() * MaxWaitSeconds);
                    await Task.Delay(retryAfter, token);
                    return await Load(uri, progress, token, retryCount - 1);
                }
                response.EnsureSuccessStatusCode();
                if (response.Content.Headers.ContentLength.HasValue)
                {
                    long contentLength = response.Content.Headers.ContentLength.Value;
                    await InBuffer(async buffer => await CopyStream(buffer, await response.Content.ReadAsStreamAsync(), resultStream, contentLength, token, progress), BufferSize);
                }
            }

            resultStream.Seek(0, SeekOrigin.Begin);
            return resultStream;
        }

        private async Task CopyStream(byte[] bufferArray,
                                Stream inputStream,
                                Stream outputStream,
                                long length,
                                CancellationToken token,
                                IProgress<ProgressInfo>? progress,
                                double progressMultiplier = 0.9d)
        {
            int read = 0;
            for (long i = 0; i < length; i += read)
            {
                token.ThrowIfCancellationRequested();
                read = await inputStream.ReadAsync(bufferArray, 0, BufferSize);
                await outputStream.WriteAsync(bufferArray, 0, read);
                progress?.Report(new ProgressInfo((int)(i * 100 / length * progressMultiplier), DownloadStatus.Active));
            }
        }

        private async Task InBuffer(Func<byte[], Task> inBufferAction, int bufferLength)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferLength);
            try
            {
                await inBufferAction(buffer);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private BitmapSource? _image;

        public BitmapSource? Image
        {
            get
            {
                return _image;
            }
            private set
            {
                if (_image != value)
                {
                    _image = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
                }
            }
        }

        public void Start(Uri uri, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
        {
            DownloadAsync(uri, progress, CancellationToken.None)
                .ContinueWith(t =>
                {
                    if (t.Exception == null && t.IsCompleted)
                    {
                        Image = t.Result;
                        progress?.Report(new ProgressInfo(100, DownloadStatus.Completed));
                    }
                    else
                    {
                        progress?.Report(new ProgressInfo(0, DownloadStatus.Failed));
                    }
                });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
