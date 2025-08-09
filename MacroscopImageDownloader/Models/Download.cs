using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MacroscopImageDownloader.Models
{
    public class Download : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private readonly Uri _uri;
        private readonly IProgress<ProgressInfo>? _progress;

        public Download(Uri uri, IProgress<ProgressInfo>? progress)
        {
            _uri = uri ?? throw new ArgumentNullException(nameof(uri));
            _progress = progress;
            ReportProgress(0, DownloadStatus.NotStarted);
        }

        private BitmapImage? _Image;

        public BitmapImage? Image
        {
            get
            {
                return _Image;
            }
            private set
            {
                if (_Image != value)
                {
                    _Image = value;
                    OnPropertyChanged(nameof(Image));
                }
            }
        }

        private BitmapImage? _ActiveLoadingImage;

        private void ReportProgress(int value, DownloadStatus status)
        {
            _progress?.Report(new ProgressInfo(value, status));
        }

        public void Start()
        {
            if (_ActiveLoadingImage != null)
                UnsubscribeImageEvents(_ActiveLoadingImage);
            BitmapImage image = new BitmapImage();
            image.BeginInit();

            SubscribeImageEvents(image);
            image.UriSource = _uri;
            image.EndInit();
            ReportProgress(0, DownloadStatus.Active);
            if (!image.IsDownloading)
            {
                ReportProgress(100, DownloadStatus.Completed);
                Image = image;
            }
            _ActiveLoadingImage = image;
        }

        private void SubscribeImageEvents(BitmapImage image)
        {
            image.DownloadCompleted += Image_DownloadCompleted;
            image.DownloadProgress += Image_DownloadProgress;
            image.DownloadFailed += Image_DownloadFailed;
            image.DecodeFailed += Image_DownloadFailed;
        }

        private void UnsubscribeImageEvents(BitmapImage image)
        {
            image.DownloadCompleted -= Image_DownloadCompleted;
            image.DownloadProgress -= Image_DownloadProgress;
            image.DownloadFailed -= Image_DownloadFailed;
            image.DecodeFailed -= Image_DownloadFailed;
        }

        private void Image_DownloadFailed(object? sender, ExceptionEventArgs e)
        {
            if (sender is BitmapImage image)
            {
                UnsubscribeImageEvents(image);
                _ActiveLoadingImage = null;
            }
        }

        private void Image_DownloadProgress(object? sender, DownloadProgressEventArgs e)
        {
            ReportProgress(e.Progress, DownloadStatus.Active);
        }

        private void Image_DownloadCompleted(object? sender, EventArgs e)
        {
            if (sender is BitmapImage image)
            {
                UnsubscribeImageEvents(image);
                ReportProgress(100, DownloadStatus.Completed);
                _ActiveLoadingImage = null;
                Image = image;
            }
        }
    }
}
