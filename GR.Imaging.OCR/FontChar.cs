using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;

using GR.Imaging;

namespace GR.Imaging.OCR
{
    public class FontCharCollection
    {
        private List<FontChar> font_chars;
        private Font font;


        public FontCharCollection(Font font)
        {
            font_chars = new List<FontChar>();
            this.font = font;
        }

        public int Count { get { return font_chars.Count; } }
        public FontChar this[int index] { get { return font_chars[index]; } }
        public Font Font { get { return font; } }
        public List<FontChar> FontChars { get { return font_chars; } }

        public int MaxHeight { get { return max_height; } }

        private int max_height;
        private int min_crop_y = int.MaxValue, max_crop_y = int.MinValue;

        public int MinCropY { get { return min_crop_y; } }

        public void Add(FontChar font_char)
        {
            font_chars.Add(font_char);

            Rectangle crop = font_char.CropRect;

            if (crop.Y < min_crop_y) min_crop_y = crop.Y;
            if (crop.Y + crop.Height > max_crop_y) max_crop_y = crop.Y + crop.Height;

            max_height = max_crop_y - min_crop_y;
        }

        //private static int CompareSizes(FontChar c1, FontChar c2)
        //{
        //    int p = -c1.PixelCount.CompareTo(c2.PixelCount);
        //    if (p != 0) return p;

        //    //int w = -c1.Width.CompareTo(c2.Width);
        //    //if (w != 0) return w;

        //    //int h = -c1.Height.CompareTo(c2.Height);
        //    //if (h != 0) return h;

        //    return c1.Character.CompareTo(c2.Character);
        //}

        public void SortBySize()
        {
            // The default comparer sorts by the amount of pixels
            font_chars.Sort();
        }

        public IEnumerator GetEnumerator()
        {
            return font_chars.GetEnumerator();
        }

    }

    public class FontChar : IComparable<FontChar>
    {
        protected char character;
        protected FastBitmap bitmap;
        protected FontCharCollection font_chars;
        protected Font font;
        protected int color;

        protected Rectangle crop_rect;

        protected int[] y_offsets;

        protected int pixel_count;

        public FastBitmap Bitmap { get { return bitmap; } }
        public int FontColor { get { return color; } }
        public int Width { get { return bitmap.Width; } }
        public int Height { get { return bitmap.Height; } }
        public int RealHeight { get { return crop_rect.Y + bitmap.Height; } }

        public Rectangle CropRect { get { return crop_rect; } }

        public Font Font { get { return font; } }
        public FontCharCollection FontChars { get { return font_chars; } }
        public char Character { get { return character; } }

        public int PixelCount { get { return pixel_count; } }

        public FontChar(FontCharCollection font_chars, char character)
        {
            this.character = character;
            this.font_chars = font_chars;
            this.font = font_chars.Font;
            this.color = Color.White.ToArgb();

            string s_char = "" + character;
            Size size = TextRenderer.MeasureText(s_char, font, new Size(int.MaxValue, int.MaxValue));
            size.Width *= 2;
            size.Height *= 2;
            Bitmap char_bmp = new Bitmap(size.Width, size.Height);
            Graphics g = Graphics.FromImage(char_bmp);
            TextRenderer.DrawText(g, s_char, font, new Point(0, 0), Color.White, TextFormatFlags.NoPrefix);

            //if ((int)character == 8364)
            //{
            //    char_bmp.Save("etmerkki.bmp");
            //    Console.ReadLine();
            //}

            //g.DrawString(s_char, font, new SolidBrush(Color.White), new Point(0, 0));
            //g.Flush();

            bitmap = new FastBitmap(BitmapAnalyzer.BoundingBitmap(char_bmp, this.color, out crop_rect));


            y_offsets = new int[bitmap.Width];
            pixel_count = 0;

            for (int x = 0; x < bitmap.Width; x++)
            {
                y_offsets[x] = -1;

                for (int y = 0; y < bitmap.Height; y++)
                {
                    if (bitmap.GetPixel(x, y) == this.color)
                    {
                        if (y_offsets[x] < 0) y_offsets[x] = y;

                        pixel_count++;
                    }
                }
            } 
        }

        public bool IsMatch(int font_color, FastBitmap dest_bitmap, Rectangle dest_bounds, bool allow_extra_pixels,
            bool offsets_optimization, int[] dest_y_offsets, int offsets_start, ref int y_offset)
        {
            if (PixelCount == 0) return false;

            if (this.Width > dest_bounds.Width) return false;

            int base_y_offset = dest_y_offsets[offsets_start] - y_offsets[0];

            if (base_y_offset < 0) return false;
            if (base_y_offset + this.Height > dest_bounds.Height) return false;

            int new_y_offset = base_y_offset - crop_rect.Y;

            // This prevents matching with a character which is too far from the top of the bounds.
            // Assumes that the top of the boundary is exactly at first pixel line of the text.
            // (+1 tolerance just for fun :)
            if (new_y_offset > -font_chars.MinCropY + 1) return false;

            if (y_offset >= -1000)
            {
                if (new_y_offset != y_offset) return false;
            }


            if (offsets_optimization)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (y_offsets[x] < 0) continue;
                    if (base_y_offset + y_offsets[x] != dest_y_offsets[offsets_start + x]) return false;
                }
            }

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    bool c1 = bitmap[x, y] == this.color;

                    bool c2 = dest_bitmap[dest_bounds.X + x, dest_bounds.Y + base_y_offset + y] == font_color;

                    if (allow_extra_pixels)
                    {
                        if (c1 && !c2) return false;
                    }
                    else
                    {
                        if (c1 != c2) return false;
                    }
                }
            }

            int last_y = base_y_offset + Height;
            if (last_y < dest_bounds.Height)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (dest_bitmap[dest_bounds.X + x, dest_bounds.Y + last_y] == font_color) return false;
                }
            }


            y_offset = new_y_offset;

            return true;
        }

        #region IComparable<FontChar> Members

        public int CompareTo(FontChar other)
        {
            int p = -PixelCount.CompareTo(other.PixelCount);
            if (p != 0) return p;

            //int w = -c1.Width.CompareTo(c2.Width);
            //if (w != 0) return w;

            //int h = -c1.Height.CompareTo(c2.Height);
            //if (h != 0) return h;

            return Character.CompareTo(other.Character);
        }

        #endregion
    }
}
