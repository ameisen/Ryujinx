using System.Collections.Generic;

namespace Ryujinx.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<int> To(this int from, int to)
        {
            if (from < to)
            {
                while (from <= to)
                {
                    yield return from++;
                }
            }
            else
            {
                while (from >= to)
                {
                    yield return from--;
                }
            }
        }

        public static IEnumerable<long> To(this long from, long to)
        {
            if (from < to)
            {
                while (from <= to)
                {
                    yield return from++;
                }
            }
            else
            {
                while (from >= to)
                {
                    yield return from--;
                }
            }
        }

        public static IEnumerable<int> Until(this int from, int to)
        {
            if (from < to)
            {
                while (from < to)
                {
                    yield return from++;
                }
            }
            else
            {
                while (from > to)
                {
                    yield return from--;
                }
            }
        }

        public static IEnumerable<long> Until(this long from, long to)
        {
            if (from < to)
            {
                while (from < to)
                {
                    yield return from++;
                }
            }
            else
            {
                while (from > to)
                {
                    yield return from--;
                }
            }
        }
    }
}
