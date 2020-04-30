using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ARMeilleure.Common
{
    static class CollectionExtensions
    {
        public static T Back<T>([NotNull] this List<T> list) => list[list.Count - 1];
        public static T Back<T>([NotNull] this IList<T> list) => list[list.Count - 1];

        public static bool Resize<T>([NotNull] this List<T> list, [Range(0, int.MaxValue)] int newSize, T value = default)
        {
            if (newSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newSize));
            }
            if (list.Count < newSize)
            {
                // Enlarge
                return list.EnlargeTo(newSize, value);
            }
            else if (list.Count > newSize)
            {
                // Shrink
                return list.ShrinkTo(newSize);
            }
            return false;
        }

        public static bool EnlargeTo<T>([NotNull] this List<T> list, [Range(0, int.MaxValue)] int newSize, T value = default)
        {
            if (newSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newSize));
            }
            if (list.Count < newSize)
            {
                list.Capacity = Math.Max(list.Capacity, newSize);
                int diff = newSize - list.Count;
                while (diff-- > 0)
                {
                    list.Add(value);
                }
                return true;
            }
            return false;
        }

        public static bool ShrinkTo<T>([NotNull] this List<T> list, [Range(0, int.MaxValue)] int newSize, bool trim = false)
        {
            if (newSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newSize));
            }
            if (list.Count > newSize)
            {
                // Shrink
                int finalIndex = newSize - 1;
                for (int currentIndex = list.Count - 1; currentIndex > finalIndex; --currentIndex)
                {
                    list.RemoveAt(currentIndex);
                }
                return true;
            }

            if (trim)
            {
                list.TrimExcess();
            }

            return false;
        }

        public static bool TryFindIndex<T>([NotNull] this List<T> list, out int index, [NotNull] Predicate<T> predicate)
        {
            var idx = list.FindIndex(predicate);
            if (idx == -1)
            {
                index = default;
                return false;
            }
            index = idx;
            return true;
        }

        public delegate bool DualPredicate<in T1, in T2>(T1 a, T2 b);

        public static bool Any<T>([NotNull] this IList<T> list, [NotNull] DualPredicate<T, int> predicate)
        {
            for (int i = list.Count; i-- > 0;)
            {
                if (predicate(list[i], i))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool All<T>([NotNull] this IList<T> list, [NotNull] DualPredicate<T, int> predicate)
        {
            for (int i = list.Count; i-- > 0;)
            {
                if (!predicate(list[i], i))
                {
                    return false;
                }
            }
            return true;
        }

        public static T Get<T>([NotNull] this IList<T> list, int index)
        {
            index = (index >= 0) ? index : (index + list.Count);
            return list[index];
        }

#nullable enable
        public static T? TryGet<T>([NotNull] this IList<T> list, int index)
            where T : class
        {
            if (index < 0)
            {
                index += list.Count;
                if (index < 0)
                {
                    return null;
                }
            }
            else if (index >= list.Count)
            {
                return null;
            }
            return list[index];
        }
#nullable restore
    }
}
