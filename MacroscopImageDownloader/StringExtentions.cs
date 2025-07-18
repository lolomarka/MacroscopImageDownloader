using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroscopImageDownloader
{
    internal static class StringExtentions
    {
        public static bool IsImageUrl(this string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                return false;

            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };

            string path = uriResult.AbsolutePath.ToLower();
            return imageExtensions.Any(ext => path.EndsWith(ext));
        }
    }
}
