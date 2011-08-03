using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GR.Imaging
{
	public class BitmapMask
	{
		private FastBitmap mask;
		private Point location;

		public BitmapMask(FastBitmap mask, Point location)
		{
			this.mask = mask;
			this.location = location;
		}

		public BitmapMask(FastBitmap mask, int x, int y)
		{
			this.mask = mask;
			this.location = new Point(x, y);
		}

		public BitmapMask(Bitmap mask, Point location)
		{
			this.mask = new FastBitmap(mask);
			this.location = location;
		}

		public BitmapMask(Bitmap mask, int x, int y)
		{
			this.mask = new FastBitmap(mask);
			this.location = new Point(x, y);
		}

		public bool IsMatch(FastBitmap bitmap)
		{
			for (int y = 0; y < mask.Height; y++)
			{
				for (int x = 0; x < mask.Width; x++)
				{
					if (mask.GetPixel(x, y) != bitmap.GetPixel(x + location.X, y + location.Y))
						return false;
				}
			}

			return true;
		}

		public bool IsIn(FastBitmap bitmap, int tolerance)
		{
			return this.mask.IsIn(bitmap, this.location, tolerance);
		}
	}
}
