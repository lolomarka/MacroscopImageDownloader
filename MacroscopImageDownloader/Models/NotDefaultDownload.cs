using ImageMagick;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MacroscopImageDownloader.Models
{
    internal class NotDefaultDownload : BitmapDownload
    {
        private readonly MagickFormat _format;

        public NotDefaultDownload(MagickFormat format)
        {
            _format = format;
        }

        protected override Task<BitmapSource> Decode(Stream loadedStream, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
        {
            return Task.Run(() => DecodeImpl(loadedStream, progress, cancellationToken), cancellationToken);
        }

        private BitmapSource DecodeImpl(Stream loadedStream, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new ProgressInfo(92, DownloadStatus.Active));
            MagickImage originalImage = new MagickImage(loadedStream, _format);
            IMagickImage<byte> image = originalImage; 

            progress?.Report(new ProgressInfo(94, DownloadStatus.Active));
            var mapping = "RGB";
            PixelFormat format = PixelFormats.Bgr24;
            try
            {
                if (image.ColorSpace == ColorSpace.CMYK && !image.HasAlpha)
                {
                    mapping = "CMYK";
                    format = PixelFormats.Cmyk32;
                }
                else
                {
                    if (image.ColorSpace != ColorSpace.sRGB)
                    {
                        image = image.Clone();
                        image.ColorSpace = ColorSpace.sRGB;
                    }

                    if (image.HasAlpha)
                    {
                        mapping = "BGRA";
                        format = PixelFormats.Bgra32;
                    }
                }

                var bytesPerPixel = (format.BitsPerPixel + 7) / 8;
                var stride = (int)image.Width * bytesPerPixel;

                progress?.Report(new ProgressInfo(96, DownloadStatus.Active));
                using var pixels = image.GetPixelsUnsafe();
                var bytes = pixels.ToByteArray(mapping);
                progress?.Report(new ProgressInfo(98, DownloadStatus.Active));
                var bitmap = BitmapSource.Create((int)image.Width, (int)image.Height, 96, 96, format, null, bytes, stride);
                bitmap.Freeze();
                progress?.Report(new ProgressInfo(99, DownloadStatus.Active));
                return bitmap;
            }
            finally
            {
                if (!ReferenceEquals(originalImage, image))
                    image.Dispose();
            }
        }
    }
}
