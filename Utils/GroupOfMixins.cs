using System;
using System.Collections.Generic;

namespace Xperitos.Common.Utils
{
    public static class GroupOfMixins
    {
        /// <summary>
        /// Split the source into groups of X items.
        /// </summary>
        public static IEnumerable<IList<T>> GroupOf<T>(this IEnumerable<T> source, int splitAmount)
        {
            if (splitAmount < 1)
                throw new ArgumentException();

            var currentList = new List<T>();
            foreach(var item in source)
            {
                currentList.Add(item);
                if (currentList.Count == splitAmount)
                {
                    yield return currentList;
                    currentList = new List<T>();
                }
            }

            // Return the last list.
            if (currentList.Count > 0)
                yield return currentList;
        }
    }
}
