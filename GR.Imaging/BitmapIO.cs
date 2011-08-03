using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using GR.IO;

namespace GR.Imaging
{
    public class BitmapIO
    {
        /// <summary>
        /// Saves image collection to given directory. Directory, with sub-folders, will be created if it does not exist.
        /// Filenames of the images will be prefixed with the given prefix and use a simple numbering naming convention, ie. "prefix_count.bmp".
        /// This will overwrite any existing files with the same name.
        /// </summary>
        /// <param name="bitmaps"></param>
        /// <param name="directory">Directory should be given as absolute ("C:/Foo/Bar/") or relative ("Foo/Bar/").</param>
        /// <param name="prefix"></param>
        public static void SaveToFiles(ICollection<FastBitmap> bitmaps, string directory, string prefix)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            int count_len = bitmaps.Count.ToString().Length;
            int index = 1;
            foreach (FastBitmap bitmap in bitmaps)
            {
                string path = directory + prefix;

                int index_len = index.ToString().Length;

                for (int i = 0; i < (count_len - index_len); i++)
                    path += "0";

                path += index.ToString() + ".bmp";

                bitmap.UnlockBitmap();

                bitmap.Bitmap.Save(path);

                bitmap.LockBitmap();

                index++;
            }
        }

        public static void SaveToFiles(ICollection<FastBitmap> bitmaps, string directory)
        {
            SaveToFiles(bitmaps, directory, "");
        }

		/// <summary>
		/// Saves to file but does not overwrite filename if it already exists, it insted adds a new number to the end.
		/// </summary>
		/// <param name="bitmap"></param>
		/// <param name="filepath"></param>
		public static void SaveToFile(FastBitmap bitmap, string filepath)
		{
			bitmap.Bitmap.Save(PathHelper.GenerateNextFilePath(filepath));
		}

        private static string[] gdiplus_file_formats = new string[] { ".bmp", ".gif", ".exig", ".jpg", ".jpeg", ".png", ".tiff" };
        public static List<FastBitmap> LoadFromDirectory(string directory)
        {
            List<FastBitmap> bitmaps = new List<FastBitmap>();

            foreach (string filepath in Directory.GetFiles(directory))
            {
                if (gdiplus_file_formats.Contains(Path.GetExtension(filepath)))
                    bitmaps.Add(new FastBitmap((Bitmap)Bitmap.FromFile(filepath)));
            }

            return bitmaps;
        }
    }
}
