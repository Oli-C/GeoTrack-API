using System;
using System.Globalization;

namespace GeoTrack.Domain.Common.ValueObjects
{
    /// <summary>
    /// Represents a heading/bearing in degrees.
    /// Normal valid range is [0, 360). (i.e., 0 inclusive, 360 exclusive)
    /// Many systems treat 360 as equivalent to 0; use Wrap/FromAllow360 if you want that.
    /// </summary>
    public struct HeadingDegrees : IEquatable<HeadingDegrees>, IComparable<HeadingDegrees>
    {
        public const double MinValueInclusive = 0.0;
        public const double MaxValueExclusive = 360.0;

        private readonly double _value;
        public double Value => _value;

        private HeadingDegrees(double value)
        {
            _value = value;
        }

        // -----------------------------
        // Factory
        // -----------------------------

        /// <summary>
        /// Creates a heading in the range [0, 360).
        /// Rejects NaN/Infinity and values outside the range.
        /// </summary>
        public static HeadingDegrees From(double value)
        {
            if (double.IsNaN(value))
                throw new ArgumentException("Heading cannot be NaN.", nameof(value));

            if (double.IsInfinity(value))
                throw new ArgumentException("Heading cannot be infinite.", nameof(value));

            if (value < MinValueInclusive || value >= MaxValueExclusive)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Heading must be in the range [0, 360).");

            return new HeadingDegrees(value);
        }

        public static HeadingDegrees? FromNullable(double? value)
        {
            if (!value.HasValue) return null;
            return From(value.Value);
        }

        public static bool TryFrom(double value, out HeadingDegrees heading)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value) ||
                value < MinValueInclusive ||
                value >= MaxValueExclusive)
            {
                heading = default(HeadingDegrees);
                return false;
            }

            heading = new HeadingDegrees(value);
            return true;
        }

        /// <summary>
        /// Accepts values in [0, 360], treating 360 as 0.
        /// Useful for integrations that send 360 instead of 0.
        /// </summary>
        public static HeadingDegrees FromAllow360(double value)
        {
            if (double.IsNaN(value))
                throw new ArgumentException("Heading cannot be NaN.", nameof(value));

            if (double.IsInfinity(value))
                throw new ArgumentException("Heading cannot be infinite.", nameof(value));

            if (value == 360.0) return new HeadingDegrees(0.0);

            return From(value);
        }

        // -----------------------------
        // Normalization
        // -----------------------------

        /// <summary>
        /// Wraps any finite value into [0, 360).
        /// Example: -10 -> 350, 370 -> 10.
        /// Use this when you explicitly want normalization instead of rejection.
        /// </summary>
        public static HeadingDegrees Wrap(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Heading cannot be NaN or Infinity.", nameof(value));

            var normalized = value % 360.0;
            if (normalized < 0) normalized += 360.0;

            // Ensure 360 normalizes to 0
            if (normalized >= 360.0) normalized = 0.0;

            return new HeadingDegrees(normalized);
        }

        /// <summary>
        /// Clamps a finite value into [0, 360) by bounding.
        /// Note: Unlike Wrap, Clamp does not preserve angular meaning for out-of-range values.
        /// </summary>
        public static HeadingDegrees Clamp(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Heading cannot be NaN or Infinity.", nameof(value));

            if (value < 0) return new HeadingDegrees(0);
            if (value >= 360) return new HeadingDegrees(359.999999); // best-effort clamp
            return new HeadingDegrees(value);
        }

        // -----------------------------
        // Derived values
        // -----------------------------

        public double ToRadians()
        {
            return _value * (Math.PI / 180.0);
        }

        /// <summary>
        /// Returns the smallest angular difference from this heading to another in degrees, in [0, 180].
        /// </summary>
        public double SmallestDifferenceTo(HeadingDegrees other)
        {
            var diff = Math.Abs(_value - other._value) % 360.0;
            return diff > 180.0 ? 360.0 - diff : diff;
        }

        // -----------------------------
        // Arithmetic
        // -----------------------------

        /// <summary>
        /// Adds delta degrees and wraps the result into [0, 360).
        /// </summary>
        public HeadingDegrees AddWrapped(double delta)
        {
            return Wrap(_value + delta);
        }

        public static HeadingDegrees operator +(HeadingDegrees heading, double delta)
        {
            return heading.AddWrapped(delta);
        }

        public static HeadingDegrees operator -(HeadingDegrees heading, double delta)
        {
            return heading.AddWrapped(-delta);
        }

        // -----------------------------
        // Comparisons
        // -----------------------------

        public int CompareTo(HeadingDegrees other)
        {
            return _value.CompareTo(other._value);
        }

        public bool Equals(HeadingDegrees other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is HeadingDegrees)) return false;
            return Equals((HeadingDegrees)obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static bool operator ==(HeadingDegrees left, HeadingDegrees right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HeadingDegrees left, HeadingDegrees right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(HeadingDegrees a, HeadingDegrees b)
        {
            return a._value < b._value;
        }

        public static bool operator >(HeadingDegrees a, HeadingDegrees b)
        {
            return a._value > b._value;
        }

        public static bool operator <=(HeadingDegrees a, HeadingDegrees b)
        {
            return a._value <= b._value;
        }

        public static bool operator >=(HeadingDegrees a, HeadingDegrees b)
        {
            return a._value >= b._value;
        }

        // -----------------------------
        // Formatting
        // -----------------------------

        public override string ToString()
        {
            return _value.ToString("F2", CultureInfo.InvariantCulture);
        }

        public string ToString(string format)
        {
            return _value.ToString(format, CultureInfo.InvariantCulture);
        }

        // -----------------------------
        // Explicit conversions
        // -----------------------------

        public static explicit operator double(HeadingDegrees heading)
        {
            return heading._value;
        }

        // -----------------------------
        // Known constants
        // -----------------------------

        public static readonly HeadingDegrees North = new HeadingDegrees(0);
        public static readonly HeadingDegrees East = new HeadingDegrees(90);
        public static readonly HeadingDegrees South = new HeadingDegrees(180);
        public static readonly HeadingDegrees West = new HeadingDegrees(270);
    }
}
