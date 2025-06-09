using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Thrume.Infrastructure;

public sealed class IdentityEmailSender<TEntity> : IEmailSender<TEntity> where TEntity : class
{
    private readonly ILogger<IdentityEmailSender<TEntity>> _logger;
    private UserCredential? _userCredential;

    public IdentityEmailSender(ILogger<IdentityEmailSender<TEntity>> logger)
    {
        _logger = logger;
    }

    public async Task SendConfirmationLinkAsync(TEntity user, string email, string confirmationLink)
    {
        var credentials = await GetOrCreateCredentialsAsync(CancellationToken.None);
        if (credentials is null)
        {
            _logger.LogError("Failed to obtain Google credentials. Cannot send email.");
            return;
        }

        var smtpClient = new SmtpClient();
        try
        {
            await smtpClient.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(new SaslMechanismOAuth2(credentials.UserId, credentials.Token.AccessToken));
            
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("wildchild", "wildchild250336@gmail.com"));
            message.To.Add(new MailboxAddress("User", email));
            message.Subject = "Test Email via OAuth";

            var messageText = $"<p>Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.</p>";
            message.Body = new TextPart("html") { Text = messageText };

            await smtpClient.SendAsync(message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Cant send confirmation link to {to}", email);
            throw;
        }
        finally
        {
            await smtpClient.DisconnectAsync(true);
            smtpClient.Dispose();
        }
    }

    public Task SendPasswordResetCodeAsync(TEntity user, string email, string resetCode) => throw new NotImplementedException();
    public Task SendPasswordResetLinkAsync(TEntity user, string email, string resetLink) => throw new NotImplementedException();

    private async Task<UserCredential?> GetOrCreateCredentialsAsync(CancellationToken ct)
    {
        if (_userCredential is not null)
        {
            if (_userCredential.Token.IsStale)
            {
                _logger.LogInformation("Access token is stale, refreshing...");
                if (await _userCredential.RefreshTokenAsync(ct))
                {
                    _logger.LogInformation("Access token was successfully refreshed.");
                }
                else
                {
                    _logger.LogWarning("Failed to refresh access token.");
                    _userCredential = null;
                }
            }
            return _userCredential;
        }

        _logger.LogInformation("Credentials not found in memory, initializing...");
        const string user = "wildchild250336@gmail.com";
        const string api = "https://mail.google.com/";
        await using var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);
        var googleClientSecrets = await GoogleClientSecrets.FromStreamAsync(stream, ct);

        _userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            googleClientSecrets.Secrets,
            [api],
            user,
            ct,
            new FileDataStore("token.json", true));
        
        _logger.LogInformation("Credentials successfully initialized.");
        return _userCredential;
    }
}