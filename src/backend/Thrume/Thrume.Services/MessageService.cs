// Thrume.Services/MessageService.cs
using Microsoft.EntityFrameworkCore;
using Thrume.Database;
using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;


namespace Thrume.Services;

public class MessageService
{
    private const int MaxMessageLength = 2000;
    private readonly AppDbContext _dbContext;

    public MessageService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Finds an existing 1-on-1 conversation between two users or creates a new one.
    /// </summary>
    public async Task<bool> StartOrGetConversationAsync(AccountId user1Id, string userName)
    {
        var existingConversation = await _dbContext.ConversationDbSet
            .Include(c => c.Participants)
            .Where(c => c.Participants.Count == 2 &&
                        c.Participants.Any(p => p.Id == user1Id) &&
                        c.Participants.Any(p => p.UserName == userName))
            .FirstOrDefaultAsync();

        if (existingConversation != null)
        {
            return false;
        }

        var user1 = await
            _dbContext
                .AccountDbSet
                .Include(a => a.Followers)
                .FirstOrDefaultAsync(a => a.Id == user1Id);
        var user2 = await
            _dbContext
                .AccountDbSet
                .Include(a => a.Followers)
                .FirstOrDefaultAsync(a => a.UserName == userName);

        if (user1 == null || user2 == null)
        {
            return false;
        }

        if (!(user1.Followers.Any(s => s.FollowingId == user2.Id) &&
              user2.Followers.Any(s => s.FollowingId == user1Id)))
            return false;
    

        var newConversation = new Conversation
        {
            CreatedAt = DateTimeOffset.UtcNow,
            Participants = new List<Account> { user1, user2 }
        };

        _dbContext.ConversationDbSet.Add(newConversation);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Sends a message within a specific conversation.
    /// </summary>
    public async Task<Message?> SendMessageAsync(AccountId senderId, ConversationId conversationId, string content)
    {

        if (string.IsNullOrWhiteSpace(content) || content.Length > MaxMessageLength)
        {

            return null;
        }


        var conversation = await _dbContext.ConversationDbSet
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null || conversation.Participants.All(p => p.Id != senderId))
        {

            return null;
        }

        var message = new Message
        {
            Id = new MessageId(Guid.CreateVersion7()),
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content,
            SentAt = DateTimeOffset.UtcNow
        };

        _dbContext.MessageDbSet.Add(message);
        await _dbContext.SaveChangesAsync();

        return message;
    }

    /// <summary>
    /// Retrieves messages for a given conversation, ordered by time (newest first), with pagination.
    /// </summary>
    public async Task<List<Message>> GetMessagesAsync(ConversationId conversationId, AccountId requesterId, int page = 1, int pageSize = 20)
    {

        var isParticipant = await _dbContext.ConversationDbSet
            .Where(c => c.Id == conversationId)
            .AnyAsync(c => c.Participants.Any(p => p.Id == requesterId));

        if (!isParticipant)
        {

            return new List<Message>(); 
        }

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 100) pageSize = 100; 

        var messages = await _dbContext.MessageDbSet
            .Where(m => m.ConversationId == conversationId)
            .Include(m => m.Sender) 
            .OrderBy(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return messages;
    }

    /// <summary>
    /// Retrieves a list of conversations for a specific user.
    /// </summary>
    public async Task<List<Conversation>> GetConversationsAsync(AccountId userId)
    {
        var conversations = await _dbContext.ConversationDbSet
            .Where(c => c.Participants.Any(p => p.Id == userId))
            .Include(c => c.Participants)
            .OrderByDescending(c => c.CreatedAt) 
            .AsNoTracking() 
            .ToListAsync();

        return conversations;
    }
}