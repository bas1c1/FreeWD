using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinApi.User32;
using WinApi.Gdi32;

namespace FWD
{
    public static class Engine2D
    {
        
        public class Keyboard //NOT READY!!!!
        {
            
            private static VirtualKey key;

            public static KeyState getKey(VirtualKey key)
            {
                return User32Methods.GetKeyState(key);
            }
        }

        public class Screen
        {
            private int sizeX, sizeY;
            private Bitmap bitmap;

            public Screen(Bitmap bitmap)
            {
                this.bitmap = bitmap;
                sizeX = bitmap.Width;
                sizeY = bitmap.Height;
            }

            public Bitmap Bitmap { get => bitmap; }
            public int SizeX { get => sizeX; }
            public int SizeY { get => sizeY; }

            public void fill(Window window, Color color)
            {
                IntPtr dc = User32Methods.GetDC(window.Hwnd);
                if (dc == IntPtr.Zero || window.Hwnd == IntPtr.Zero)
                {
                    return;
                }
                IntPtr brush = Gdi32Methods.CreateSolidBrush(Gdi32Helpers.Bgr32((uint)color.R, (uint)color.G, (uint)color.B));
                IntPtr thisreg = Gdi32Methods.CreateRectRgn(0, 0, bitmap.Width, bitmap.Height);
                Gdi32Methods.FillRgn(User32Methods.GetDC(window.Hwnd), thisreg, brush);
                Gdi32Methods.DeleteObject(thisreg);
                Gdi32Methods.DeleteObject(brush);
                User32Methods.ReleaseDC(window.Hwnd, dc);
            }

            public void fillReg(Window window, Color color, int left, int top, int right, int bottom)
            {
                IntPtr dc = User32Methods.GetDC(window.Hwnd);
                if (dc == IntPtr.Zero || window.Hwnd == IntPtr.Zero)
                {
                    return;
                }
                IntPtr brush = Gdi32Methods.CreateSolidBrush(Gdi32Helpers.Bgr32((uint)color.R, (uint)color.G, (uint)color.B));
                IntPtr thisreg = Gdi32Methods.CreateRectRgn(left, top, right, bottom);
                Gdi32Methods.FillRgn(User32Methods.GetDC(window.Hwnd), thisreg, brush);
                Gdi32Methods.DeleteObject(thisreg);
                Gdi32Methods.DeleteObject(brush);
                User32Methods.ReleaseDC(window.Hwnd, dc);
            }

            public void Split(int x1, int y1, int x2, int y2)
            {
                foreach (Pixel pixel in bitmap.Pixels)
                {
                    if (!(pixel.X >= x1 && pixel.Y >= y1 && pixel.X <= x2 && pixel.Y <= y2))
                    {
                        bitmap.Pixels.Remove(pixel);
                    }
                }
            }
        }

        public class Layer
        {
            private Draw drawf;
            private Screen screen;

            public Layer(Draw drawf, Screen screen)
            {
                this.drawf = drawf;
                this.screen = screen;
            }

            public Draw Drawf { get => drawf; set => drawf = value; }
            public Screen Screen { get => screen; set => screen = value; }

            public void draw(Window window)
            {
                drawf(window);
            }
        }

        public delegate void Draw(Window window);
        public delegate void Loop();

        public class Window
        {
            public delegate void windowloop(Window window);

            private int x, y, height, width;
            private string name;

            private Dictionary<string, Layer> layers = new Dictionary<string, Layer>();
            private windowloop mainloop;
            private IntPtr hwnd;

            public IntPtr Hwnd { get => hwnd; set => hwnd = value; }
            public Dictionary<string, Layer> Layers { get => layers; set => layers = value; }
            public int X { get => x; }
            public int Y { get => y; }
            public int Height { get => height; }
            public int Width { get => width; }
            public string Name { get => name; set => name = value; }

            public Window(string name, int height, int width, windowloop mainloop, int x=0, int y=0)
            {
                this.name = name;
                this.x = x;
                this.y = y;
                this.height = height;
                this.width = width;
                this.mainloop = mainloop;
            }

            public void createWindow()
            {
                new Thread(() => { for(; ; ) mainloop(this); }) { IsBackground = true }.Start();
            }

            public void Show()
            {
                User32Methods.ShowWindow(hwnd, ShowWindowCommands.SW_SHOWNORMAL);
            }

            public void Update()
            {
                User32Methods.UpdateWindow(hwnd);
            }

            public void Close()
            {
                User32Methods.CloseWindow(hwnd);
            }

            public void AddLayer(string lname, Layer layer)
            {
                layers[lname] = layer;
            }

            public void AddLoop(Loop loop)
            {
                new Thread(() => { for (; ; ) loop(); }) { IsBackground = true }.Start();
            }

            public void DrawLayers()
            {
                foreach (KeyValuePair<string, Layer> screen in layers)
                {
                    new Thread(() =>
                    {
                        screen.Value.draw(this);
                    })
                    { IsBackground = true }.Start();
                }
            }

            public void DrawLayer(string layer)
            {
                new Thread(() =>
                {
                    layers[layer].draw(this);
                })
                { IsBackground = true }.Start();
            }
        }
    }
}
