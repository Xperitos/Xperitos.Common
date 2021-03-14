using System;
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

		/// <summary>
		/// Find nearest element in array.
		/// Runs efficiently similar to binary search in O(logn).
		/// </summary>
		/// <param name="arr"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static double? FindNearest(this double[] arr, double item)
		{
			if (arr == null)
				return null;

			int left = 0;
			int right = arr.Length - 1;
			int mid = 0;

			// Perform binary search
			while (left < right)
			{
				mid = (right + left) / 2;
				if (item < arr[mid])
					right = mid - 1;
				else if (item > arr[mid])
					left = mid + 1;
				else
					// arr[mid] == item
					return arr[mid];
			}

			// Find the nearest element

			// Set nearest as the left
			double minDiff = Math.Abs(arr[left] - item);
			int minPos = left;

			// Check left-1
			if (left - 1 >= 0 && arr[left - 1] < minDiff)
			{
				minDiff = Math.Abs(arr[left - 1] - item);
				minPos = left - 1;
			}

			// Check left+1
			if (left + 1 < arr.Length && arr[left + 1] < minDiff)
			{
				minDiff = Math.Abs(arr[left + 1] - item);
				minPos = left + 1;
			}

			return arr[minPos];
		}

	}
}
