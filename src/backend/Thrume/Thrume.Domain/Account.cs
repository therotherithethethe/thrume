using Microsoft.AspNetCore.Identity;

namespace Thrume.Domain;

public sealed class Account : IdentityUser<AccountId>
{
    public override AccountId Id { get; set; } = Guid.CreateVersion7();
    public override string? UserName { get; set; }
    public override string? Email { get ; set; }
    public override string? PasswordHash { get; set; }
    public override string? ConcurrencyStamp { get; set; } = Guid.CreateVersion7().ToString();

    public Account(AccountId id, string userName, string email, string passwordHash)
    {
        Id = id;
        UserName = userName;
        Email = email;
        PasswordHash = passwordHash;
    }

    public Account() {} //Ef core
    public override string ToString()
    {
        return $"{UserName}:{Email}:{PasswordHash}";
    }
}
