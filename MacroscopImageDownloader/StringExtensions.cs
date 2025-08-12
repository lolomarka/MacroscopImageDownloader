namespace MacroscopImageDownloader
{
    internal static class StringExtensions
    {
        public static bool IsImageUrl(this string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                return false;
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".wmp", ".tiff", ".ico", ".heic", ".heif" };

            string path = uriResult.AbsolutePath.ToLower();
            return imageExtensions.Any(path.EndsWith);
        }
    }
}
