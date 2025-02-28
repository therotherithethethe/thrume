using Microsoft.AspNetCore.Identity;

namespace Thrume.Domain;

public class Account(AccountId id, string userName, string passwordHash) : IdentityUser<AccountId>
{
    public override AccountId Id { get; set; } = id;
    public override string? UserName { get; set; } = userName;
    public override string? PasswordHash { get; set; } = BCrypt.Net.BCrypt.HashPassword(passwordHash);
    public override string? ConcurrencyStamp { get; set; } = Guid.CreateVersion7().ToString();

    public Account() : this(default, default, default) {} //Ef core
    public static Account CreateNew(string login, string pass) => 
        new(new AccountId(Guid.CreateVersion7()), login, pass);
}
