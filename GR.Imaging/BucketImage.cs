using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GR.Imaging
{
	public class BucketImage
	{
		private int[,] map;
		private int buckets;

		public BucketImage(FastBitmap bitmap, int buckets)
		{
			map = new int[bitmap.Width, bitmap.Height];

			this.buckets = buckets;

			int bucket_width = 255 / buckets;

			for (int y = 0; y<bitmap.Height; y++)
			{
				for (int x = 0; x < bitmap.Width; x++)
				{
					Color c = Color.FromArgb(bitmap[x, y]);

					int intensity = (int)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);

					map[x, y] = intensity / bucket_width;
				}
			}
		}

		public int HammingDistance(BucketImage image)
		{
			if (buckets != image.buckets || map.GetLength(0) != image.map.GetLength(0) || map.GetLength(1) != image.map.GetLength(1))
				throw new InvalidOperationException();

			int width = map.GetLength(0);
			int height = map.GetLength(1);

			int distance = 0;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (map[x, y] != image.map[x, y])
						distance++;
				}
			}

			return distance;
		}

		public FastBitmap ToGrayscale()
		{
			FastBitmap bitmap = new FastBitmap(new Bitmap(map.GetLength(0), map.GetLength(1)));

			for (int y = 0; y < bitmap.Height; y++)
			{
				for (int x = 0; x < bitmap.Width; x++)
				{
					int gray = (int)(255 * map[x, y] / (double)buckets);
					bitmap.SetPixel(x, y, Color.FromArgb(gray, gray, gray).ToArgb());
				}
			}

			return bitmap;
		}
	}
}
