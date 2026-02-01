namespace GeoTrack.API.Contracts;

public sealed record ApiErrorResponse
{
    public required string Error { get; init; }
    public required string Message { get; init; }
}
