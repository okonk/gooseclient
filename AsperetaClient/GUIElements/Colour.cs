

namespace AsperetaClient
{
    class Colour
    {
        public static Colour White = new Colour(255, 255, 255, 255);
        public static Colour Black = new Colour(1, 1, 1, 255);

        public byte R { get; set; }

        public byte G { get; set; }

        public byte B { get; set; }

        public byte A { get; set; }

        public Colour(byte r, byte g, byte b, byte a = 255)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        public Colour(int r, int g, int b, int a = 255)
        {
            this.R = (byte)r;
            this.G = (byte)g;
            this.B = (byte)b;
            this.A = (byte)a;
        }
    }
}
