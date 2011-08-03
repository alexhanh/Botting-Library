using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace GR.Imaging
{
    public class BitmapAnalyzer
    {
        // inclusive crop rectangle
        public static Image CropImage(Image image, Rectangle crop_area)
        {
            Bitmap bitmap = new Bitmap(image);
            return (Image)bitmap.Clone(crop_area, bitmap.PixelFormat);
        }

        public static Bitmap CropBitmap(Bitmap bitmap, Rectangle crop_area)
        {
            return bitmap.Clone(crop_area, bitmap.PixelFormat);
        }

        public static List<Image> SliceImage(Image image, int vertical_slices, int horizontal_slices)
        {
            int hor_count = horizontal_slices + 1;
            int ver_count = vertical_slices + 1;

            if (vertical_slices < 0 || vertical_slices > (image.Size.Width - 1))
                return null;
            if (horizontal_slices < 0 || horizontal_slices > (image.Size.Height - 1))
                return null;
            if (image.Size.Width % ver_count != 0 || image.Size.Height % hor_count != 0)
                return null;

            Size sz = new Size(image.Size.Width / ver_count, image.Size.Height / hor_count);

            List<Image> slices = new List<Image>();

            for (int y = 0; y < hor_count; y++)
            {
                for (int x = 0; x < ver_count; x++)
                {
                    Rectangle crop_rect = new Rectangle(x * sz.Width, y * sz.Height, sz.Width, sz.Height);
                    Image slice = CropImage(image, crop_rect);
                    slices.Add(slice);
                }
            }

            return slices;
        }

        public static List<Bitmap> SliceBitmap(Bitmap image, int vertical_slices, int horizontal_slices)
        {
            List<Image> slices = SliceImage((Image)image, vertical_slices, horizontal_slices);
            List<Bitmap> bitmaps = new List<Bitmap>();
            foreach (Image slice in slices)
            {
                bitmaps.Add(new Bitmap(slice));
            }

            return bitmaps;
        }

        public static int AbsoluteDiff(Color c1, Color c2)
        {
            return Math.Abs(c2.R - c1.R) + Math.Abs(c2.G - c1.G) + Math.Abs(c2.B - c1.B);
        }

        public static int AbsoluteDiff(FastBitmap bmp1, FastBitmap bmp2, Rectangle sub_rect)
        {
            int diff = 0;
            for (int y = sub_rect.Y; y < sub_rect.Y + sub_rect.Height; y++)
				for (int x = sub_rect.X; x < sub_rect.X + sub_rect.Width; x++)
				{
					diff += AbsoluteDiff(Color.FromArgb(bmp1.GetPixel(x, y)), Color.FromArgb(bmp2.GetPixel(x, y)));
				}

            return diff;
        }

        public static int AbsoluteDiff(FastBitmap bmp1, FastBitmap bmp2)
        {
            return AbsoluteDiff(bmp1, bmp2, new Rectangle(0, 0, bmp1.Width, bmp1.Height));
        }

        /// <summary>
        /// Removes duplicate bitmaps from an enumerable FastBitmap collection and returns the resulting unique bitmap collection.
        /// </summary>
        /// <param name="bitmaps"></param>
        /// <returns></returns>
        public static List<FastBitmap> RemoveDuplicates(IEnumerable<FastBitmap> bitmaps)
        {
            List<FastBitmap> unique_bitmaps = new List<FastBitmap>();

            foreach (FastBitmap bitmap in bitmaps)
            {
                bool unique = true;

                foreach (FastBitmap unique_bitmap in unique_bitmaps)
                {
                    if (FastBitmap.IsMatch(bitmap, unique_bitmap))
                    {
                        unique = false;
                        break;
                    }
                }

                if (unique)
                {
                    unique_bitmaps.Add(bitmap);
                }
            }

            return unique_bitmaps;
        }

        public static bool IsEqual(Color c1, Color c2, int diff)
        {
            if (Math.Abs(c1.R - c2.R) > diff) return false;
            if (Math.Abs(c1.G - c2.G) > diff) return false;
            if (Math.Abs(c1.B - c2.B) > diff) return false;
            return true;
        }

        // gets least similar pixel in all bitmaps, and returns back the bitmaps in which it was the same
        // todo: make it return all of least matching pixels.. ie. List<List<Bitmap>> and out List<Point> points
        // or class solution { List<Bitmap>; Point point; } .... static solution Pass(List<Bitmap> bitmaps)..
        public static List<Bitmap> FindLeastSimilarPixel(List<Bitmap> bitmaps, out Point point)
        {
            Size size = bitmaps[0].Size;
            // matching bitmaps in with the least similar pixel
            List<Bitmap> matching_bitmaps = new List<Bitmap>();
            Point least_similar = new Point(-1, -1);
            int smallest = int.MaxValue;

            for (int y = 0; y < size.Height; y++)
            {
                for (int x = 0; x < size.Width; x++)
                {
                    Dictionary<int, List<Bitmap>> color_matches = new Dictionary<int, List<Bitmap>>();

                    foreach (Bitmap bitmap in bitmaps)
                    {
                        int hash_code = bitmap.GetPixel(x, y).GetHashCode();
                        if (color_matches.ContainsKey(hash_code))
                        {
                            color_matches[hash_code].Add(bitmap);
                        }
                        else
                        {
                            color_matches[hash_code] = new List<Bitmap>();
                            color_matches[hash_code].Add(bitmap);
                        }
                    }

                    List<Bitmap> biggest_set = null;

                    foreach (List<Bitmap> set in color_matches.Values)
                    {
                        if (biggest_set == null)
                            biggest_set = set;
                        else
                        {
                            if (set.Count > biggest_set.Count)
                                biggest_set = set;
                        }
                    }
                    //Console.WriteLine("similar pixels on " + x + "," + y + "=" + biggest_set.Count);
                    //Console.ReadKey();

                    if (biggest_set.Count < smallest)
                    {
                        least_similar.X = x;
                        least_similar.Y = y;
                        matching_bitmaps = biggest_set;
                        smallest = biggest_set.Count;
                    }
                }
            }

            point = least_similar;

            return matching_bitmaps;
        }

        public static Bitmap BoundingBitmap(Bitmap bmp, int bound_color, out Rectangle crop_rect)
        {
            crop_rect = BoundingRectangle(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height), bound_color);
            return BitmapAnalyzer.CropBitmap(bmp, crop_rect);
        }

        public static Rectangle BoundingRectangle(Bitmap bmp, Rectangle bounds, int bound_color)
        {
            FastBitmap fbmp = new FastBitmap(bmp);
            int s_x = 0, e_x = 0, s_y = 0, e_y = 0; bool done = false;
            for (int x = bounds.X; x < bounds.X + bounds.Width && !done; x++)
            {
                for (int y = bounds.Y; y < bounds.Y + bounds.Height; y++)
                {
                    if (fbmp.GetPixel(x, y) == bound_color)
                    {
                        s_x = x;
                        done = true;
                        break;
                    }
                }
            }
            done = false;
            for (int x = bounds.X + bounds.Width - 1; x >= bounds.X && !done; x--)
            {
                for (int y = bounds.Y; y < bounds.Y + bounds.Height; y++)
                {
                    if (fbmp.GetPixel(x, y) == bound_color)
                    {
                        e_x = x;
                        done = true;
                        break;
                    }
                }
            }
            done = false;
            for (int y = bounds.Y; y < bounds.Y + bounds.Height && !done; y++)
            {
                for (int x = bounds.X; x < bounds.X + bounds.Width; x++)
                {
                    if (fbmp.GetPixel(x, y) == bound_color)
                    {
                        s_y = y;
                        done = true;
                        break;
                    }
                }
            }
            done = false;
            for (int y = bounds.Y + bounds.Height - 1; y >= bounds.Y && !done; y--)
            {
                for (int x = bounds.X; x < bounds.X + bounds.Width; x++)
                {
                    if (fbmp.GetPixel(x, y) == bound_color)
                    {
                        e_y = y;
                        done = true;
                        break;
                    }
                }
            }

            fbmp.UnlockBitmap();

            //Console.WriteLine(s_x + " " + e_x + " " + s_y + " " + e_y);

            Rectangle crop_rect = new Rectangle(s_x, s_y, e_x - s_x + 1, e_y - s_y + 1);


            return crop_rect;
        }

		public static Point CenterOfIntensity(FastBitmap bitmap)
		{
			Point center = new Point(0, 0);

			int total_mass = 0;
			for (int y = 0; y < bitmap.Height; y++)
			{
				for (int x = 0; x < bitmap.Width; x++)
				{
					Color c = Color.FromArgb(bitmap[x, y]);

					// http://www.devmaster.net/forums/showthread.php?t=8201
					int luminance = (int)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);

					//center.X
					center.X += luminance * x;
					center.Y += luminance * y;

					total_mass += luminance;
				}
			}

			if (total_mass > 0)
			{
				center.X /= total_mass;
				center.Y /= total_mass;
			}

			return center;
		}

		/// <summary>
		/// Generates transition images of #left and #right pixels to left and right respectfully.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static List<FastBitmap> GenerateTransitions(FastBitmap number, int left, int right)
		{
			List<FastBitmap> transitions = new List<FastBitmap>();

			for (int x = -1 * left; x <= right; x++)
			{
				if (x == 0)
					continue;

				number.UnlockBitmap();

				Bitmap bitmap = new Bitmap(number.Width, number.Height, number.Bitmap.PixelFormat);
				Graphics g = Graphics.FromImage(bitmap);
				g.Clear(Color.FromArgb(0, 0, 0));

				if (x < 0)
					g.DrawImageUnscaled(number.Bitmap.Clone(new Rectangle(Math.Abs(x), 0, number.Width - Math.Abs(x), number.Height), number.Bitmap.PixelFormat), 0, 0);
				else
					g.DrawImageUnscaled(number.Bitmap.Clone(new Rectangle(0, 0, number.Width - x, number.Height), number.Bitmap.PixelFormat), x, 0);

				number.LockBitmap();

				transitions.Add(new FastBitmap(bitmap));
			}

			return transitions;
		}

		/// <summary>
		/// Finds the first occurence of color and returns its position.
		/// </summary>
		/// <param name="color"></param>
		/// <returns>Returns the point where color was found; Point.Empty otherwise.</returns>
		public static Point PositionOf(FastBitmap bmp, Color color)
		{
			int argb = color.ToArgb();
			for (int y = 0; y < bmp.Height; y++)
			{
				for (int x = 0; x < bmp.Width; x++)
				{
					if (bmp[x, y] == argb)
						return new Point(x, y);
				}
			}

			return Point.Empty;
		}

		/// <summary>
		/// Finds the first occurence of any color in given range and returns its position.
		/// </summary>
		/// <param name="bmp"></param>
		/// <param name="range"></param>
		/// <returns>Returns the point where color was found; Point.Empty otherwise.</returns>
		public static Point PositionOf(FastBitmap bmp, ColorRange range)
		{
			for (int y = 0; y < bmp.Height; y++)
			{
				for (int x = 0; x < bmp.Width; x++)
				{
					if (range.HasArgb(bmp[x, y]))
						return new Point(x, y);
				}
			}

			return Point.Empty;
		}

		public static Point PositionOf(FastBitmap bmp, ColorRange range, Rectangle subRect)
		{
			for (int y = subRect.Y; y <= subRect.Height; y++)
			{
				for (int x = subRect.X; x <= subRect.Width; x++)
				{
					if (range.HasArgb(bmp[x, y]))
						return new Point(x, y);
				}
			}

			return Point.Empty;
		}

		public static Point PositionOf(FastBitmap bmp, Point start, ColorRange range, Rectangle subRect)
		{
			for (int y = start.Y; y <= subRect.Height; y++)
			{
				for (int x = start.X; x <= subRect.Width; x++)
				{
					if (range.HasArgb(bmp[x, y]))
						return new Point(x, y);
				}
			}

			return Point.Empty;
		}
	}
}
