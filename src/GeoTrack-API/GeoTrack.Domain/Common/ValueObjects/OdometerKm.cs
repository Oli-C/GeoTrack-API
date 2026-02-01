using System;
using System.Globalization;

namespace GeoTrack.Domain.Common.ValueObjects
{
    /// <summary>
    /// Represents a vehicle odometer reading in kilometres.
    /// Must be >= 0.
    /// </summary>
    public struct OdometerKm : IEquatable<OdometerKm>, IComparable<OdometerKm>
    {
        public const double MinValue = 0.0;

        private readonly double _value;
        public double Value { get { return _value; } }

        private OdometerKm(double value)
        {
            _value = value;
        }

        public static OdometerKm From(double value)
        {
            if (double.IsNaN(value))
                throw new ArgumentException("Odometer cannot be NaN.", nameof(value));

            if (double.IsInfinity(value))
                throw new ArgumentException("Odometer cannot be infinite.", nameof(value));

            if (value < MinValue)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Odometer must be >= 0.");

            return new OdometerKm(value);
        }

        public static OdometerKm? FromNullable(double? value)
        {
            if (!value.HasValue) return null;
            return From(value.Value);
        }

        public int CompareTo(OdometerKm other)
        {
            return _value.CompareTo(other._value);
        }

        public bool Equals(OdometerKm other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is OdometerKm)) return false;
            return Equals((OdometerKm)obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString("F3", CultureInfo.InvariantCulture);
        }

        public static bool operator ==(OdometerKm left, OdometerKm right) { return left.Equals(right); }
        public static bool operator !=(OdometerKm left, OdometerKm right) { return !left.Equals(right); }
        public static bool operator <(OdometerKm left, OdometerKm right) { return left._value < right._value; }
        public static bool operator >(OdometerKm left, OdometerKm right) { return left._value > right._value; }
        public static bool operator <=(OdometerKm left, OdometerKm right) { return left._value <= right._value; }
        public static bool operator >=(OdometerKm left, OdometerKm right) { return left._value >= right._value; }

        public static explicit operator double(OdometerKm odometer) { return odometer._value; }
    }
}
