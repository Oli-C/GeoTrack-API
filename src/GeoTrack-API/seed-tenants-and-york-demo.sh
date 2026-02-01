#!/usr/bin/env bash
set -euo pipefail

# Seeds a few UK council tenants (incl. York) directly into the DB, then creates a vehicle
# and ingests a handful of GPS fixes via the API.
#
# Requirements:
# - psql available
# - API running (default http://localhost:8080)
# - DB reachable (defaults match docker-compose typicals; override with env vars)
#
# Auth:
# - This API uses an API key header by default.
# - The default local key in GeoTrack.API/appsettings.json is: dev-local-key
# - Override with API_KEY=... if you changed config.
#
# Usage:
#   ./seed-tenants-and-york-demo.sh
#   API_BASE_URL=http://localhost:5000 DB_HOST=localhost DB_PASSWORD=postgres ./seed-tenants-and-york-demo.sh

API_BASE_URL="${API_BASE_URL:-http://localhost:8080}"
API_KEY="${API_KEY:-dev-local-key}"

DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-geotrack}"
DB_USER="${DB_USER:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-postgres}"

# Deterministic tenant IDs so reruns are safe and you can keep using the same header values.
# (Any stable GUIDs are fine as long as they’re unique.)
YORK_TENANT_ID="11111111-1111-1111-1111-111111111111"
LEEDS_TENANT_ID="11111111-1111-1111-1111-111111111112"
MANCHESTER_TENANT_ID="11111111-1111-1111-1111-111111111113"
BIRMINGHAM_TENANT_ID="11111111-1111-1111-1111-111111111114"

# Demo vehicle
VEHICLE_REG="VU21 EBB"

# --- helpers ---
header_args=()
header_args+=("-H" "Content-Type: application/json")
header_args+=("-H" "Accept: application/json")
if [[ -n "${API_KEY}" ]]; then
  header_args+=("-H" "X-Api-Key: ${API_KEY}")
fi

psql_uri="postgresql://${DB_USER}:${DB_PASSWORD}@${DB_HOST}:${DB_PORT}/${DB_NAME}"

echo "Seeding tenants into DB (${DB_HOST}:${DB_PORT}/${DB_NAME})..."
PGPASSWORD="${DB_PASSWORD}" psql "${psql_uri}" -v ON_ERROR_STOP=1 <<SQL
INSERT INTO tenants (id, name)
VALUES
  ('${YORK_TENANT_ID}', 'City of York Council'),
  ('${LEEDS_TENANT_ID}', 'Leeds City Council'),
  ('${MANCHESTER_TENANT_ID}', 'Manchester City Council'),
  ('${BIRMINGHAM_TENANT_ID}', 'Birmingham City Council')
ON CONFLICT (name) DO NOTHING;
SQL

echo "Creating demo vehicle '${VEHICLE_REG}' under York tenant via API (${API_BASE_URL})..."
# Create vehicle; if it already exists (duplicate registration), we’ll look it up via list endpoint.
create_payload=$(cat <<JSON
{"registrationNumber":"${VEHICLE_REG}","name":"York demo vehicle","externalId":"york-demo-1"}
JSON
)

set +e
create_resp=$(curl -sS -i "${header_args[@]}" -H "X-Tenant-Id: ${YORK_TENANT_ID}" \
  -X POST "${API_BASE_URL}/vehicles" \
  -d "${create_payload}")
create_status=$(printf "%s" "${create_resp}" | head -n 1 | awk '{print $2}')
set -e

vehicle_id=""
if [[ "${create_status}" == "201" ]]; then
  vehicle_id=$(printf "%s" "${create_resp}" | tail -n 1 | python3 -c 'import json,sys; print(json.load(sys.stdin)["id"])')
elif [[ "${create_status}" == "409" ]]; then
  echo "Vehicle already exists (duplicate registration). Searching via GET /vehicles..."
  list_json=$(curl -sS "${header_args[@]}" -H "X-Tenant-Id: ${YORK_TENANT_ID}" "${API_BASE_URL}/vehicles?page=1&pageSize=100")
  vehicle_id=$(printf "%s" "${list_json}" | python3 -c 'import json,sys

d=json.load(sys.stdin)
reg=(sys.argv[1] or "").strip().upper()
for v in d.get("items", []):
  if (v.get("registrationNumber") or "").strip().upper() == reg:
    print(v["id"])
    break
' "${VEHICLE_REG}")
else
  echo "Unexpected response creating vehicle (HTTP ${create_status}). Full response:" >&2
  echo "${create_resp}" >&2
  exit 1
fi

if [[ -z "${vehicle_id}" ]]; then
  echo "Could not determine vehicle id. Aborting." >&2
  exit 1
fi

echo "Using vehicleId=${vehicle_id}"

echo "Ingesting a few GPS fixes around York..."
# Coordinates roughly central York.
# DeviceTimeUtc must be UTC (Z) and correlationId is required.
base_time="2026-02-01T12:00:00Z"

# 5 sample points (a tiny path) — tweak as needed.
latlons=(
  "53.959965 -1.087298"
  "53.960500 -1.086200"
  "53.961100 -1.085000"
  "53.961700 -1.083800"
  "53.962300 -1.082600"
)

idx=0
for ll in "${latlons[@]}"; do
  lat=$(echo "${ll}" | awk '{print $1}')
  lon=$(echo "${ll}" | awk '{print $2}')
  # increment seconds
  device_time=$(python3 -c 'import datetime as d; base=d.datetime.fromisoformat("'"${base_time/Z/+00:00}"'"); print((base+d.timedelta(seconds='"${idx}"')).isoformat().replace("+00:00","Z"))')

  payload=$(cat <<JSON
{"latitude":${lat},"longitude":${lon},"deviceTimeUtc":"${device_time}","deviceSequence":${idx},"speedKph":25,"headingDegrees":90,"accuracyMeters":5,"correlationId":"york-seed-${idx}"}
JSON
)

  curl -sS "${header_args[@]}" -H "X-Tenant-Id: ${YORK_TENANT_ID}" \
    -X POST "${API_BASE_URL}/vehicles/${vehicle_id}/gps-fixes" \
    -d "${payload}" >/dev/null

  idx=$((idx+1))
done

echo "Done."
echo "Tenant IDs:"
echo "  York:        ${YORK_TENANT_ID}"
echo "  Leeds:       ${LEEDS_TENANT_ID}"
echo "  Manchester:  ${MANCHESTER_TENANT_ID}"
echo "  Birmingham:  ${BIRMINGHAM_TENANT_ID}"
echo "Vehicle:"
echo "  Registration: ${VEHICLE_REG}"
echo "  VehicleId:    ${vehicle_id}"
