using MacroscopImageDownloader.Models;
using System.ComponentModel;
using System.Windows.Input;

namespace MacroscopImageDownloader.ViewModels
{
    public class ImageDownloader : INotifyPropertyChanged, IDisposable
    {
        ~ImageDownloader()
        {
            DisposeImpl();
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private string _Url = string.Empty;

        public string Url
        {
            get { return _Url; }
            set
            {
                if (_Url != value)
                {
                    _Url = value;
                    OnPropertyChanged(nameof(Url));
                }
            }
        }

        private RelayCommand? _StartDownloadCommand;

        public ICommand StartDownloadCommand
        {
            get
            {
                if (_StartDownloadCommand == null)
                    _StartDownloadCommand = new RelayCommand(StartDownload, CanStartDownload);
                return _StartDownloadCommand;
            }
        }

        private RelayCommand? _StopDownloadCommand;

        public ICommand StopDownloadCommand
        {
            get
            {
                if (_StopDownloadCommand == null)
                    _StopDownloadCommand = new RelayCommand(StopDownload, CanStopDownload);
                return _StopDownloadCommand;
            }
        }

        private IDownload? _Download;

        public IDownload? Download
        {
            get
            {
                return _Download;
            }
            private set
            {
                if (_Download != value)
                {
                    _Download = value;
                    OnPropertyChanged(nameof(Download));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private Progress? _progress;

        public Progress Progress
        {
            get
            {
                if (_progress == null)
                {
                    _progress = new Progress() { Percent = 0, Status = DownloadStatus.NotStarted };
                }
                return _progress;
            }
        }

        private CancellationTokenSource? _cancellationTokenSource;

        private void Cancel()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        private CancellationToken GetNewCancellationToken()
        {
            Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            return _cancellationTokenSource.Token;
        }

        private static IDownload CreateDownload(string url)
        {
            ArgumentNullException.ThrowIfNull(url, nameof(url));
            if (url.EndsWith(".heic"))
                return new NotDefaultDownload(ImageMagick.MagickFormat.Heic);
            if (url.EndsWith(".heif"))
                return new NotDefaultDownload(ImageMagick.MagickFormat.Heif);
            if (url.EndsWith(".tif"))
                return new NotDefaultDownload(ImageMagick.MagickFormat.Tif);
            return new BitmapDownload();
        }

        private void StartDownload(object? obj)
        {
            if (Url.IsImageUrl())
            {
                DisposeDownload();
                Download = CreateDownload(Url);
                Download.Start(new Uri(Url), new Progress<ProgressInfo>(info =>
                {
                    Progress.Percent = info.ProgressPercent;
                    Progress.Status = info.Status;
                    CommandManager.InvalidateRequerySuggested();
                }), GetNewCancellationToken());
            }
        }

        private bool CanStartDownload(object? arg)
        {
            return ((Download == null && (Url?.IsImageUrl() ?? false)) && (Progress.Status & DownloadStatus.Active) == 0) || Download?.Image != null;
        }

        private bool CanStopDownload(object? arg)
        {
            return Download != null && (Progress?.Status & DownloadStatus.Active) > 0;
        }

        private void StopDownload(object? obj)
        {
            Cancel();
            DisposeDownload();
        }

        private void DisposeDownload()
        {
            var download = Download;
            Download = null;
            if (download is IDisposable disposableDownload)
                disposableDownload.Dispose();
        }

        private bool disposed;

        private void DisposeImpl()
        {
            if (!disposed)
            {
                DisposeDownload();
                disposed = true;
            }
        }


        public void Dispose()
        {
            DisposeImpl();
            GC.SuppressFinalize(this);
        }
    }
}
