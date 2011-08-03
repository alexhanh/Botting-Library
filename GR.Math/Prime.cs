using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Math
{
    public class Prime
    {
        public static int[] LUT52 = {   1, 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 
                                        103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211,
                                        223, 227, 229, 233, 239 };

        private static bool IsPrime(int n)
        {
            for (int i = 2; i * i <= n; i++)
                if (n % i == 0)
                    return false;
            return true;
        }

        public static List<int> GetPrimes(int n_first)
        {
            List<int> primes = new List<int>();
            int count = 0;
            for (int n = 2; count < n_first; n++)
                if (IsPrime(n))
                {
                    primes.Add(n);
                    count++;
                }

            return primes;
        }
    }
}
