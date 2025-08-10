using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WIC;

namespace MacroscopImageDownloader.Models
{
    public class WICPixelFormatMapping
    {
        private readonly static WICPixelFormatMapping _instance = new WICPixelFormatMapping();

        public static WICPixelFormatMapping Instance => _instance;

        private readonly Dictionary<Guid, PixelFormat> _mapping;

        private WICPixelFormatMapping() 
        {
            _mapping = new Dictionary<Guid, PixelFormat>()
            {
                {WICPixelFormat.WICPixelFormat24bppBGR, PixelFormats.Bgr24 },
                {WICPixelFormat.WICPixelFormat32bppBGR, PixelFormats.Bgr32 },
                {WICPixelFormat.WICPixelFormat32bppBGR101010, PixelFormats.Bgr101010 },
                {WICPixelFormat.WICPixelFormat16bppBGR555, PixelFormats.Bgr555},
                {WICPixelFormat.WICPixelFormat32bppBGRA, PixelFormats.Bgra32 },
                {WICPixelFormat.WICPixelFormat24bppRGB, PixelFormats.Rgb24 },
                {WICPixelFormat.WICPixelFormat48bppRGB, PixelFormats.Rgb48},
                {WICPixelFormat.WICPixelFormat64bppRGBA, PixelFormats.Rgba64},
                {WICPixelFormat.WICPixelFormat128bppRGBAFloat, PixelFormats.Rgba128Float},
                {WICPixelFormat.WICPixelFormat2bppGray, PixelFormats.Gray2},
                {WICPixelFormat.WICPixelFormat4bppGray, PixelFormats.Gray4},
                {WICPixelFormat.WICPixelFormat8bppGray, PixelFormats.Gray8},
                {WICPixelFormat.WICPixelFormat16bppGray, PixelFormats.Gray16},
                {WICPixelFormat.WICPixelFormat32bppGrayFloat, PixelFormats.Gray32Float },
                {WICPixelFormat.WICPixelFormat32bppCMYK, PixelFormats.Cmyk32},
                {WICPixelFormat.WICPixelFormat32bppPBGRA, PixelFormats.Pbgra32},
                {WICPixelFormat.WICPixelFormat1bppIndexed, PixelFormats.Indexed1},
                {WICPixelFormat.WICPixelFormat2bppIndexed, PixelFormats.Indexed2},
                {WICPixelFormat.WICPixelFormat4bppIndexed, PixelFormats.Indexed4},
                {WICPixelFormat.WICPixelFormat8bppIndexed, PixelFormats.Indexed8},
                {WICPixelFormat.WICPixelFormatBlackWhite, PixelFormats.BlackWhite},
                {WICPixelFormat.WICPixelFormatDontCare, PixelFormats.Default},
            };
        }

        public PixelFormat GetPixelFormat(Guid guid)
        {
            return _mapping[guid];
        }

        public Guid GetGuid(PixelFormat format)
        {
            return _mapping.First(kvp => kvp.Value == format).Key;
        }
    }
}
