using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace GR.Imaging
{
    public class ImageFilter
    {
        public static void Binary(Bitmap b, bool flag, int threshold)
        {
            // GDI+ still lies to us - the return format is BGR, NOT RGB. 
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride; //the length of the line
            System.IntPtr Scan0 = bmData.Scan0;
            //int Threshold = 160;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - b.Width * 3;

                byte red, green, blue;
                byte binary;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        blue = p[0];
                        green = p[1];
                        red = p[2];

                        binary = (byte)(.299 * red
                            + .587 * green
                            + .114 * blue);

                        if (binary < threshold && flag)
                            p[0] = p[1] = p[2] = 0;
                        else
                            if (binary >= threshold && flag)
                                p[0] = p[1] = p[2] = 255;
                            else
                                if (binary < threshold && !flag)
                                    p[0] = p[1] = p[2] = 255;
                                else
                                    p[0] = p[1] = p[2] = 0;
                        p += 3;
                    }
                    p += nOffset;
                }

            }

            b.UnlockBits(bmData);
        }
    }
}
