using Microsoft.AspNetCore.Identity;
using Thrume.Domain;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Thrume.Database;
namespace Thrume.Api;

public class EmailSender<TEntity> : IEmailSender<TEntity> where TEntity : class
{
    //TODO: NOT IDEAL
    private readonly UserCredential _userCredentials;
    private readonly SmtpClient _smtpClient;
    public EmailSender(UserCredential userCredentials)
    {
        _userCredentials = userCredentials;
        _smtpClient = new SmtpClient();
    }
    public async Task SendConfirmationLinkAsync(TEntity user, string email, string confirmationLink)
    {
        await _smtpClient.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        await _smtpClient.AuthenticateAsync(new SaslMechanismOAuth2(_userCredentials.UserId, _userCredentials.Token.AccessToken));
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("wildchild", "wildchild250336@gmail.com"));
        message.To.Add(new MailboxAddress("asdasd", email));
        message.Subject = "Test Email via OAuth";

        var messageText = $"<p>Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.</p>";
        message.Body = new TextPart("html") { Text = messageText };

        await _smtpClient.SendAsync(message);
        await _smtpClient.DisconnectAsync(true);

    }

    public Task SendPasswordResetCodeAsync(TEntity user, string email, string resetCode)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetLinkAsync(TEntity user, string email, string resetLink)
    {
        throw new NotImplementedException();
    }
}