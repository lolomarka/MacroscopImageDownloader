using MacroscopImageDownloader.Models;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Input;

namespace MacroscopImageDownloader.ViewModels
{
    public class ImageDownloader : INotifyPropertyChanged
    {
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

        private Download? _Download;

        public Download? Download
        {
            get
            {
                return _Download;
            }
            private set
            {
                if (_Download != value)
                {
                    if (_Download != null)
                    {
                        _Download.PropertyChanged -= DownLoadPropertyChanged;
                    }
                    _Download = value;
                    if (_Download != null)
                    {
                        _Download.PropertyChanged += DownLoadPropertyChanged;
                    }
                    OnPropertyChanged(nameof(Download));
                }
            }
        }

        private void DownLoadPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is Download download &&
                e.PropertyName == nameof(Download.Progress))
            {
                OnProgressChanged(download.Progress);
            }
        }

        public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;

        private void OnProgressChanged(int progress)
        {
            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(progress, Download?.Status ?? Download.DownloadStatus.NotStarted));
        }

        private void StartDownload(object? obj)
        {
            if (Url.IsImageUrl())
            {
                Size desiredSize = obj is Size size ? size : Size.Empty;
                Download = new Download(new Uri(Url), desiredSize);
                Download.Start();
            }
        }

        private bool CanStartDownload(object? arg)
        {
            return Download == null && (Url?.IsImageUrl() ?? false);
        }

        private bool CanStopDownload(object? arg)
        {
            return Download != null;
        }

        private void StopDownload(object? obj)
        {
            Download = null;
            OnProgressChanged(0);
        }
    }
}
