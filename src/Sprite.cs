#pragma warning disable CA1416
namespace FWD
{
    public class Sprite
    {
        private string name;
        private int x, y;
        private Bitmap sprite;

        public Sprite(int x, int y, Bitmap sprite, string name = "")
        {
            this.x = x;
            this.y = y;
            this.sprite = sprite;
            this.name = name;
        }

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public Bitmap _Sprite { get => sprite; set => sprite = value; }
        public string Name { get => name; set => name = value; }

        /*public static Sprite fromFile(string name, int x, int y)
        {
            System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(name);
            return new Sprite(x, y, Bitmap.fromGdiBitmap(bitmap), name);
        }*/
    }
}
