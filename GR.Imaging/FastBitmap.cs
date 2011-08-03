using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace GR.Imaging
{
    public unsafe class FastBitmap : IDisposable
    {
        private Bitmap subject;
        private int subject_width;
        private BitmapData bitmap_data = null;
        private Byte* p_base = null;
        //private long id = 0;

        public FastBitmap(Bitmap subject_bitmap)
        {
            this.subject = subject_bitmap;
            try
            {
                LockBitmap();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // This is causing exception on termination on Vista.
        /*~FastBitmap()
        {
            Release();
        }*/

		/// <summary>
		/// Call, when you no longer need this fast bitmap nor the bitmap locked with this instance.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		private bool disposed = false;
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					UnlockBitmap();
					Bitmap.Dispose();
				}

				subject = null;
				bitmap_data = null;
				p_base = null;

				disposed = true;
			}
		}
		
		~FastBitmap()
		{
			Dispose(false);
		}

        public Bitmap Bitmap
        {
            get { return subject; }
        }

		public PixelFormat PixelFormat
		{
			get { return subject.PixelFormat; }
		}

        public int this[int x, int y]
        {
            get { return GetPixel(x, y); }
            set { SetPixel(x, y, value); }
        }

        public void SetPixel(int x, int y, int color)
        {
            try
            {
                int* p = PixelAt(x, y);
                (*p) = color;
            }
            catch (AccessViolationException ave)
            {
				// TODO: Remove when fixed.
				this.Bitmap.Save("setpixel_accessviolationexception.bmp");
				System.IO.File.WriteAllText("setpixel_accessviolationexception.txt", "x: " + x + " y: " + y + " bmp width: " + this.Width + " bmp height: " + this.Height);

                throw ave;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int GetPixel(int x, int y)
        {
	        int* p = PixelAt(x, y);
		    return (*p);
        }

		public int GetPixel(Point point)
		{
			return GetPixel(point.X, point.Y);
		}

		public Color GetColor(int x, int y)
		{
			return Color.FromArgb(GetPixel(x, y));
		}

		public Color GetColor(Point point)
		{
			return GetColor(point.X, point.Y);
		}

        public void BlitSingleColor(FastBitmap bitmap, Point location, int test_color, int blit_color)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            if (location.X + width > this.Width) width = this.Width - location.X;
            if (location.Y + height > this.Height) height = this.Height - location.Y;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (bitmap[x, y] == test_color)
                    {
                        this[location.X + x, location.Y + y] = blit_color;
                    }
                }
            }
        }

        /// <summary>
        /// Returns whether these two fast bitmaps match in width, height and pixel data.
        /// </summary>
        /// <param name="fast_bitmap1"></param>
        /// <param name="fast_bitmap2"></param>
        /// <returns></returns>
        public static bool IsMatch(FastBitmap fast_bitmap1, FastBitmap fast_bitmap2)
        {
            if (fast_bitmap1.Width != fast_bitmap2.Width || fast_bitmap1.Height != fast_bitmap2.Height)
                return false;

            for (int y = 0; y < fast_bitmap1.Height; y++)
            {
                for (int x = 0; x < fast_bitmap1.Width; x++)
                {
                    if (fast_bitmap1.GetPixel(x, y) != fast_bitmap2.GetPixel(x, y))
                        return false;
                }
            }

            return true;
        }

		public static bool IsMatch(FastBitmap fast_bitmap1, FastBitmap fast_bitmap2, int total_tolerance)
		{
			if (fast_bitmap1.Width != fast_bitmap2.Width || fast_bitmap1.Height != fast_bitmap2.Height)
				return false;

			int total = 0;
			for (int y = 0; y < fast_bitmap1.Height; y++)
			{
				for (int x = 0; x < fast_bitmap1.Width; x++)
				{
					total += BitmapAnalyzer.AbsoluteDiff(fast_bitmap1.GetColor(x, y), fast_bitmap2.GetColor(x, y));

					if (total > total_tolerance)
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Returns whether these two fast bitmaps match in width, height and pixel data within the given sub rectangle.
		/// </summary>
		/// <param name="fast_bitmap1"></param>
		/// <param name="fast_bitmap2"></param>
		/// <param name="sub_rect"></param>
		/// <returns></returns>
		public static bool IsMatch(FastBitmap fast_bitmap1, FastBitmap fast_bitmap2, Rectangle sub_rect)
		{
			if (fast_bitmap1.Width != fast_bitmap2.Width || fast_bitmap1.Height != fast_bitmap2.Height)
				return false;

			int end_y = sub_rect.Y + sub_rect.Height;
			int end_x = sub_rect.X + sub_rect.Width;
			for (int y = sub_rect.Y; y < end_y; y++)
			{
				for (int x = sub_rect.X; x < end_x; x++)
				{
					if (fast_bitmap1.GetPixel(x, y) != fast_bitmap2.GetPixel(x, y))
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Does an exact match of this bitmap in fast_bitmap in basepoint.
		/// </summary>
		/// <param name="fast_bitmap"></param>
		/// <param name="sub_rect"></param>
		/// <returns></returns>
		public bool IsIn(FastBitmap fast_bitmap, Point basepoint)
        {
            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    if (this.GetPixel(x, y) != fast_bitmap.GetPixel(basepoint.X + x, basepoint.Y + y))
                        return false;
                }
            }

            return true;
        }

		/// <summary>
		/// Matches until difference is more than total tolerance.
		/// </summary>
		/// <param name="fast_bitmap"></param>
		/// <param name="sub_rect"></param>
		/// <returns></returns>
		public bool IsIn(FastBitmap fast_bitmap, Point basepoint, int tolerance)
		{
			int total = 0;
			for (int y = 0; y < this.Height; y++)
			{
				for (int x = 0; x < this.Width; x++)
				{
					total += BitmapAnalyzer.AbsoluteDiff(this.GetColor(x, y), fast_bitmap.GetColor(basepoint.X + x, basepoint.Y + y));
					if (total > tolerance)
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Tests whether this bitmap exists in the collection.
		/// </summary>
		/// <param name="bmps"></param>
		/// <returns>True if it matches a bitmap; false otherwise.</returns>
		public bool IsIn(IEnumerable<FastBitmap> bmps)
		{
			foreach (FastBitmap bmp in bmps)
				if (FastBitmap.IsMatch(this, bmp))
					return true;

			return false;
		}

        public void LockBitmap()
        {
            GraphicsUnit unit = GraphicsUnit.Pixel;
            RectangleF boundsF = subject.GetBounds(ref unit);
            Rectangle bounds = new Rectangle((int)boundsF.X, (int)boundsF.Y, (int)boundsF.Width, (int)boundsF.Height);
            subject_width = (int)boundsF.Width * sizeof(int);

            if (subject_width % 4 != 0)
            {
                subject_width = 4 * (subject_width / 4 + 1);
            }

            bitmap_data = subject.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            p_base = (Byte*)bitmap_data.Scan0.ToPointer();
        }

        private int* PixelAt(int x, int y)
        {
            return (int*)(p_base + y * subject_width + x * sizeof(int));
        }

        public void UnlockBitmap()
        {
            if (bitmap_data == null) return;
            subject.UnlockBits(bitmap_data); bitmap_data = null; p_base = null;
        }

        public int Width { get { return Bitmap.Width; } }
        public int Height { get { return Bitmap.Height; } }
	}
}
