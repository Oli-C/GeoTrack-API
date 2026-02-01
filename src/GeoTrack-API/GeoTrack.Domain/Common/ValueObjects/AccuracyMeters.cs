using System;

namespace GeoTrack.Domain.Common.ValueObjects
{
    /// <summary>
    /// Represents horizontal positional accuracy in meters.
    /// Must be >= 0.
    /// </summary>
    public struct AccuracyMeters : IEquatable<AccuracyMeters>
    {
        private readonly double _value;
        public double Value => _value;

        private AccuracyMeters(double value)
        {
            _value = value;
        }

        public static AccuracyMeters From(double value)
        {
            if (double.IsNaN(value))
                throw new ArgumentException("Accuracy cannot be NaN.", nameof(value));

            if (double.IsInfinity(value))
                throw new ArgumentException("Accuracy cannot be infinite.", nameof(value));

            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Accuracy must be >= 0.");

            return new AccuracyMeters(value);
        }

        public static AccuracyMeters? FromNullable(double? value)
        {
            if (!value.HasValue) return null;
            return From(value.Value);
        }

        public bool Equals(AccuracyMeters other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            return obj is AccuracyMeters other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static bool operator ==(AccuracyMeters left, AccuracyMeters right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AccuracyMeters left, AccuracyMeters right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return _value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}