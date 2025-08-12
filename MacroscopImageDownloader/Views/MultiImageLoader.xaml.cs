using MacroscopImageDownloader.Models;
using MacroscopImageDownloader.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MacroscopImageDownloader.Views
{
    public partial class MultiImageLoader : UserControl
    {
        public MultiImageLoader()
        {
            InitializeComponent();
#if TEST
            TestPanel.Visibility = Visibility.Visible;
#endif
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
            DependencyProperty.RegisterReadOnly(nameof(Progress), typeof(int), typeof(MultiImageLoader), new PropertyMetadata(0));

        public static readonly DependencyProperty ProgressProperty = ProgressPropertyKey.DependencyProperty;

        public IEnumerable<ImageDownloader> ImageLoaders
        {
            get { return (IEnumerable<ImageDownloader>)GetValue(ImageLoadersProperty); }
            private set { SetValue(ImageLoadersPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey ImageLoadersPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ImageLoaders), typeof(IEnumerable<ImageDownloader>), typeof(MultiImageLoader), new PropertyMetadata());

        public static readonly DependencyProperty ImageLoadersProperty = ImageLoadersPropertyKey.DependencyProperty;

        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register(nameof(Rows), typeof(int), typeof(MultiImageLoader), new PropertyMetadata(1));

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(int), typeof(MultiImageLoader), new PropertyMetadata(3));

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
            return ImageLoaders?.Any(loader => loader.StartDownloadCommand.CanExecute(arg)) ?? false;
        }

#if TEST

        private RelayCommand _FillAllCommand;

        public ICommand FillAllCommand
        {
            get
            {
                if (_FillAllCommand == null)
                    _FillAllCommand = new RelayCommand(FillAll);
                return _FillAllCommand;
            }
        }

        private void FillAll(object? arg)
        {
            // only for test purposes
            foreach (var loader in ImageLoaders)
            {
                loader.Url = FillAllTextBox.Text;
            }
        }

#endif

        private void MultiImageLoader_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeChildrenWatching(Rows * Columns);
        }
    }
}
