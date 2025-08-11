namespace MacroscopImageDownloader.Models
{
    public enum DownloadStatus
    {
        NotStarted = 1,
        Active = 2,
        Failed = 4,
        Completed = 8,
        Cancelled = 12
    }
}
