namespace MacroscopImageDownloader.Models
{
    public record ProgressInfo(int ProgressPercent, string? Status)
    {
        public ProgressInfo(int progressPercent, DownloadStatus status) : this(progressPercent, status.ToString())
        {
        }
    }
}
