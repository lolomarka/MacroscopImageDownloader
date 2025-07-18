using MacroscopImageDownloader.Models;
using MacroscopImageDownloader.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MacroscopImageDownloader.Views
{
    public partial class ThreeImageLoader : UserControl
    {
        private IEnumerable<ImageDownloader> _ImageLoaders;

        public ThreeImageLoader()
        {
            InitializeComponent();
            InitializeChildrenWatching();
        }

        private void InitializeChildrenWatching()
        {
            _ImageLoaders = LoadersGrid.Children.OfType<ImageLoaderView>()
                                                .Select(view => view.DataContext)
                                                .OfType<ImageDownloader>().ToArray();
            foreach (var loader in _ImageLoaders)
            {
                loader.ProgressChanged += Loader_ProgressChanged;
            }
        }

        private void Loader_ProgressChanged(object? sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            InvalidateProgress();
        }

        private void InvalidateProgress()
        {
            var displayableLoaders = _ImageLoaders.Where(loader => loader.Download != null
                                                     && (loader.Download.Status == Download.DownloadStatus.Active
                                                     || loader.Download.Status == Download.DownloadStatus.Completed));

            Progress = displayableLoaders.Any() ? (int)displayableLoaders.Average(loader => loader.Download.Progress) : 0;
        }

        private static readonly DependencyPropertyKey ProgressPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Progress), typeof(int), typeof(ThreeImageLoader), new PropertyMetadata(0));

        public static readonly DependencyProperty ProgressProperty = ProgressPropertyKey.DependencyProperty;

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
            foreach (var item in _ImageLoaders.Where(loader => loader.StartDownloadCommand.CanExecute(obj)))
            {
                item.StartDownloadCommand.Execute(obj);
            }
        }

        private bool DownloadAllCanExecute(object? arg)
        {
            return _ImageLoaders.Any(loader => loader.StartDownloadCommand.CanExecute(arg));
        }
    }
}
