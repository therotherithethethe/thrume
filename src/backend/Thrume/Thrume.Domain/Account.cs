namespace Thrume.Domain;

public readonly record struct AccountId(Guid Id)
{
    public static implicit operator AccountId(Guid id) => new(id);
}

public class Account
{
    public AccountId Id { get; init; }
    public string Login { get; init; }
    public int PasswordHash { get; init; }

    public Account(AccountId id, string login, string pass) => 
      (Id, Login, PasswordHash) = (id, login, pass.GetHashCode()); //TODO

    public static Account CreateNew(string login, string pass) => new(Guid.CreateV7(), login, pass);
}
