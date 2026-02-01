using System;
using System.Globalization;

namespace GeoTrack.Domain.Common.ValueObjects
{
    /// <summary>
    /// Represents a speed in kilometers per hour.
    /// Must be >= 0. No upper bound is imposed.
    /// </summary>
    public struct SpeedKph : IEquatable<SpeedKph>, IComparable<SpeedKph>
    {
        public const double MinValue = 0.0;

        private readonly double _value;
        public double Value => _value;

        private SpeedKph(double value)
        {
            _value = value;
        }

        // -----------------------------
        // Factory
        // -----------------------------

        public static SpeedKph From(double value)
        {
            if (double.IsNaN(value))
                throw new ArgumentException("Speed cannot be NaN.", nameof(value));

            if (double.IsInfinity(value))
                throw new ArgumentException("Speed cannot be infinite.", nameof(value));

            if (value < MinValue)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Speed must be greater than or equal to 0.");

            return new SpeedKph(value);
        }

        public static SpeedKph? FromNullable(double? value)
        {
            if (!value.HasValue) return null;
            return From(value.Value);
        }

        public static bool TryFrom(double value, out SpeedKph speed)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value) ||
                value < MinValue)
            {
                speed = default(SpeedKph);
                return false;
            }

            speed = new SpeedKph(value);
            return true;
        }

        // -----------------------------
        // Derived values
        // -----------------------------

        public double InMetersPerSecond
        {
            get { return _value / 3.6; }
        }

        public double InMilesPerHour
        {
            get { return _value * 0.621371; }
        }

        public bool IsStopped
        {
            get { return _value == 0; }
        }

        // -----------------------------
        // Arithmetic
        // -----------------------------

        public SpeedKph Add(double delta)
        {
            return From(_value + delta);
        }

        public static SpeedKph operator +(SpeedKph speed, double delta)
        {
            return speed.Add(delta);
        }

        public static SpeedKph operator -(SpeedKph speed, double delta)
        {
            return speed.Add(-delta);
        }

        public static double operator -(SpeedKph a, SpeedKph b)
        {
            return a._value - b._value;
        }

        // -----------------------------
        // Comparisons
        // -----------------------------

        public int CompareTo(SpeedKph other)
        {
            return _value.CompareTo(other._value);
        }

        public bool Equals(SpeedKph other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SpeedKph)) return false;
            return Equals((SpeedKph)obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static bool operator ==(SpeedKph left, SpeedKph right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SpeedKph left, SpeedKph right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(SpeedKph a, SpeedKph b)
        {
            return a._value < b._value;
        }

        public static bool operator >(SpeedKph a, SpeedKph b)
        {
            return a._value > b._value;
        }

        public static bool operator <=(SpeedKph a, SpeedKph b)
        {
            return a._value <= b._value;
        }

        public static bool operator >=(SpeedKph a, SpeedKph b)
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

        public static explicit operator double(SpeedKph speed)
        {
            return speed._value;
        }

        // -----------------------------
        // Known constants
        // -----------------------------

        public static readonly SpeedKph Zero = new SpeedKph(0);
    }
}
