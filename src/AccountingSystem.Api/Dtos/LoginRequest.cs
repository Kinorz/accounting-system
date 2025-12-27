namespace AccountingSystem.Api.Dtos;

public sealed record LoginRequest(
    string Email,
    string Password);
