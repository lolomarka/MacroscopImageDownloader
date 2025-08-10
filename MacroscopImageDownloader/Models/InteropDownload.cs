using System.Buffers;
using System.ComponentModel;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WIC;

namespace MacroscopImageDownloader.Models
{
    public class InteropDownload : IDownload, IDisposable
    {
        private const int BufferSize = 1024 * 10;
        private bool _disposed;

        ~InteropDownload()
        {
            DisposeImpl(false);
        }

        private MemoryMappedFile? _MemoryMappedFile;

        private BitmapSource? _image;

        public BitmapSource? Image
        {
            get => _image;
            private set
            {
                if (_image != value)
                {
                    _image = value;
                    OnPropertyChanged();
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string paramName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(paramName));

        private void DisposeImpl(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;
            DisposeMemoryMappedFile();

            if (disposing)
                GC.SuppressFinalize(this);
        }

        private void DisposeMemoryMappedFile()
        {
            _MemoryMappedFile?.Dispose();
            _MemoryMappedFile = null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;


        public async Task StartAsync(Uri uri, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(InteropDownload));

            Image = await DownloadAsync(uri, progress, cancellationToken);
        }

        private async Task<BitmapSource?> DownloadAsync(Uri uri, IProgress<ProgressInfo>? progress, CancellationToken token)
        {
            Stream? stream = null;
            try
            {
                using (var response = await Http.Client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token))
                {
                    response.EnsureSuccessStatusCode();

                    if (!response.Content.Headers.ContentLength.HasValue)
                    {
                        progress?.Report(new ProgressInfo(0, DownloadStatus.Failed));
                        return null;
                    }
                    var contentLength = response.Content.Headers.ContentLength.Value;

                    using (var responseStream = await response.Content.ReadAsStreamAsync(token))
                    {
                        stream = new MemoryStream();
                        byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
                        try
                        {
                            long i = 0;
                            do
                            {
                                token.ThrowIfCancellationRequested();
                                var count = (int)Math.Min((long)BufferSize, (long)(contentLength - i));
                                int read = await responseStream.ReadAsync(buffer, 0, count, token);
                                if (read == 0)
                                    break;
                                await stream.WriteAsync(buffer, 0, read, token);
                                i += read;
                                progress?.Report(new ProgressInfo((int)((100 * i) / contentLength), DownloadStatus.Active));
                            } while (i < contentLength);
                        }
                        catch (OperationCanceledException)
                        {
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(buffer);
                        }
                    }
                }
                if (token.IsCancellationRequested)
                {
                    progress?.Report(new ProgressInfo(0, DownloadStatus.Canceled));
                    return null;
                }
                DecodeWithWIC(stream, out byte[] pixels, out int pixelWidth, out int pixelHeight, out PixelFormat pixelFormat, out int stride, out int byteCount);

                _MemoryMappedFile?.Dispose();
                _MemoryMappedFile = MemoryMappedFile.CreateNew(null, byteCount);
                using (var accessor = _MemoryMappedFile.CreateViewAccessor())
                {
                    accessor.WriteArray<byte>(0, pixels, 0, byteCount);
                }

                var handle = _MemoryMappedFile.SafeMemoryMappedFileHandle.DangerousGetHandle();
                var interopBitmap = Imaging.CreateBitmapSourceFromMemorySection(
                    handle,
                    pixelWidth,
                    pixelHeight,
                    pixelFormat,
                    stride,
                    0);
                interopBitmap.Freeze();
                return interopBitmap;
            }
            finally
            {
                stream?.Dispose();
            }
        }

        private void DecodeWithWIC(Stream inputStream,
                                   out byte[] pixels,
                                   out int pixelWidth,
                                   out int pixelHeight,
                                   out PixelFormat pixelFormat,
                                   out int stride,
                                   out int byteCount)
        {
            IWICImagingFactory factory = WICImagingFactory.Create();
            IWICStream stream = factory.CreateStream();
            stream.InitializeFromIStream(inputStream.AsCOMStream());
            IWICBitmapDecoder decoder = factory.CreateDecoderFromStream(stream, nint.Zero, WICDecodeOptions.WICDecodeMetadataCacheOnDemand);
            IWICBitmapFrameDecode frame = decoder.GetFrame(0);
            frame.GetSize(out pixelWidth, out pixelHeight);

            pixelFormat = WICPixelFormatMapping.Instance.GetPixelFormat(frame.GetPixelFormat());
            pixels = frame.GetPixels();
            stride = pixelWidth * ((pixelFormat.BitsPerPixel + 7) / 8);
            byteCount = pixels.Length;
        }

        public void Dispose()
        {
            DisposeImpl(true);
        }
    }
}
