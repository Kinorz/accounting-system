namespace AccountingSystem.Api.Dtos;

public sealed record TokenResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc);
