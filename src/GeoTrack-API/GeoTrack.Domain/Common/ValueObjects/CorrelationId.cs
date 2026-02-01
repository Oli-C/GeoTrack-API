using System;

namespace GeoTrack.Domain.Common.ValueObjects
{
    /// <summary>
    /// Client-provided correlation identifier for traceability / idempotency.
    /// Trimmed; cannot be empty/whitespace; maximum length 128.
    /// </summary>
    public struct CorrelationId : IEquatable<CorrelationId>
    {
        public const int MaxLength = 128;

        private readonly string _value;
        public string Value { get { return _value; } }

        private CorrelationId(string value)
        {
            _value = value;
        }

        public static CorrelationId From(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var trimmed = value.Trim();
            if (trimmed.Length == 0)
                throw new ArgumentException("CorrelationId cannot be empty.", nameof(value));

            if (trimmed.Length > MaxLength)
                throw new ArgumentOutOfRangeException(nameof(value), value, "CorrelationId must be 128 characters or fewer.");

            return new CorrelationId(trimmed);
        }

        public override string ToString()
        {
            return _value;
        }

        public bool Equals(CorrelationId other)
        {
            return string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CorrelationId)) return false;
            return Equals((CorrelationId)obj);
        }

        public override int GetHashCode()
        {
            return _value == null ? 0 : _value.GetHashCode();
        }

        public static bool operator ==(CorrelationId left, CorrelationId right) { return left.Equals(right); }
        public static bool operator !=(CorrelationId left, CorrelationId right) { return !left.Equals(right); }

        public static implicit operator string(CorrelationId id) { return id._value; }
    }
}
