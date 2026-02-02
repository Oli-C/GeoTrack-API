# GeoTrack API
> Multi-tenant REST API for ingesting GPS telemetry (“fixes”) for vehicles and querying vehicle state.

- API project: `GeoTrack.API/`
- Domain model: `GeoTrack.Domain/`
- BDD Tests: `GeoTrack.API.Specs/` (Reqnroll + xUnit)

## Key concepts

### Multi-tenancy
Endpoints are **tenant-scoped**.

- Send tenant ID using header: `X-Tenant-Id: <guid>`
- The API validates the tenant header:
  - It must be a valid GUID and not empty.
  - The tenant must exist in the `tenants` table. 
  - Will return **400** if the tenant header is missing.

### API key authentication
By default, the API requires an API key header on most endpoints.

- Header name (default): `X-Api-Key`
- Default dev key (from `GeoTrack.API/appsettings.json`): `dev-local-key`
- Anonymous paths (default): `/health`
- OpenAPI/Scalar docs are anonymous in **Development** only (`/openapi/*`, `/scalar/*`).

Configuration is under `ApiKey` in `appsettings.json`.

## Quick start (Docker Compose)
This repo includes Docker Compose for Postgres + the API.
- Postgres exposed on `localhost:5432`
- API exposed on `http://localhost:8080`
- EF Core migrations run automatically on API startup

1) Start containers

```sh
docker compose up --build
```

2) Check health

```sh
curl -sS http://localhost:8080/health | jq
```

3) Seed demo tenants + a York demo vehicle + a few GPS fixes

```sh
./seed-tenants-and-york-demo.sh
```

That script:
- inserts a few tenants directly into the DB
- creates a demo vehicle via `POST /vehicles`
- ingests a small set of fixes via `POST /vehicles/{vehicleId}/gps-fixes`

4. **Once finished, stop and clean up everything:**
```sh
docker compose down -v
```

## Run locally (dotnet)

### Prerequisites
- .NET SDK (the solution targets .NET 10; see `*.csproj`)
- A Postgres instance (local or container)

### Configuration
The API reads the Postgres connection string from:
- `ConnectionStrings:Postgres` in `GeoTrack.API/appsettings.json`
Default (local dev) connection string is:

```text
Host=localhost;Port=5432;Database=geotrack;Username=postgres;Password=postgres
```

## API documentation (Scalar)
In **Development**:
- OpenAPI JSON: `GET /openapi/v1.json`
- Scalar UI: `GET /scalar/v1`

These docs are mapped *before* the API key middleware runs, so they don’t require `X-Api-Key` in Development.

## Common request headers

| Header | Required | Description |
|---|---:|---|
| `X-Api-Key` | Yes (most endpoints) | API key. Default dev key is `dev-local-key`. |
| `X-Tenant-Id` | Yes (tenant-scoped endpoints) | Tenant GUID. Must exist in the database. |

## Endpoints

### Health
- `GET /health` (anonymous by default)

### Vehicles
- `POST /vehicles` – create a vehicle
- `GET /vehicles/{vehicleId}` – get vehicle by id
- `GET /vehicles?page=&pageSize=` – list vehicles (paged)
- `PATCH /vehicles/{vehicleId}` – patch identity/status
- `GET /vehicles/{vehicleId}/summary` – vehicle summary (identity/status + latest location + progress window)
- `GET /vehicles/{vehicleId}/latest-location` – latest known location (or 204 if none)

### GPS fixes (telemetry ingest)
- `POST /vehicles/{vehicleId}/gps-fixes` – ingest a single fix
- `POST /gps-fixes/batch` – ingest a batch of fixes

## Example requests

### Create a vehicle

```sh
TENANT_ID=11111111-1111-1111-1111-111111111111
API_KEY=dev-local-key

curl -sS -X POST http://localhost:8080/vehicles \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "X-Api-Key: ${API_KEY}" \
  -H "X-Tenant-Id: ${TENANT_ID}" \
  -d '{"registrationNumber":"VU21 EBB","name":"Demo vehicle","externalId":"demo-1"}' | jq
```

### Ingest a single GPS fix

```sh
VEHICLE_ID=<vehicle-guid>

curl -sS -X POST "http://localhost:8080/vehicles/${VEHICLE_ID}/gps-fixes" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "X-Api-Key: ${API_KEY}" \
  -H "X-Tenant-Id: ${TENANT_ID}" \
  -d '{
    "latitude": 53.959965,
    "longitude": -1.087298,
    "deviceTimeUtc": "2026-02-01T12:00:00Z",
    "deviceSequence": 1,
    "speedKph": 25,
    "headingDegrees": 90,
    "accuracyMeters": 5,
    "correlationId": "demo-1"
  }' | jq
```

### Get latest location

```sh
curl -sS "http://localhost:8080/vehicles/${VEHICLE_ID}/latest-location" \
  -H "Accept: application/json" \
  -H "X-Api-Key: ${API_KEY}" \
  -H "X-Tenant-Id: ${TENANT_ID}" | jq
```

## Database and migrations

- EF Core migrations live in `GeoTrack.API/Migrations/`.
- On startup (except in the `Testing` environment), the API runs `db.Database.MigrateAsync()` with retries.

## Testing

### Specs project
`GeoTrack.API.Specs` is a test project using:
- Reqnroll (Gherkin-style BDD)
- xUnit
- EF Core InMemory provider
- `Microsoft.AspNetCore.Mvc.Testing`

The specs start the API in the `Testing` environment and replace the DB context with InMemory (`GeoTrack.API.Specs/Infrastructure/GeoTrackApiFactory.cs`).

Run tests:

```sh
dotnet test
```

> This approach is intentionally used for logic and API behavior validation, not for validating PostgreSQL-specific or relational database behavior. EF Core’s InMemory provider does not enforce constraints, transactions, indexes, or SQL translation rules in the same way as a real relational database.

## Troubleshooting

### 503 ApiKey.NotConfigured
If API key enforcement is enabled but no keys are configured, requests fail closed with 503.
- Ensure `ApiKey:Keys` is configured in `GeoTrack.API/appsettings.json` or overridden via environment variables.

### 400 Tenant header errors
- Missing tenant header: tenant-scoped endpoints return `400`.
- Invalid tenant header: if `X-Tenant-Id` is not a GUID, is empty, or doesn’t exist in DB.

### Postgres connectivity
If the API fails on startup with a missing connection string or migration failures:
- Confirm `ConnectionStrings__Postgres` is set correctly.
- If running via Docker Compose, the override sets:
  `Host=postgres;Port=5432;Database=geotrack;Username=postgres;Password=postgres`

---

Data Retention, Analytics & Data Protection
-------------------------------------------

GPS telemetry is high-volume time-series data. Moving forward, the system should consider supporting **long-term analytics**, and **data-protection best practices** through a tiered retention strategy.

### Short-Term

Full-fidelity GPS fixes are stored in PostgreSQL for a limited window (e.g. **7–30 days**). This supports:

*   Real-time vehicle tracking.

*   Route and shift monitoring.

*   Incident investigation and support.

### Long-Term (Analytics & Reporting)

Rather than deleting historical data entirely, older telemetry could be downsampled into lower-resolution location traces (for example, one point per minute) and rolled up into summary metrics such as:

- Distance travelled.
- Time spent on route. 
- Average speed and collection speed.
- Route adherence.
- Idle times.
- Stop durations.
- Automated stop verifcation.
- Productivity metrics.


These roll-ups could be stored in dedicated DB tables or exported to cheaper long-term storage or even blob storage.

This allows customers to analyse trends across weeks and months, without us keeping high-volume location data in the operational database.

*Oli Clarke - 2026.02.02*