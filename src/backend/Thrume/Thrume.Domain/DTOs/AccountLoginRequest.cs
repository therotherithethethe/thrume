namespace Thrume.Domain.DTOs;
public readonly record struct AccountLoginRequest(
    string Email,
    string Password
    );
