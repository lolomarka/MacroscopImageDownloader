using System.Configuration;
using System.Data;
using System.Windows;
using MacroscopImageDownloader.Models;

namespace MacroscopImageDownloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Http.Client.Dispose();
        }
    }
}
