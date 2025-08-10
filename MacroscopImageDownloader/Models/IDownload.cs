using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace MacroscopImageDownloader.Models
{
    public interface IDownload : INotifyPropertyChanged
    {
        BitmapSource Image { get; }

        Task StartAsync(Uri uri, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken);
    }
}