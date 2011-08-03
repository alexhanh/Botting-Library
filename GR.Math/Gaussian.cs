using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Math
{
    public class Gaussian
    {
        private static bool uselast = true;
        private static double next_gaussian = 0.0;
        private static Random random = new Random();

        public static double BoxMuller()
        {
            if (uselast) 
            { 
                uselast = false;
                return next_gaussian;
            }
            else
            {
                double v1, v2, s;
                do
                {
                    v1 = 2.0 * random.NextDouble() - 1.0;
                    v2 = 2.0 * random.NextDouble() - 1.0;
                    s = v1 * v1 + v2 * v2;
                } while (s >= 1.0 || s == 0);

                s = System.Math.Sqrt((-2.0 * System.Math.Log(s)) / s);

                next_gaussian = v2 * s;
                uselast = true;
                return v1 * s;
            }
        }

        public static double BoxMuller(double mean, double standard_deviation)
        {
            return mean + BoxMuller() * standard_deviation;
        }

        public static int Next(int mean, int std_away_from_mean, double std)
        {
            return (int)System.Math.Round(BoxMuller(mean, std_away_from_mean / std), MidpointRounding.ToEven);
        }

        public static int Next(int mean, int min, int max, int std_away_from_mean, double std)
        {
            int r;
            while ((r = Next(mean, std_away_from_mean, std)) > max || r < min) ;
            return r;
        }
    }
}
