using System.Collections.Generic;

namespace Xperitos.Common.Utils
{
    public static class ArrayMixins
    {
        public static bool ArraySequenceEqual<T>(this T[] left, T[] right, int leftOffset = 0, int rightOffset = 0, int count = -1, IEqualityComparer<T> comparer = null)
        {
            int leftSize = left.Length - leftOffset;
            int rightSize = right.Length - rightOffset;

            if (count == -1)
            {
                // Size mismatch?
                if ( leftSize != rightSize )
                    return false;

                count = leftSize;
            }
            else
            {
                if ( leftSize < count )
                    return false;
                if ( rightSize < count )
                    return false;
            }

            if (comparer == null)
                comparer = EqualityComparer<T>.Default;

            for ( int i = 0; i < count; ++i)
            {
                if ( !comparer.Equals(left[leftOffset + i], right[rightOffset + i]) )
                    return false;
            }

            return true;
        }
    }
}
