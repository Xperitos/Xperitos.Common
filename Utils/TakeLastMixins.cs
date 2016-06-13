using System.Collections.Generic;
using System.Linq;

namespace Xperitos.Common.Utils
{
    public static class TakeLastMixins
    {
        /// <summary>
        /// Return all elements except the last X count
        /// </summary>
        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> list, int count)
        {
            // Try fast track first.
            ICollection<T> collection = list as ICollection<T>;
            if (collection == null)
                collection = list.ToList();

            if (collection.Count <= count)
                return Enumerable.Empty<T>();

            return collection.Take(collection.Count - count);
        }

        /// <summary>
        /// Returns the last <paramref name="count"/> elements.
        /// </summary>
        /// <remarks>Function doesn't return until enumeration completes</remarks>
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> list, int count)
        {
            // Try fast track first.
            ICollection<T> collection = list as ICollection<T>;
            if (collection != null)
            {
                if ( collection.Count <= count )
                    return collection;

                return collection.Skip(collection.Count - count);
            }

            Queue<T> result = new Queue<T>(count + 1);
            foreach (var item in list)
            {
                result.Enqueue(item);
                if ( result.Count > count )
                    result.Dequeue();
            }

            return result;
        }
    }
}
