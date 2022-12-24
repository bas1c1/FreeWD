using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinApi.User32;
using WinApi.Gdi32;
using WinApi.Kernel32;
using System.Windows;
#pragma warning disable CA1416
namespace FWD
{
    public class Bitmap
    {
        private byte[] bytepixels;
        private List<Pixel> pixels = new List<Pixel>();
        private int height, width;
        private bool isDrawingMode = false;
        private bool isNeedSave = true;

        private IntPtr hwnd = new IntPtr();
        private IntPtr device = new IntPtr();

        public int Height { get => height; set => height = value; }
        public int Width { get => width; set => width = value; }
        public bool IsNeedSave { get => isNeedSave; set => isNeedSave = value; }

        public Bitmap(int height, int width)
        {
            this.height = height;
            this.width = width;
            
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    pixels.Add(new Pixel(i, j, 0, 0, 0, 255));
                }
            }
        }

        public Bitmap(List<Pixel> pixels)
        {
            this.pixels = pixels;
        }

        public static Bitmap fromGdiBitmap(System.Drawing.Bitmap bmp)
        {
            Bitmap bitmap = new Bitmap(bmp.Height, bmp.Width);
            byte[] bytes = new byte[((bmp.Width)*(bmp.Height))*4];
            BitmapSource bmps = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                  bmp.GetHbitmap(),
                  IntPtr.Zero,
                  Int32Rect.Empty,
                  BitmapSizeOptions.FromEmptyOptions());
            bmps.CopyPixels(Int32Rect.Empty, bytes, 4*bmp.Width, 0);
            bitmap.clear();
            int count = 0;
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color color = new Color(0, 0, 0, 0);
                    for (; count < bytes.Length; )
                    {
                        color.B = bytes[count];
                        count++;
                        color.G = bytes[count];
                        count++;
                        color.R = bytes[count];
                        count++;
                        color.A = bytes[count];
                        count++;
                        break;
                    }
                    bitmap.addPixel(i, j, color);
                }
            }
            return bitmap;
        }

        public System.Drawing.Bitmap asGdiBitmap()
        {
            var source = BitmapSource.Create(width, height, 96d, 96d,
                                              PixelFormats.Bgra32, null, asByteArray(), width*4);
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(
                source.PixelWidth,
                source.PixelHeight,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(
                new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(
                System.Windows.Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        public byte[] asByteArray()
        {
            List<byte> vs = new List<byte>(height*width);
            foreach (Pixel pixel in pixels)
            {
                foreach(byte byt in pixel.asByte())
                {
                    vs.Add(byt);
                }
            }
            bytepixels = vs.ToArray();
            return bytepixels;
        }

        public List<Pixel> getPixels()
        {
            return pixels;
        }

        public void fill(Color color)
        {
            if (isNeedSave)
            {
                pixels.Clear();
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        pixels.Add(new Pixel(i, j, color));
                    }
                }
            }
            if (isDrawingMode)
            {
                User32Methods.UpdateWindow(hwnd);
                IntPtr brush = Gdi32Methods.CreateSolidBrush(Gdi32Helpers.Bgr32((uint)color.R, (uint)color.G, (uint)color.B));
                IntPtr thisreg = Gdi32Methods.CreateRectRgn(0, 0, width, height);
                Gdi32Methods.FillRgn(device, thisreg, brush);
            }
        }

        public void fillReg(Color color, int left, int top, int right, int bottom)
        {
            if (isNeedSave)
            {
                for (int i = left; i < right; i++)
                {
                    for (int j = top; j < bottom; j++)
                    {
                        setPixel(i, j, color);
                    }
                }
            }
            if (isDrawingMode)
            {
                IntPtr brush = Gdi32Methods.CreateSolidBrush(Gdi32Helpers.Bgr32((uint)color.R, (uint)color.G, (uint)color.B));
                IntPtr thisreg = Gdi32Methods.CreateRectRgn(left, top, right, bottom);
                Gdi32Methods.FillRgn(device, thisreg, brush);
            }
        }

        public Pixel getPixel(int x, int y)
        {
            return pixels[getPixelIndex(x, y)];
        }

        public void addPixel(int x, int y, Color color)
        {
            pixels.Add(new Pixel(x, y, color));
        }

        public void clear()
        {
            pixels.Clear();
        }

        public void setPixel(int x, int y, Color color)
        {
            if (isNeedSave)
            {
                int index = getPixelIndex(x, y);
                pixels[index] = new Pixel(x, y, color);
            }
            if (isDrawingMode)
            {
                Gdi32Methods.SetPixel(device, x, y, Gdi32Helpers.Bgr32((uint)color.R, (uint)color.G, (uint)color.B));
            }
        }

        public void save(string filename)
        {
            FWDIO.writeImage(filename, this);
        }

        public void init_drawing()
        {
            hwnd = Kernel32Methods.GetConsoleWindow();
            device = User32Methods.GetDC(hwnd);
            isDrawingMode = true;
        }

        public static Bitmap fromFile(string name)
        {
            using (FileStream fs = new FileStream(name, FileMode.Open, FileAccess.Read))
            {
                fs.Position = 0;
                using (System.Drawing.Image original = System.Drawing.Image.FromStream(fs))
                {
                    return fromGdiBitmap((System.Drawing.Bitmap)original);
                }
            }
        }

        public void spriteLoad(Sprite sprite)
        {
            if (isNeedSave)
            {
                for (int i = sprite.X; i < sprite._Sprite.Height; i++)
                {
                    for (int j = sprite.Y; j < sprite._Sprite.Width; j++)
                    {
                        setPixel(i, j, sprite._Sprite.getPixel(i, j).Color);
                    }
                }
            }
            if (isDrawingMode)
            {
                //USING GDI+ FOR DRAWING IMAGES!!!!
                System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(hwnd);
                graphics.DrawImage(sprite._Sprite.asGdiBitmap(), sprite.X, sprite.Y);
            }
        }

        public int getPixelIndex(int x, int y)
        {
            int i = 0;
            foreach (Pixel pixel in pixels)
            {
                if (pixel.X == x && pixel.Y == y) break;
                i++;
            }
            return i;
        }
    }
}
