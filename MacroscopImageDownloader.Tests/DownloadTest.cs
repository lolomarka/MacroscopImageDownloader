using MacroscopImageDownloader.Models;
using System.ComponentModel;

namespace MacroscopImageDownloaderTests
{
    [TestClass]
    public sealed class DownloadTest
    {
        private readonly Uri _TestUri = new Uri("https://4kwallpapers.com/images/wallpapers/assassins-creed-12000x6871-16786.jpeg");

        [TestMethod]
        public void Constructor_ValidUri_SetsUriAndInitialStatus()
        {
            // Arrange & Act
            var download = new Download(_TestUri);

            // Assert
            Assert.AreEqual(_TestUri, download.GetType().GetField("_Uri", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(download));
            Assert.AreEqual(Download.DownloadStatus.NotStarted, download.Status);
            Assert.AreEqual(0, download.Progress);
        }

        [TestMethod]
        public void Constructor_NullUri_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new Download(null));
        }

        [TestMethod]
        public void Start_WhenNotStarted_InitializesBackgroundWorker()
        {
            // Arrange
            var download = new Download(_TestUri);

            // Act
            download.Start();

            // Assert
            var worker = (BackgroundWorker)download.GetType().GetField("_DownloadWorker", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(download);
            Assert.IsNotNull(worker);
            Assert.IsTrue(worker.WorkerReportsProgress);
            Assert.IsTrue(worker.WorkerSupportsCancellation);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Start_WhenAlreadyStarted_ThrowsInvalidOperationException()
        {
            // Arrange
            var download = new Download(_TestUri);
            download.Start();

            // Act
            download.Start();

            // Assert: Expect InvalidOperationException
        }
    }
}
