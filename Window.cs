using System;
using WinApi.User32;
using WinApi.Gdi32;
using WinApi.Kernel32;
#pragma warning disable CA1416
namespace FWD
{
    public class Window
    {
        private readonly IntPtr hwnd;
        private readonly IntPtr device;

        public Window()
        {
            hwnd = Kernel32Methods.GetConsoleWindow();
            User32Methods.ShowWindow(hwnd, ShowWindowCommands.SW_SHOW);
            User32Methods.UpdateWindow(hwnd);
            device = User32Methods.GetDC(hwnd);
        }

        public void show(Bitmap bitmap)
        {
            foreach (Pixel pixel in bitmap.getPixels())
            {
                Gdi32Methods.SetPixel(device, pixel.X, pixel.Y, Gdi32Helpers.Bgr32((uint)pixel.Color.R, (uint)pixel.Color.G, (uint)pixel.Color.B));
            }
            //User32Methods.ReleaseDC(hwnd, device);
        }
    }
}
