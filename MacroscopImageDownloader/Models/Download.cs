using System.ComponentModel;
using System.Drawing;
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

        public enum DownloadStatus
        {
            NotStarted = 0,
            Active = 1,
            Failed = 2,
            Completed = 4
        }

        private readonly Uri _Uri;
        private readonly Size _DesizeredSize;

        public Download(Uri uri, Size desiredSize)
        {
            _DesizeredSize = desiredSize;
            _Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            _Status = DownloadStatus.NotStarted;
        }

        private DownloadStatus _Status;

        public DownloadStatus Status
        {
            get
            {
                return _Status;
            }
            private set
            {
                if (_Status != value)
                {
                    _Status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        private int _Progress;

        public int Progress
        {
            get { return _Progress; }
            private set
            {
                if (_Progress != value)
                {
                    _Progress = value;
                    OnPropertyChanged(nameof(Progress));
                }
            }
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

        public void Start()
        {
            if (_ActiveLoadingImage != null)
                UnsubscribeImageEvents(_ActiveLoadingImage);
            BitmapImage image = new BitmapImage();
            image.BeginInit();

            if (_DesizeredSize != Size.Empty)
            {
                image.DecodePixelWidth = _DesizeredSize.Width;
                image.DecodePixelHeight = _DesizeredSize.Height;
            }

            SubscribeImageEvents(image);
            image.UriSource = _Uri;
            image.EndInit();
            Status = DownloadStatus.Active;
            if (!image.IsDownloading)
            {
                Progress = 100;
                Image = image;
                Status = DownloadStatus.Completed;
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
                Status = DownloadStatus.Failed;
                _ActiveLoadingImage = null;
            }
        }

        private void Image_DownloadProgress(object? sender, DownloadProgressEventArgs e)
        {
            Progress = e.Progress;
        }

        private void Image_DownloadCompleted(object? sender, EventArgs e)
        {
            if (sender is BitmapImage image)
            {
                UnsubscribeImageEvents(image);
                Status = DownloadStatus.Completed;
                _ActiveLoadingImage = null;
                Image = image;
            }
        }
    }
}
