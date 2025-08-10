using System.Net.Http;

namespace MacroscopImageDownloader.Models
{
    internal static class Http
    {
        public static readonly HttpClient Client = new HttpClient();
    }
}
