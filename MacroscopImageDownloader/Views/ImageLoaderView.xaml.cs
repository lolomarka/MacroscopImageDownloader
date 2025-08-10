using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MacroscopImageDownloader.Views
{
    /// <summary>
    /// Interaction logic for ImageLoaderView.xaml
    /// </summary>
    public partial class ImageLoaderView : UserControl
    {
        public ImageLoaderView()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == DataContextProperty)
            {
                if (e.OldValue is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
