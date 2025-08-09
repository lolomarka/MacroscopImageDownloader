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
                    _Download = value;
                    OnPropertyChanged(nameof(Download));
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
                    _progress = new Progress() { Percent = 0, Status = DownloadStatus.NotStarted.ToString() };
                }
                return _progress;
            }
        }

        private void StartDownload(object? obj)
        {
            if (Url.IsImageUrl())
            {
                Download = new Download(new Uri(Url), new Progress<ProgressInfo>(info =>
                {
                    Progress.Percent = info.ProgressPercent;
                    Progress.Status = info.Status;
                }));
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
            
        }
    }
}
