using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using GR.Imaging;


namespace GR.Imaging.OCR
{
    public class TextRecognizer
    {
        private List<FontCharCollection> fonts;

        private FontCharMatcher matcher;

        private List<FontCharCollection> possible_fonts;
        private Dictionary<FontCharCollection, bool> possible_fonts_lookup;

        private List<FontChar> possible_chars; // All characters in all possible fonts (used when only few fonts are possible)
        private int possible_chars_font_count; // Number of fonts in possible_chars array

        private Dictionary<FontCharCollection, int> match_offsets; // The y-offset of each matching font

        private Dictionary<string, List<FontCharCollection>> font_cache = new Dictionary<string,List<FontCharCollection>>();

        public TextRecognizer(List<FontFamily> families, List<FontStyle> styles, float size_range_start, float size_range_end, float size_spacing)
		{
			fonts = new List<FontCharCollection>();
            matcher = new FontCharMatcher();

			foreach (FontFamily family in families)
			{
				for (float size = size_range_start; size <= size_range_end; size += size_spacing)
				{
                    foreach (FontStyle style in styles)
                    {
                        if (family.IsStyleAvailable(style))
                        {
                            Font font = new Font(family, size, style);
                            // Console.WriteLine("Processing font: {0} [{1}, {2}]", family.GetName(0), size, style);

                            FontCharCollection fcc = new FontCharCollection(font);
                            for (int i = 33; i <= 128; i++)
                                fcc.Add(new FontChar(fcc, (char)i));
                            fcc.Add(new FontChar(fcc, '€')); // Add € symbol.

                            fcc.SortBySize();
                            
                            fonts.Add(fcc);

                            matcher.Add(fcc);
                        }
                    }
				}
			}
		}

        public TextRecognizer(List<FontFamily> families, List<FontStyle> styles, int size_start, int size_end, int spacing)
        {
            fonts = new List<FontCharCollection>();
            matcher = new FontCharMatcher();

            foreach (FontFamily family in families)
            {
                for (int size = size_start; size <= size_end; size += spacing)
                {
                    foreach (FontStyle style in styles)
                    {
                        if (family.IsStyleAvailable(style))
                        {
                            Font font = new Font(family, size, style, GraphicsUnit.Pixel);
                            //Console.WriteLine("Processing font: {0} [{1}, {2}]", family.GetName(0), size, style);

                            FontCharCollection fcc = new FontCharCollection(font);
                            for (int i = 33; i <= 128; i++)
                                fcc.Add(new FontChar(fcc, (char)i));
                            fcc.Add(new FontChar(fcc, '€')); // Add € symbol.

                            fcc.SortBySize();

                            fonts.Add(fcc);

                            matcher.Add(fcc);
                        }
                    }
                }
            }
        }

        public Multistring GetSingleLine(Bitmap bitmap, Rectangle bounds, int font_color)
        {
            SetPossibleFonts(fonts); // Set all fonts possible

            return _GetSingleLine(bitmap, bounds, font_color);
        }

        public Multistring GetSingleLine(Bitmap bitmap, int font_color)
        {
            SetPossibleFonts(fonts); // Set all fonts possible

            return _GetSingleLine(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), font_color);
        }

        public List<Multistring> GetMultiLine(Bitmap bitmap, Rectangle bounds, int font_color)
        {
            SetPossibleFonts(fonts); // Set all fonts possible

            return _GetMultiLine(bitmap, bounds, font_color);
        }

        public Multistring GetSingleLineCached(Bitmap bitmap, Rectangle bounds, int font_color, string cache_key)
        {
            if (!font_cache.ContainsKey(cache_key))
            {
                SetPossibleFonts(fonts);
            }
            else
            {
                SetPossibleFonts(font_cache[cache_key]);
            }

            Multistring result = _GetSingleLine(bitmap, bounds, font_color);

            // Update cache
            List<FontCharCollection> new_fonts = new List<FontCharCollection>();
            foreach (FontCharCollection font in possible_fonts)
            {
                new_fonts.Add(font);
            }
            font_cache[cache_key] = new_fonts;

            return result;
        }

        public List<Multistring> GetMultiLineCached(Bitmap bitmap, Rectangle bounds, int font_color, string cache_key)
        {
            if (!font_cache.ContainsKey(cache_key))
            {
                SetPossibleFonts(fonts);
            }
            else
            {
                SetPossibleFonts(font_cache[cache_key]);
            }

            List<Multistring> result = _GetMultiLine(bitmap, bounds, font_color);

            // Update cache
            List<FontCharCollection> new_fonts = new List<FontCharCollection>();
            foreach (FontCharCollection font in possible_fonts)
            {
                new_fonts.Add(font);
            }
            font_cache[cache_key] = new_fonts;

            return result;
        }

        private Multistring _GetSingleLine(Bitmap bitmap, Rectangle bounds, int font_color)
        {
            Rectangle dest_bounds = BitmapAnalyzer.BoundingRectangle(bitmap, bounds, font_color);
            FastBitmap dest_bitmap = new FastBitmap(bitmap);

            int h;
            Multistring result = RecognizeSingleLine(dest_bitmap, dest_bounds, font_color, out h);

            dest_bitmap.UnlockBitmap();

            PrintPossibleFonts();

            return result;
        }

        private List<Multistring> _GetMultiLine(Bitmap bitmap, Rectangle bounds, int font_color)
        {
            Rectangle dest_bounds = BitmapAnalyzer.BoundingRectangle(bitmap, bounds, font_color);
            FastBitmap dest_bitmap = new FastBitmap(bitmap);

            List<Multistring> result = RecognizeMultiLine(dest_bitmap, dest_bounds, font_color);

            dest_bitmap.UnlockBitmap();

            PrintPossibleFonts();

            return result;
        }

        private void SetPossibleFonts(List<FontCharCollection> new_fonts)
        {
            possible_fonts = new List<FontCharCollection>();
            possible_fonts_lookup = new Dictionary<FontCharCollection, bool>();

            possible_chars = null;
            possible_chars_font_count = 0;

            match_offsets = new Dictionary<FontCharCollection, int>();

            foreach (FontCharCollection font in new_fonts)
            {
                possible_fonts.Add(font);
                possible_fonts_lookup[font] = true;
            }
        }


        private List<Multistring> RecognizeMultiLine(FastBitmap dest_bitmap, Rectangle dest_bounds, int font_color)
        {
            int max_height = 0;
            foreach (FontCharCollection font in possible_fonts) if (font.MaxHeight > max_height) max_height = font.MaxHeight;

            int start_y = 0;

            List<Multistring> lines = new List<Multistring>();

            while (start_y < dest_bounds.Height - 1)
            {
                bool empty = true;

                for (int x = 0; x < dest_bounds.Width; x++)
                {
                    if (dest_bitmap[dest_bounds.X + x, dest_bounds.Y + start_y] == font_color)
                    {
                        empty = false;
                        break;
                    }
                }

                if (!empty)
                {
                    int end_y = start_y + max_height +1;
                    if (end_y > dest_bounds.Height - 1) end_y = dest_bounds.Height - 1;

                    Rectangle bounds = dest_bounds;
                    bounds.Y += start_y;
                    bounds.Height = end_y - start_y + 1;

                    //Console.WriteLine("fop " + bounds);

                    int h;

                    Multistring line = RecognizeSingleLine(dest_bitmap, bounds, font_color, out h);

                    if (h > 0)
                    {
                       // Console.WriteLine("start_y += " + h);
                        lines.Add(line);
                        start_y += h;
                    }
                    else
                    {
                        start_y++;
                    }
                }
                else
                {
                    start_y++;
                }
            }

            return lines;
        }


        private Multistring RecognizeSingleLine(FastBitmap dest_bitmap, Rectangle dest_bounds, int font_color, out int max_height)
        {
            match_offsets.Clear();


            max_height = 0;

            int[] y_offsets = new int[dest_bounds.Width];
            bool[] empty = new bool[dest_bounds.Width];

            for (int x = 0; x < dest_bounds.Width; x++)
            {
                y_offsets[x] = -1;

                for (int y = 0; y < dest_bounds.Height; y++)
                {
                    if (dest_bitmap[dest_bounds.X + x, dest_bounds.Y + y] == font_color)
                    {
                        y_offsets[x] = y;
                        break;
                    }
                }

                if (y_offsets[x] < 0) empty[x] = true;
            }

            int space_width = TextRenderer.MeasureText("   ", possible_fonts[0].Font).Width - TextRenderer.MeasureText("  ", possible_fonts[0].Font).Width;
            if (space_width < 1) space_width = 1;

           //Console.WriteLine(space_width);

            int current_x = 0;
            int empty_cols = 0;

            int wanted_match_offset = int.MinValue;

            Multistring line_str = new Multistring();

            while (current_x < dest_bounds.Width)
            {
                if (y_offsets[current_x] >= 0)
                {
                    empty_cols = 0;

                    Rectangle bounds = dest_bounds;
                    bounds.X = dest_bounds.X + current_x;
                    bounds.Width = dest_bounds.Width - current_x;

                    List<FontChar> matches = GetMatches(dest_bitmap, bounds, font_color, wanted_match_offset, y_offsets, current_x);

                    if (matches.Count >0)
                    {

                        // Remove the recognized character from the destination bitmap
                        dest_bitmap.BlitSingleColor(matches[0].Bitmap, 
                            new Point(bounds.X, bounds.Y + match_offsets[matches[0].FontChars] + matches[0].CropRect.Y), matches[0].FontColor, (int)0x00FFFFFF);

                        // Update y-offsets after removing the recognized character
                        for (int x = 0; x < matches[0].Bitmap.Width; x++)
                        {
                            y_offsets[current_x + x] = -1;

                            for (int y = 0; y < dest_bounds.Height; y++)
                            {
                                if (dest_bitmap[bounds.X + x, dest_bounds.Y + y] == font_color)
                                {
                                    y_offsets[current_x + x] = y;
                                    break;
                                }
                            }
                        }

                        if (possible_fonts.Count > 1)
                        {
                            possible_fonts_lookup.Clear();
                            possible_fonts.Clear();

                            foreach (FontChar fc in matches)
                            {
                                possible_fonts_lookup[fc.FontChars] = true;
                            }

                            foreach (FontCharCollection font in possible_fonts_lookup.Keys)
                            {
                                possible_fonts.Add(font);
                            }

                            space_width = TextRenderer.MeasureText("   ", possible_fonts[0].Font).Width - TextRenderer.MeasureText("  ", possible_fonts[0].Font).Width;
                            if (space_width < 1) space_width = 1;
                        }

                        List<char> possible_chars = new List<char>();

                        foreach (FontChar fc in matches)
                        {
                            if (match_offsets[fc.FontChars] + matches[0].RealHeight > max_height) max_height = match_offsets[fc.FontChars] + matches[0].RealHeight;

                            if (!possible_chars.Contains(fc.Character)) possible_chars.Add(fc.Character);
                        }

                        line_str.Append(possible_chars);
                    }
                }
                else if (empty[current_x]) 
                {
                    //Console.WriteLine("{0} empty", current_x);

                    empty_cols++;

                    if (empty_cols >= space_width)
                    {
                        line_str.Append(' ');
                        //Console.WriteLine("SPACE");
                        empty_cols = 0;
                    }
                }

                current_x++;
            }

            return line_str;
        }


        private List<FontChar> GetMatches(FastBitmap bitmap, Rectangle bounds, int font_color, int wanted_match_offset, int[] y_offsets, int current_x)
        {
            List<FontChar> matches = new List<FontChar>();

            int match_pixels = -1, match_width = 0, match_height = 0;

            List<FontChar> font_chars;


            if (possible_fonts.Count >= 5)
            {
                font_chars = matcher.FindPossibleMatches(bitmap, bounds, font_color, possible_fonts_lookup);
            }
            else 
            {
                // Update char collection if the amount of possible fonts has decreased
                if (possible_chars == null || possible_chars_font_count != possible_fonts.Count)
                {
                    possible_chars_font_count = possible_fonts.Count;

                    possible_chars = new List<FontChar>();

                    foreach (FontCharCollection fcc in possible_fonts)
                    {
                        possible_chars.AddRange(fcc.FontChars);
                    }

                    // Sort by pixel count
                    possible_chars.Sort();
                }

                font_chars = possible_chars;
            }

            // Do the real matching using pixel-by-pixel comparison
            foreach (FontChar fc in font_chars)
            {
                // We already have a match and this one has less pixels than the match, so no-no
                if (match_pixels >= 0 && fc.PixelCount < match_pixels) break;

                // Alternative matching character should have the same width and height as the original
                if (match_pixels < 0 || (fc.Width == match_width && fc.Height == match_height))
                {
                    int offset = int.MinValue;
                    if (match_offsets.ContainsKey(fc.FontChars)) offset = match_offsets[fc.FontChars];

                    if (fc.IsMatch(font_color, bitmap, bounds, true, false, y_offsets, current_x, ref offset))
                    {
                        match_offsets[fc.FontChars] = offset;

                        matches.Add(fc);

                        if (match_pixels < 0)
                        {
                            match_pixels = fc.PixelCount;
                            match_width = fc.Width;
                            match_height = fc.Height;
                        }
                    }
                }
            }
            

            return matches;
        }


        private void PrintPossibleFonts()
        {
            /*Console.Write("Possible fonts: ");
            foreach (FontCharCollection font in possible_fonts)
            {
                Console.Write("{0}({1},{2}) ", font.Font.FontFamily.GetName(0), font.Font.Style, font.Font.Size);
            }
            Console.WriteLine();*/
        }
    }
}
