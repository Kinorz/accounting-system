namespace AccountingSystem.Api.Dtos;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string CompanyName);
