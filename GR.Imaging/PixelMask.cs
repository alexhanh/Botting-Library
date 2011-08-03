using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GR.Imaging
{
	public class PixelMask
	{
		private int color;
		private Point location;

        public int Color { get { return color; } }
		//public int ColorArgb { get { return color; } }
		public Point Location { get { return location; } set { location = value; } }
		public int X { get { return location.X; } set { location.X = value; } }
		public int Y { get { return location.Y; } set { location.Y = value; } }

		public PixelMask(Point location, Color color)
		{
			this.location = location;
			this.color = color.ToArgb();
		}

        public PixelMask(int x, int y, int argb)
        {
            this.location = new Point(x, y);
            this.color = argb;
        }

        public PixelMask(int x, int y, Color color)
        {
            this.location = new Point(x, y);
            this.color = color.ToArgb();
        }

        public PixelMask(int x, int y, int r, int g, int b)
        {
            this.location = new Point(x, y);
            this.color = System.Drawing.Color.FromArgb(r, g, b).ToArgb();
        }

		public bool IsMatch(FastBitmap bitmap)
		{
			return bitmap.GetPixel(X, Y) == color;
		}

		public bool IsMatch(Bitmap bitmap)
		{
			return bitmap.GetPixel(X, Y).ToArgb() == color;
		}

		public bool IsMatch(FastBitmap bitmap, int tolerance)
		{
			Color c1 = bitmap.GetColor(X, Y);
			Color c2 = this.ToColor();

			if (Math.Abs(c1.R - c2.R) > tolerance || Math.Abs(c1.G - c2.G) > tolerance || Math.Abs(c1.B - c2.B) > tolerance)
				return false;

			return true;
		}
		
        public Color ToColor()
        {
            return System.Drawing.Color.FromArgb(color);
        }

        public void Offset(Point p) { location.Offset(p); }

		// "x,y hex_color", hex_color in format #ff00ff
		public static PixelMask FromPosHexString(string s)
		{
			string[] args = s.Split(new char[] { ' ' });
			string[] coords = args[0].Split(new char[] { ',' });

			Point position = new Point(int.Parse(coords[0]), int.Parse(coords[1]));
			Color color = ColorTranslator.FromHtml(args[1]);

			return new PixelMask(position, color);
		}
	}
}
