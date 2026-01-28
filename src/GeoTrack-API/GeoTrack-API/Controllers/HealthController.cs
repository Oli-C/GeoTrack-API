using Microsoft.AspNetCore.Mvc;

namespace GeoTrack_API.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var version = typeof(HealthController).Assembly.GetName().Version?.ToString() ?? "unknown";

        return Ok(new
        {
            status = "ok",
            version,
            timestampUtc = DateTimeOffset.UtcNow
        });
    }
}
