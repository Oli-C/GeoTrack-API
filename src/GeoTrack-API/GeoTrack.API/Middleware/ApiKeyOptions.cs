namespace GeoTrack.API.Middleware;

public sealed class ApiKeyOptions
{
    public const string SectionName = "ApiKey";

    /// <summary>
    /// Enables API key enforcement when true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// The request header that carries the API key.
    /// </summary>
    public string HeaderName { get; init; } = "X-Api-Key";

    /// <summary>
    /// One or more valid API keys.
    /// </summary>
    public string[] Keys { get; init; } = [];

    /// <summary>
    /// Paths that will skip API key validation (exact match, case-insensitive).
    /// Useful for health checks.
    /// </summary>
    public string[] AllowAnonymousPaths { get; init; } = ["/health"];
}
