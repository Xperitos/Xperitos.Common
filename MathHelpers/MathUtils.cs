using System;
using System.Collections.Generic;
using System.Linq;

namespace Xperitos.Common.MathHelpers
{
    /// <summary>
    /// Utils taken from various sources.
    /// </summary>
    public static class MathUtils
    {
        // http://csharphelper.com/blog/2014/08/calculate-the-greatest-common-divisor-gcd-and-least-common-multiple-lcm-of-two-integers-in-c/
        /// <summary>
        /// Use Euclid's algorithm to calculate the
        /// greatest common divisor (GCD) of two numbers.
        /// </summary>
        public static long GCD(long a, long b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);

            // Pull out remainders.
            for (;;)
            {
                long remainder = a % b;
                if (remainder == 0) return b;
                a = b;
                b = remainder;
            };
        }

        // http://csharphelper.com/blog/2014/08/calculate-the-greatest-common-divisor-gcd-and-least-common-multiple-lcm-of-two-integers-in-c/
        /// <summary>
        /// Return the least common multiple
        /// (LCM) of two numbers. 
        /// </summary>
        public static long LCM(long a, long b)
        {
            return a * b / GCD(a, b);
        }

		/// <summary>
		/// Calculates the std
		/// </summary>
		/// <param name="values">Values for calculation</param>
		/// <param name="average">Optional average value if it was already calculated (providing it 
		/// will make the calculation faster).</param>
		/// <returns></returns>
		public static double CalculateStdDev(IEnumerable<double> values, double? average = null)
        {
			//Compute/get the Average      
			double avg;
			if (average.HasValue)
				avg = average.Value;
			else
				avg = values.Average();


			double ret = 0;
            if (values.Count() > 0)
            {
                //Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => (d-avg)*(d-avg));
                //Put it all together      
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }

		/// <summary>
		/// Calculate the 50th percentile
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		public static double Calc50Percentile(List<double> values)
		{
			values.Sort();

			if (values.Count % 2 == 0)
			{
				// In case of even number of items in value
				var val1 = values[values.Count / 2];
				var val2 = values[(values.Count / 2) + 1];

				return (val1 + val2) / 2;
			}
			else
			{
				// In case of odd number of items in value
				var val = values[(values.Count + 1) / 2];

				return val;
			}
		}

	}
}