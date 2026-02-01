using System;

namespace GeoTrack.API.Data.Entities
{
    public sealed class Vehicle
    {
        public Guid TenantId { get; private set; }

        public Guid Id { get; private set; }

        public VehicleIdentity Identity { get; private set; }

        public VehicleStatus Status { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        // Optimistic concurrency is handled by PostgreSQL `xmin` (configured in EF as a shadow property).

        // Latest location is stored in vehicle_latest_location (one row per vehicle).

        // History removed: vehicle_location_progress table is no longer used.

        private Vehicle() { } // EF

        public Vehicle(Guid tenantId, Guid id, VehicleIdentity identity, DateTime createdAtUtc)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("TenantId is required.", nameof(tenantId));

            if (id == Guid.Empty)
                throw new ArgumentException("Id is required.", nameof(id));

            if (createdAtUtc.Kind != DateTimeKind.Utc)
                throw new ArgumentException("createdAtUtc must be UTC.", nameof(createdAtUtc));

            if (identity == null)
                throw new ArgumentNullException(nameof(identity));

            TenantId = tenantId;
            Id = id;
            Identity = identity;
            CreatedAtUtc = createdAtUtc;
            Status = VehicleStatus.Active;
        }

        // -----------------------------
        // Lifecycle
        // -----------------------------

        public void Activate()
        {
            if (Status == VehicleStatus.Decommissioned)
                throw new InvalidOperationException("Cannot activate a decommissioned vehicle.");

            Status = VehicleStatus.Active;
        }

        public void SetInactive()
        {
            if (Status == VehicleStatus.Decommissioned)
                throw new InvalidOperationException("Cannot inactivate a decommissioned vehicle.");

            Status = VehicleStatus.Inactive;
        }

        public void Decommission()
        {
            Status = VehicleStatus.Decommissioned;
        }

        // -----------------------------
        // Identity updates
        // -----------------------------

        public void UpdateIdentity(string registrationNumber, string name, string externalId)
        {
            Identity = new VehicleIdentity(
                NormalizeOrNull(registrationNumber),
                NormalizeOrNull(name),
                NormalizeOrNull(externalId));
        }

        public void Rename(string name)
        {
            Identity = new VehicleIdentity(
                Identity.RegistrationNumber,
                NormalizeOrNull(name),
                Identity.ExternalId);
        }

        public void SetRegistration(string registrationNumber)
        {
            Identity = new VehicleIdentity(
                NormalizeOrNull(registrationNumber),
                Identity.Name,
                Identity.ExternalId);
        }

        public void SetExternalId(string externalId)
        {
            Identity = new VehicleIdentity(
                Identity.RegistrationNumber,
                Identity.Name,
                NormalizeOrNull(externalId));
        }

        // -----------------------------
        // Latest location pointer
        // -----------------------------

        // Removed: LatestLocationProgressId/LatestLocationProgress are replaced by VehicleLatestLocation.

        private static string NormalizeOrNull(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Trim();
        }
    }

    public sealed class VehicleIdentity
    {
        public string RegistrationNumber { get; private set; }
        public string Name { get; private set; }
        public string ExternalId { get; private set; }

        private VehicleIdentity() { } // EF

        public VehicleIdentity(string registrationNumber, string name, string externalId)
        {
            RegistrationNumber = NormalizeOrNull(registrationNumber);
            Name = NormalizeOrNull(name);
            ExternalId = NormalizeOrNull(externalId);
        }

        private static string NormalizeOrNull(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Trim();
        }
    }

    public enum VehicleStatus
    {
        Active = 1,
        Inactive = 2,
        Decommissioned = 3
    }
}