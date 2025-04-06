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
    //TODO: NOT IDEAL
    private readonly UserCredential _userCredentials;
    private readonly ILogger<IdentityEmailSender<TEntity>> _logger;

    public IdentityEmailSender(ILogger<IdentityEmailSender<TEntity>> logger)
    {
        _logger = logger;
        _userCredentials = Setup(CancellationToken.None).GetAwaiter().GetResult();
    }

    public async Task SendConfirmationLinkAsync(TEntity user, string email, string confirmationLink)
    {
        var smtpClient = new SmtpClient();
        try
        {
            await smtpClient.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(new SaslMechanismOAuth2(_userCredentials.UserId,
                _userCredentials.Token.AccessToken));
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("wildchild", "wildchild250336@gmail.com"));
            message.To.Add(new MailboxAddress("asdasd", email));
            message.Subject = "Test Email via OAuth";

            var messageText = $"<p>Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.</p>";
            message.Body = new TextPart("html") { Text = messageText };

            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);
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

    public Task SendPasswordResetCodeAsync(TEntity user, string email, string resetCode)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetLinkAsync(TEntity user, string email, string resetLink)
    {
        throw new NotImplementedException();
    }
    private async Task<UserCredential> Setup(CancellationToken ct)
    {
        const string user = "wildchild250336@gmail.com";
        const string api = "https://mail.google.com/";
        await using var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);
        var googleClientSecrets = await GoogleClientSecrets.FromStreamAsync(stream, ct);

        var credentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            googleClientSecrets.Secrets,
            [api],
            user,
            ct,
            new FileDataStore("token.json", true));
        if (!credentials.Token.IsStale) return credentials; //TODO
        await credentials.RefreshTokenAsync(ct);
        await Setup(ct);

        return credentials;
    }
}
