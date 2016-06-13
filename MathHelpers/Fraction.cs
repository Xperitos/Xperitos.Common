using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xperitos.Common.MathHelpers
{
    // http://csharphelper.com/blog/2015/06/make-a-fraction-class-in-c/
    /// <summary>
    /// Represents a fraction number using Numerator and Denominator.
    /// </summary>
    public class Fraction
    {
        public Fraction() : this(0)
        {
        }

        public Fraction(long numerator, long denominator = 1)
        {
            if (denominator == 0)
                throw new DivideByZeroException("Denominator can't be 0");

            // Simplify the sign.
            if (denominator < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }

            // Factor out the greatest common divisor of the
            // numerator and the denominator.
            var gcd_ab = (long)MathUtils.GCD(numerator, denominator);
            Numerator = numerator / gcd_ab;
            Denominator = denominator / gcd_ab;
        }

        /// <summary>
        /// Convert from simple number
        /// </summary>
        /// <param name="numerator"></param>
        public static implicit operator Fraction(long numerator) => new Fraction(numerator, 1);

        public long Numerator { get; }
        public long Denominator { get; }

        public override string ToString()
        {
            return $"{Numerator}/{Denominator}";
        }

        /// <summary>
        /// Return the value (With possible loss of fraction).
        /// </summary>
        public double Value => Numerator / (double)Denominator;

        /// <summary>
        /// Return the value (with possible loss of fraction) - using decimal for greater precision.
        /// </summary>
        public decimal DecimalValue => Numerator / (decimal)Denominator;

        // Return -a.
        public static Fraction operator -(Fraction a)
        {
            return new Fraction(-a.Numerator, a.Denominator);
        }

        // Return a + b.
        public static Fraction operator +(Fraction a, Fraction b)
        {
            // Get the denominators' greatest common divisor.
            var gcd_ab = (long)MathUtils.GCD(a.Denominator, b.Denominator);

            var numer =
                a.Numerator * (b.Denominator / gcd_ab) +
                b.Numerator * (a.Denominator / gcd_ab);
            var denom =
                a.Denominator * (b.Denominator / gcd_ab);
            return new Fraction(numer, denom);
        }

        // Return a - b.
        public static Fraction operator -(Fraction a, Fraction b)
        {
            return a + -b;
        }

        // Return a * b.
        public static Fraction operator *(Fraction a, Fraction b)
        {
            // Swap numerators and denominators to simplify.
            Fraction result1 = new Fraction(a.Numerator, b.Denominator);
            Fraction result2 = new Fraction(b.Numerator, a.Denominator);

            return new Fraction(
                result1.Numerator * result2.Numerator,
                result1.Denominator * result2.Denominator);
        }

        // Return a / b.
        public static Fraction operator /(Fraction a, Fraction b)
        {
            return a * new Fraction(b.Denominator, b.Numerator);
        }
    }
}
