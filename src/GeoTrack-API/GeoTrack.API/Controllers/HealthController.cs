using Microsoft.AspNetCore.Mvc;

namespace GeoTrack.API.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> Get()
    {
        var version = typeof(HealthController).Assembly.GetName().Version?.ToString() ?? "unknown";

        return Ok(new HealthResponse
        {
            Status = "ok",
            Version = version,
            TimestampUtc = DateTimeOffset.UtcNow
        });
    }

    public sealed record HealthResponse
    {
        public required string Status { get; init; }
        public required string Version { get; init; }
        public required DateTimeOffset TimestampUtc { get; init; }
    }
}
