namespace Thrume.Domain.DTOs;
public readonly record struct AccountRegisterRequest(
    string Username,
    string Email,
    string Password
);