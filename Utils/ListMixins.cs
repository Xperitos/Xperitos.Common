using System;
using System.Collections.Generic;

namespace Xperitos.Common.Utils
{
    public static class ListMixins
    {
        /// <summary>
        /// Search for item in the list. Providing a transform func for the list items for search.
        /// Note: The list must be sorted.
        /// </summary>
        /// <returns>Same as <see cref="List{T}.BinarySearch(T)"/></returns>
        public static Int32 BinarySearchIndexOf<TElem, TValue>(this IList<TElem> list, TValue value, Func<TElem, TValue> transformFunc, IComparer<TValue> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            comparer = comparer ?? Comparer<TValue>.Default;

            Int32 lower = 0;
            Int32 upper = list.Count - 1;

            while (lower <= upper)
            {
                Int32 middle = lower + (upper - lower) / 2;
                Int32 comparisonResult = comparer.Compare(value, transformFunc(list[middle]));
                if (comparisonResult == 0)
                    return middle;
                else if (comparisonResult < 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            return ~lower;
        }

        /// <summary>
        /// Linear search for in item in the list.
        /// </summary>
        /// <returns>Same as <see cref="List{T}.IndexOf(T)"/></returns>
        public static Int32 LinearIndexOf<TValue>(this IList<TValue> list, TValue value, IComparer<TValue> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            comparer = comparer ?? Comparer<TValue>.Default;

            for (int i = 0; i < list.Count; ++i)
            {
                if ( comparer.Compare(list[i], value) == 0 )
                    return i;
            }

            return -1;
        }
    }
}
