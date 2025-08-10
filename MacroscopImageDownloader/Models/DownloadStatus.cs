namespace MacroscopImageDownloader.Models
{
    public enum DownloadStatus
    {
        NotStarted = 0,
        Active = 1,
        Failed = 2,
        Completed = 4,
        Cancelled = 6
    }
}
