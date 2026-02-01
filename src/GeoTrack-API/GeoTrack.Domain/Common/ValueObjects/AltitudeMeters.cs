using System;
using System.Globalization;

namespace GeoTrack.Domain.Common.ValueObjects
{
    /// <summary>
    /// Represents an altitude in meters.
    /// Optional for GPS fixes.
    /// </summary>
    public struct AltitudeMeters : IEquatable<AltitudeMeters>, IComparable<AltitudeMeters>
    {
        private readonly double _value;
        public double Value { get { return _value; } }

        private AltitudeMeters(double value)
        {
            _value = value;
        }

        public static AltitudeMeters From(double value)
        {
            if (double.IsNaN(value))
                throw new ArgumentException("Altitude cannot be NaN.", nameof(value));

            if (double.IsInfinity(value))
                throw new ArgumentException("Altitude cannot be infinite.", nameof(value));

            // No domain upper/lower bound; negative altitudes (below sea level) are valid.
            return new AltitudeMeters(value);
        }

        public static AltitudeMeters? FromNullable(double? value)
        {
            if (!value.HasValue) return null;
            return From(value.Value);
        }

        public int CompareTo(AltitudeMeters other)
        {
            return _value.CompareTo(other._value);
        }

        public bool Equals(AltitudeMeters other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AltitudeMeters)) return false;
            return Equals((AltitudeMeters)obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString("F2", CultureInfo.InvariantCulture);
        }

        public static bool operator ==(AltitudeMeters left, AltitudeMeters right) { return left.Equals(right); }
        public static bool operator !=(AltitudeMeters left, AltitudeMeters right) { return !left.Equals(right); }
        public static bool operator <(AltitudeMeters left, AltitudeMeters right) { return left._value < right._value; }
        public static bool operator >(AltitudeMeters left, AltitudeMeters right) { return left._value > right._value; }
        public static bool operator <=(AltitudeMeters left, AltitudeMeters right) { return left._value <= right._value; }
        public static bool operator >=(AltitudeMeters left, AltitudeMeters right) { return left._value >= right._value; }

        public static explicit operator double(AltitudeMeters altitude) { return altitude._value; }
    }
}
