using System;
using System.Globalization;

namespace GeoTrack.Domain.Common.ValueObjects
{
    /// <summary>
    /// Represents a geographic longitude in decimal degrees.
    /// Valid range is -180.0 to +180.0 inclusive.
    /// </summary>
    public struct Longitude : IEquatable<Longitude>, IComparable<Longitude>
    {
        public const double MinValue = -180.0;
        public const double MaxValue = 180.0;

        private readonly double _value;
        public double Value => _value;

        private Longitude(double value)
        {
            _value = value;
        }

        // -----------------------------
        // Factory
        // -----------------------------

        public static Longitude From(double value)
        {
            if (double.IsNaN(value))
                throw new ArgumentException("Longitude cannot be NaN.", nameof(value));

            if (double.IsInfinity(value))
                throw new ArgumentException("Longitude cannot be infinite.", nameof(value));

            if (value < MinValue || value > MaxValue)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Longitude must be between -180 and +180 degrees.");

            return new Longitude(value);
        }

        public static Longitude? FromNullable(double? value)
        {
            if (!value.HasValue) return null;
            return From(value.Value);
        }

        public static bool TryFrom(double value, out Longitude longitude)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value) ||
                value < MinValue ||
                value > MaxValue)
            {
                longitude = default(Longitude);
                return false;
            }

            longitude = new Longitude(value);
            return true;
        }

        // -----------------------------
        // Normalization
        // -----------------------------

        /// <summary>
        /// Clamps a raw value into the valid longitude range.
        /// Use only when you explicitly want correction instead of rejection.
        /// </summary>
        public static Longitude Clamp(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Longitude cannot be NaN or Infinity.", nameof(value));

            if (value < MinValue) return new Longitude(MinValue);
            if (value > MaxValue) return new Longitude(MaxValue);
            return new Longitude(value);
        }

        /// <summary>
        /// Wraps any finite value into the conventional [-180, +180] longitude range.
        /// Use this when you explicitly want normalization (e.g. 181 -> -179).
        /// </summary>
        public static Longitude Wrap(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Longitude cannot be NaN or Infinity.", nameof(value));

            // Normalize to [0, 360)
            var normalized = value % 360.0;
            if (normalized < 0) normalized += 360.0;

            // Shift to (-180, 180]
            if (normalized > 180.0) normalized -= 360.0;

            // Now in [-180, 180], except possibly 180 exactly; allow it.
            if (normalized < MinValue) normalized = MinValue;
            if (normalized > MaxValue) normalized = MaxValue;

            return new Longitude(normalized);
        }

        // -----------------------------
        // Derived values
        // -----------------------------

        public double ToRadians()
        {
            return _value * (Math.PI / 180.0);
        }

        public double AbsoluteDegrees
        {
            get { return Math.Abs(_value); }
        }

        public bool IsEasternHemisphere
        {
            get { return _value > 0; }
        }

        public bool IsWesternHemisphere
        {
            get { return _value < 0; }
        }

        public bool IsPrimeMeridian
        {
            get { return _value == 0; }
        }

        public bool IsInternationalDateLine
        {
            get { return _value == 180.0 || _value == -180.0; }
        }

        // -----------------------------
        // Arithmetic
        // -----------------------------

        public Longitude AddDegrees(double delta)
        {
            return From(_value + delta);
        }

        public static Longitude operator +(Longitude lon, double delta)
        {
            return lon.AddDegrees(delta);
        }

        public static Longitude operator -(Longitude lon, double delta)
        {
            return lon.AddDegrees(-delta);
        }

        public static double operator -(Longitude a, Longitude b)
        {
            return a._value - b._value;
        }

        // -----------------------------
        // Comparisons
        // -----------------------------

        public int CompareTo(Longitude other)
        {
            return _value.CompareTo(other._value);
        }

        public bool Equals(Longitude other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Longitude)) return false;
            return Equals((Longitude)obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static bool operator ==(Longitude left, Longitude right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Longitude left, Longitude right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Longitude a, Longitude b)
        {
            return a._value < b._value;
        }

        public static bool operator >(Longitude a, Longitude b)
        {
            return a._value > b._value;
        }

        public static bool operator <=(Longitude a, Longitude b)
        {
            return a._value <= b._value;
        }

        public static bool operator >=(Longitude a, Longitude b)
        {
            return a._value >= b._value;
        }

        // -----------------------------
        // Formatting
        // -----------------------------

        public override string ToString()
        {
            return _value.ToString("F6", CultureInfo.InvariantCulture);
        }

        public string ToString(string format)
        {
            return _value.ToString(format, CultureInfo.InvariantCulture);
        }

        // -----------------------------
        // Explicit conversions
        // -----------------------------

        public static explicit operator double(Longitude longitude)
        {
            return longitude._value;
        }

        // -----------------------------
        // Known constants
        // -----------------------------

        public static readonly Longitude PrimeMeridian = new Longitude(0);
        public static readonly Longitude DateLineEast = new Longitude(180);
        public static readonly Longitude DateLineWest = new Longitude(-180);
    }
}
