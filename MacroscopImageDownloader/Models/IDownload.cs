using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace MacroscopImageDownloader.Models
{
    public interface IDownload : INotifyPropertyChanged
    {
        BitmapSource? Image { get; }

        void Start(Uri uri, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken);
    }
}