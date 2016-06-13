using System;

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
    }
}