using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace SteamDesktopAuthenticatorCore.Services
{
    public static class BitmapToBitmapImageService
    {
        public static BitmapImage BitmapToBitmapImage(in Bitmap bitmap)
        {
            using MemoryStream stream = new();
            bitmap.Save(stream, ImageFormat.Png); // Pit: When the format is Bmp, no transparency

            stream.Position = 0;
            BitmapImage result = new();
            result.BeginInit();
            // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
            // Force the bitmap to load right now so we can dispose the stream.
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();
            return result;
        }
    }
}
