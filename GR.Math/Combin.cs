using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Math
{
    public class Combin
    {
        private int[] comb;
        private int n, k;

        public int[] Combo { get { return comb; } }
        public int N { get { return n; } }
        public int K { get { return k; } }

        public long TotalCombs
        {
            get
            {
                //return Factorial(n) / (Factorial(k)*Factorial(n-k));
                int combs = 1;
                for (int i = n; i > n - k; i--)
                {
                    combs *= i;
                }

                return combs / Factorial(k);
            }
        }

        public bool NextComb()
        {
            int i = k - 1;
            comb[i]++;
            while ((i > 0) && (comb[i] >= n - k + 1 + i))
            {
                i--;
                comb[i]++;
            }

            if (comb[0] > n - k)
                return false;

            for (i = i + 1; i < k; i++)
                comb[i] = comb[i - 1] + 1;

            return true;
        }

        public Combin(int n, int k)
        {
            this.n = n;
            this.k = k;

            comb = new int[k];
            for (int i = 0; i < k; i++)
                comb[i] = i;
        }

        public static int Factorial(int n)
        {
            int fact = 1;
            for (int i = 1; i <= n; i++)
            {
                fact *= i;
            }

            return fact;
        }
    }

    public class CombinBuilder<T>
    {
        private T[] objects;
        private Combin comb;

        public CombinBuilder(ICollection<T> objects, int k)
        {
            this.objects = new T[objects.Count];
            objects.CopyTo(this.objects, 0);
            //this.objects = new List<T>(objects);

            Reset(objects.Count, k);
        }

        public long TotalCombs { get { return comb.TotalCombs; } }

        public T[] Current()
        {
            T[] c = new T[comb.K];

            int j = 0;
            foreach (int i in comb.Combo)
            {
                c[j] = objects[i];
                j++;
            }

            return c;
        }

        public bool NextComb()
        {
            return comb.NextComb();
        }

        public void Reset(int n, int k)
        {
            comb = new Combin(n, k);
        }
    }

    public class Perm
    {
        private int[] perm;
        private int n;

        public Perm(int n)
        {
            this.n = n;

            perm = new int[n];
            for (int i = 0; i < n; i++)
                perm[i] = i;
        }

        public int[] Permu { get { return perm; } }

        public bool NextPermRep()
        {
            int i = n - 1;
            perm[i]++;

            while ((i >= 0) && (perm[i] > n))
            {
                perm[i] = 1;
                i--;
                if (i >= 0)
                    perm[i]++;
            }

            if (i < 0)
                return false;

            return true;
        }

        public bool NextPermLex()
        {
            int i = n - 2;
            while ((i >= 0) && (perm[i] > perm[i + 1]))
                i--;

            if (i < 0)
                return false;

            int k = n - 1;
            while (perm[i] > perm[k])
                k--;

            Swap(ref perm[i], ref perm[k]);

            int j;
            k = 0;
            for (j = i + 1; j < (n + i) / 2 + 1; j++, k++)
                Swap(ref perm[j], ref perm[n - k - 1]);

            return true;
        }

        private static void Swap(ref int a, ref int b)
        {
            a = a + b - (b = a);
        }

        #region C# Iterator method
        // Returns an enumeration of enumerators, one for each permutation
        // of the input.
        public static IEnumerable<IEnumerable<T>> Permute<T>(IEnumerable<T> list, int count)
        {
            if (count == 0)
            {
                yield return new T[0];
            }
            else
            {
                int startingElementIndex = 0;
                foreach (T startingElement in list)
                {
                    IEnumerable<T> remainingItems = AllExcept(list, startingElementIndex);

                    foreach (IEnumerable<T> permutationOfRemainder in Permute(remainingItems, count - 1))
                    {
                        yield return Concat<T>(
                            new T[] { startingElement },
                            permutationOfRemainder);
                    }
                    startingElementIndex += 1;
                }
            }
        }

        // Enumerates over contents of both lists.
        private static IEnumerable<T> Concat<T>(IEnumerable<T> a, IEnumerable<T> b)
        {
            foreach (T item in a) { yield return item; }
            foreach (T item in b) { yield return item; }
        }

        // Enumerates over all items in the input, skipping over the item
        // with the specified offset.
        private static IEnumerable<T> AllExcept<T>(IEnumerable<T> input, int indexToSkip)
        {
            int index = 0;
            foreach (T item in input)
            {
                if (index != indexToSkip) yield return item;
                index += 1;
            }
        }
        #endregion
    }

    public class PermBuilder<T>
    {
        private List<T> objects;
        private Perm perm;

        public PermBuilder(List<T> objects)
        {
            this.objects = objects;
            Reset(objects.Count);
        }

        public T[] Current()
        {
            T[] p = new T[objects.Count];

            int i = 0;
            foreach (int e in perm.Permu)
            {
                p[i] = objects[e];
                i++;
            }

            return p;
        }

        public bool NextPerm()
        {
            return perm.NextPermLex();
        }

        public void Reset(int n)
        {
            perm = new Perm(n);
        }
    }
}
