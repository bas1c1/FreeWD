using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#pragma warning disable CA1416
namespace FWD
{
    public class FWDIO
    {
        public static byte[] readImage(string name)
        {
            Stream imageStreamSource = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read);
            PngBitmapDecoder decoder = new PngBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            BitmapSource bitmapSource = decoder.Frames[0];
            byte[] bytepixels = new byte[bitmapSource.PixelWidth * bitmapSource.PixelWidth];
            bitmapSource.CopyPixels(bytepixels, bitmapSource.PixelWidth, 0);
            return bytepixels;
        }

        public static void writeImage(string name, Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            int stride;
            byte[] pixels = bitmap.asByteArray();
            var dpiX = 96d;
            var dpiY = 96d;
            var pixelFormat = PixelFormats.Bgra32;
            stride = 4 * width;

            var image = BitmapSource.Create(width, height, dpiX, dpiY,
                                             pixelFormat, null, pixels, stride);

            FileStream stream = new FileStream(name, FileMode.Create);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Interlace = PngInterlaceOption.On;
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.Save(stream);
            stream.Close();
        }
    }
}
