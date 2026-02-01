using System;

namespace GeoTrack.Domain.Common.ValueObjects
{
    /// <summary>
    /// Optional device-provided monotonic sequence number.
    /// Must be >= 0.
    /// </summary>
    public struct DeviceSequence : IEquatable<DeviceSequence>, IComparable<DeviceSequence>
    {
        public const long MinValue = 0;

        private readonly long _value;
        public long Value { get { return _value; } }

        private DeviceSequence(long value)
        {
            _value = value;
        }

        public static DeviceSequence From(long value)
        {
            if (value < MinValue)
                throw new ArgumentOutOfRangeException(nameof(value), value, "DeviceSequence must be >= 0.");

            return new DeviceSequence(value);
        }

        public static DeviceSequence? FromNullable(long? value)
        {
            if (!value.HasValue) return null;
            return From(value.Value);
        }

        public int CompareTo(DeviceSequence other)
        {
            return _value.CompareTo(other._value);
        }

        public bool Equals(DeviceSequence other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DeviceSequence)) return false;
            return Equals((DeviceSequence)obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static bool operator ==(DeviceSequence left, DeviceSequence right) { return left.Equals(right); }
        public static bool operator !=(DeviceSequence left, DeviceSequence right) { return !left.Equals(right); }
        public static bool operator <(DeviceSequence left, DeviceSequence right) { return left._value < right._value; }
        public static bool operator >(DeviceSequence left, DeviceSequence right) { return left._value > right._value; }
        public static bool operator <=(DeviceSequence left, DeviceSequence right) { return left._value <= right._value; }
        public static bool operator >=(DeviceSequence left, DeviceSequence right) { return left._value >= right._value; }

        public static explicit operator long(DeviceSequence seq) { return seq._value; }
    }
}
