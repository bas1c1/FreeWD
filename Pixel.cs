#pragma warning disable CA1416
namespace FWD
{
    public class Pixel
    {
        private int x, y;
        private Color color;

        public Pixel(int x, int y, int r, int g, int b, int a)
        {
            this.x = x;
            this.y = y;
            color = new Color(r, g, b, a);
        }

        public Pixel(int x, int y, Color color)
        {
            this.x = x;
            this.y = y;
            this.color = color;
        }

        public byte[] asByte()
        {
            return new byte[4]
            {
                (byte)color.B,
                (byte)color.G,
                (byte)color.R,
                (byte)color.A
            };
        }

        public static Pixel fromByte(byte[] byt)
        {
            return new Pixel(0, 0, byt[0], byt[1], byt[2], byt[3]);
        }

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public Color Color { get => color; set => color = value; }
    }
}
