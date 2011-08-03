using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GR.Input
{
    public abstract class ClickDistribution
    {
        protected Random random;

        public ClickDistribution(Random random)
        {
            this.random = random;
        }

        public Point Click(Button button)
        {
            Point point = _Click(button);

            if (point.X < 0 || point.X > button.Width || point.Y < 0 || point.Y > button.Height)
                throw new Exception("Point out of bounds");

            return point;
        }

        protected abstract Point _Click(Button button);
    }

    public class UniformDistribution : ClickDistribution
    {
        public UniformDistribution(Random random)
            : base(random)
        {
        }

        protected override Point _Click(Button button)
        {
            Point point = new Point();

            point.X = random.Next(button.Width);
            point.Y = random.Next(button.Height);

            return point;
        }
    }

    public class BgbDistribution : ClickDistribution
    {
        public BgbDistribution(Random random)
            : base(random)
        {
        }

        protected override Point _Click(Button button)
        {
            /*Point point = new Point();

            point.X = random.Next(button.Width);
            point.Y = random.Next(button.Height);

            return point;*/
            Point center = new Point(button.Width / 2, button.Height / 2);

            double dir = random.NextDouble() * (2 * Math.PI);
            double dist = random.NextDouble() * 20;

            double new_x = center.X + Math.Cos(dir) * dist;
            double new_y = center.Y + Math.Sin(dir) * dist;
            //double r = random.NextDouble() + random.NextDouble() - 1;
            //return new Point(60+(int)(15 * Math.Cos(r)), 25+(int)(15 * Math.Sin(r)));
            return new Point((int)new_x, (int)new_y);
        }
    }

    public class NormalDistribution : ClickDistribution
    {
        private double deviation;

        public NormalDistribution(Random random, double deviation)
            : base(random)
        {
            this.deviation = deviation;
        }

        private bool uselast = true;
        private double y2 = 0;

        private double RandomNormal(double m, double s)
        {
            double x1, x2, w, y1;

            if (uselast) { y1 = y2; uselast = false; }
            else
            {
                do
                {
                    x1 = 2.0 * random.NextDouble() - 1.0;
                    x2 = 2.0 * random.NextDouble() - 1.0;
                    w = x1 * x1 + x2 * x2;
                } while (w >= 1.0);

                w = Math.Sqrt((-2.0 * Math.Log(w)) / w);
                y1 = x1 * w;
                y2 = x2 * w;
                uselast = true;
            }
            return (m + y1 * s);
        }

        protected double RandomNormalScaled(double scale, double m, double s)
        {
            double res = RandomNormal(m, deviation);
            if (res < -3.5 * s) res = -3.5 * s;
            if (res > 3.5 * s) res = 3.5 * s;
            return (res / 3.5 * s + 1) * (scale / 2);
        }

        private double[] _ClickButton(double x, double y, double rx, double ry)
        {
            double rx2 = (RandomNormalScaled(2 * rx, 0, 1) - (rx));
            double ry2 = (RandomNormalScaled(2 * ry, 0, 1) - (ry));
            return new double[] { x + rx2, y + ry2 };
        }

        private double[] ClickButton(double x1, double y1, double x2, double y2)
        {
            double mx = (x2 - x1) / 2.0;
            double my = (y2 - y1) / 2.0;
            return _ClickButton(x1 + mx, y1 + my, mx, my);
        }

        protected override Point _Click(Button button)
        {
            double[] coords = ClickButton(0, 0, button.Width - 1, button.Height - 1);

            return new Point((int)coords[0], (int)coords[1]);
        }
    }

    public class EllipticalNormalDistribution : NormalDistribution
    {
        public EllipticalNormalDistribution(Random random, double deviation)
            : base(random, deviation)
        {
        }

        protected override Point _Click(Button button)
        {
            double rx = (button.Width - 1) / 2, ry = (button.Height - 1) / 2;

            for (int i = 0; i < 10; i++)
            {
                Point point = base._Click(button);

                double nx = (point.X - rx) / rx, ny = (point.Y - ry) / ry;

                if (Math.Sqrt(nx * nx + ny * ny) < 1.0) return point;
            }


            return new Point((int)rx, (int)ry);
        }
    }

    public struct Button
    {
        private int width;
        private int height;
        private int[,] clicks;

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public Bitmap GetBitmap()
        {

            Bitmap bitmap = new Bitmap(width, height);

            int max = 0;
            int min = int.MaxValue;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (clicks[x, y] > max)
                        max = clicks[x, y];

                    if (clicks[x, y] < min)
                        min = clicks[x, y];
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int r = 155 + (int)(100 * (double)(clicks[x, y]) / (double)max);
                    //bitmap.SetPixel(x, y, Color.FromArgb(30, r, 0, 0));
                    if (clicks[x, y] == 0)
                        continue;//bitmap.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    else
                        bitmap.SetPixel(x, y, Color.FromArgb(r, 0, 0));
                }
            }

            return bitmap;
        }

        public void AddClick(Point point)
        {
            clicks[point.X, point.Y]++;
        }

        public Button(int width, int height)
        {
            clicks = new int[width, height];

            this.width = width;
            this.height = height;
        }
    }
}
