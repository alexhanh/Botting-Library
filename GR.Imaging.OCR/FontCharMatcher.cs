using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GR.Imaging;

namespace GR.Imaging.OCR
{
    public class FontCharMatcher
    {
        private static int MAX_HASH_WIDTH = 3;
        private static int MAX_CHAR_HEIGHT = 1000;

        Dictionary<int, List<FontChar>> lookup_table;

        int max_char_height = 0;
        int[] max_offset_by_height = new int[MAX_CHAR_HEIGHT];


        public FontCharMatcher()
        {
            lookup_table = new Dictionary<int, List<FontChar>>();
        }

        public void Add(FontCharCollection chars)
        {
            foreach (FontChar c in chars)
            {
                Add(c);
            }
        }

        public void Add(FontChar c)
        {
            if (c.PixelCount == 0) return;

            Rectangle hash_bounds = new Rectangle(0, 0, c.Width, c.Height);
            if (hash_bounds.Width > MAX_HASH_WIDTH) hash_bounds.Width = MAX_HASH_WIDTH;

            int hash = CalculateHash(c.Bitmap, hash_bounds, c.FontColor);

            if (c.Height > max_char_height) max_char_height = c.Height;

            int max_offset = c.FontChars.MaxHeight - c.Height;
            if (max_offset > max_offset_by_height[c.Height]) max_offset_by_height[c.Height] = max_offset;


            if (!lookup_table.ContainsKey(hash))
            {
                lookup_table[hash] = new List<FontChar>();
            }

            lookup_table[hash].Add(c);
        }


        public List<FontChar> FindPossibleMatches(FastBitmap bitmap, Rectangle bounds, int font_color, Dictionary<FontCharCollection, bool> possible_fonts)
        {
            List<FontChar> matches = new List<FontChar>();

            for (int hash_width = Math.Min(MAX_HASH_WIDTH, bounds.Width); hash_width >= 1; hash_width--)
            {
                for (int hash_height = Math.Min(max_char_height, bounds.Height); hash_height >= 1; hash_height--)
                {
                    for (int offset = 0; offset <= max_offset_by_height[hash_height]; offset++)
                    {
                        if (offset + hash_height > bounds.Height) continue;

                        Rectangle hash_bounds = new Rectangle(bounds.X, bounds.Y + offset, hash_width, hash_height);

                        int hash = CalculateHash(bitmap, hash_bounds, font_color);

                        if (!lookup_table.ContainsKey(hash)) continue;

                        foreach (FontChar c in lookup_table[hash])
                        {
                            if (c.Height != hash_height) continue;

                            if (c.Width < hash_width) continue;

                            if (!possible_fonts.ContainsKey(c.FontChars)) continue;

                            matches.Add(c);
                        }
                    }
                }
            }

            // Sort by pixel count
            matches.Sort();

            return matches;
        }

        private int CalculateHash(FastBitmap bitmap, Rectangle bounds, int target_color)
        {
            int hash = 0;
            int index = 0;

            for (int y = 0; y < bounds.Height; y++)
            {
                for (int x = 0; x < bounds.Width; x++)
                {
                    if (bitmap[bounds.X + x, bounds.Y + y] == target_color)
                    {
                        hash ^= 0x01 << index;
                    }

                    index = (index + 1) % 32;
                }
            }

            return hash;
        }
    }
}
