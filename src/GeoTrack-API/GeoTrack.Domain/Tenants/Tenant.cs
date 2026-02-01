using System;

namespace GeoTrack.API.Data.Entities
{
    /// <summary>
    /// Represents an organisation/account boundary (multi-tenant partition).
    /// </summary>
    public sealed class Tenant
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        private Tenant() { } // EF

        public Tenant(Guid id, string name, DateTime createdAtUtc)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id is required.", nameof(id));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.", nameof(name));

            if (createdAtUtc.Kind != DateTimeKind.Utc)
                throw new ArgumentException("createdAtUtc must be UTC.", nameof(createdAtUtc));

            Id = id;
            Name = name.Trim();
            CreatedAtUtc = createdAtUtc;
        }
    }
}
