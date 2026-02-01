using System;
using System.Globalization;

namespace GeoTrack.Domain.Common.ValueObjects
{
    /// <summary>
    /// Represents a geographic latitude in decimal degrees.
    /// Valid range is -90.0 to +90.0 inclusive.
    /// </summary>
    public struct Latitude : IEquatable<Latitude>, IComparable<Latitude>
    {
        public const double MinValue = -90.0;
        public const double MaxValue = 90.0;

        private readonly double _value;
        public double Value => _value;

        private Latitude(double value)
        {
            _value = value;
        }

        // -----------------------------
        // Factory
        // -----------------------------

        public static Latitude From(double value)
        {
            if (double.IsNaN(value))
                throw new ArgumentException("Latitude cannot be NaN.", nameof(value));

            if (double.IsInfinity(value))
                throw new ArgumentException("Latitude cannot be infinite.", nameof(value));

            if (value < MinValue || value > MaxValue)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Latitude must be between -90 and +90 degrees.");

            return new Latitude(value);
        }

        public static Latitude? FromNullable(double? value)
        {
            if (!value.HasValue) return null;
            return From(value.Value);
        }

        public static bool TryFrom(double value, out Latitude latitude)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value) ||
                value < MinValue ||
                value > MaxValue)
            {
                latitude = default(Latitude);
                return false;
            }

            latitude = new Latitude(value);
            return true;
        }

        // -----------------------------
        // Normalization
        // -----------------------------

        /// <summary>
        /// Clamps a raw value into the valid latitude range.
        /// Use only when you explicitly want correction instead of rejection.
        /// </summary>
        public static Latitude Clamp(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Latitude cannot be NaN or Infinity.", nameof(value));

            if (value < MinValue) return new Latitude(MinValue);
            if (value > MaxValue) return new Latitude(MaxValue);
            return new Latitude(value);
        }

        // -----------------------------
        // Derived values
        // -----------------------------

        public double ToRadians()
        {
            return _value * (Math.PI / 180.0);
        }

        public double DistanceFromEquatorDegrees
        {
            get { return Math.Abs(_value); }
        }

        public bool IsNorthernHemisphere
        {
            get { return _value > 0; }
        }

        public bool IsSouthernHemisphere
        {
            get { return _value < 0; }
        }

        public bool IsEquator
        {
            get { return _value == 0; }
        }

        // -----------------------------
        // Arithmetic
        // -----------------------------

        public Latitude AddDegrees(double delta)
        {
            return From(_value + delta);
        }

        public static Latitude operator +(Latitude lat, double delta)
        {
            return lat.AddDegrees(delta);
        }

        public static Latitude operator -(Latitude lat, double delta)
        {
            return lat.AddDegrees(-delta);
        }

        public static double operator -(Latitude a, Latitude b)
        {
            return a._value - b._value;
        }

        // -----------------------------
        // Comparisons
        // -----------------------------

        public int CompareTo(Latitude other)
        {
            return _value.CompareTo(other._value);
        }

        public bool Equals(Latitude other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Latitude)) return false;
            return Equals((Latitude)obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static bool operator ==(Latitude left, Latitude right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Latitude left, Latitude right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Latitude a, Latitude b)
        {
            return a._value < b._value;
        }

        public static bool operator >(Latitude a, Latitude b)
        {
            return a._value > b._value;
        }

        public static bool operator <=(Latitude a, Latitude b)
        {
            return a._value <= b._value;
        }

        public static bool operator >=(Latitude a, Latitude b)
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

        public static explicit operator double(Latitude latitude)
        {
            return latitude._value;
        }

        // -----------------------------
        // Known constants
        // -----------------------------

        public static readonly Latitude NorthPole = new Latitude(90);
        public static readonly Latitude SouthPole = new Latitude(-90);
        public static readonly Latitude Equator = new Latitude(0);
    }
}