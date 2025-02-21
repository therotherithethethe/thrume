namespace Thrume.Domain;
public readonly record struct AccountId(Guid Id);

/*public class Account
{
    public AccountId Id { get; init; }
    public string Username { get; init; }           
    public Email Email { get; init; }              
    public string PasswordHash { get; init; }       
    public AccountProfile Profile { get; init; }    
    public Role Role { get; init; }
    public DateTime DateCreated { get; init; }     
    public List<Post> Posts { get; init; } 
    public List<Message> SentMessages { get; init; }
    public List<Message> ReceivedMessages { get; init; }
    public List<CallSession> CallSessions { get; init; }
}*/

public record Email(string Value);