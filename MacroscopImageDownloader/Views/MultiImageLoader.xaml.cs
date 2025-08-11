using MacroscopImageDownloader.Models;
using MacroscopImageDownloader.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MacroscopImageDownloader.Views
{
    public partial class ThreeImageLoader : UserControl
    {
        public const int Rows = 1;
        public const int Columns = 3;

        public ThreeImageLoader()
        {
            InitializeComponent();
            InitializeChildrenWatching(Rows * Columns);
        }

        private void InitializeChildrenWatching(int count)
        {
            var loaders = new ObservableCollection<ImageDownloader>();
            for (int i = 0; i < count; i++)
            {
                var loader = new ImageDownloader();
                loader.Progress.PropertyChanged += Loader_ProgressChanged;
                loaders.Add(loader);
            }
            ImageLoaders = loaders;
        }

        private void Loader_ProgressChanged(object? sender, PropertyChangedEventArgs e)
        {
            InvalidateProgress();
        }

        private void InvalidateProgress()
        {
            var loaders = ImageLoaders.Where(loader => (loader.Progress.Status & DownloadStatus.NotStarted & DownloadStatus.Cancelled) == 0);
            Progress = loaders.Any() ? (int)loaders.Average(loader => loader.Progress.Percent) : 0;
        }

        private static readonly DependencyPropertyKey ProgressPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Progress), typeof(int), typeof(ThreeImageLoader), new PropertyMetadata(0));

        public static readonly DependencyProperty ProgressProperty = ProgressPropertyKey.DependencyProperty;

        public IEnumerable<ImageDownloader> ImageLoaders
        {
            get { return (IEnumerable<ImageDownloader>)GetValue(ImageLoadersProperty); }
            private set { SetValue(ImageLoadersPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey ImageLoadersPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ImageLoaders), typeof(IEnumerable<ImageDownloader>), typeof(ThreeImageLoader), new PropertyMetadata());

        public static readonly DependencyProperty ImageLoadersProperty = ImageLoadersPropertyKey.DependencyProperty;

        public int Progress
        {
            get { return (int)GetValue(ProgressProperty); }
            private set { SetValue(ProgressPropertyKey, value); }
        }

        private RelayCommand _DownloadAllCommand;

        public ICommand DownloadAllCommand
        {
            get
            {
                if (_DownloadAllCommand == null)
                    _DownloadAllCommand = new RelayCommand(DownloadAllExecute, DownloadAllCanExecute);
                return _DownloadAllCommand;
            }
        }

        private void DownloadAllExecute(object? obj)
        {
            foreach (var item in ImageLoaders.Where(loader => loader.StartDownloadCommand.CanExecute(obj)))
            {
                item.StartDownloadCommand.Execute(obj);
            }
        }

        private bool DownloadAllCanExecute(object? arg)
        {
            return ImageLoaders.Any(loader => loader.StartDownloadCommand.CanExecute(arg));
        }

        private void FillAllClick(object sender, RoutedEventArgs e)
        {
            // only for test purposes
            foreach (var loader in ImageLoaders)
            {
                loader.Url = FillAllTextBox.Text;
            }
        }
    }
}
