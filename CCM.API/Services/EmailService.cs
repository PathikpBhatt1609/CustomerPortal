using CCM.API.Data; using CCM.API.Models; using MailKit.Net.Smtp; using MailKit.Security; using MimeKit;
namespace CCM.API.Services;
public class EmailService {
    private readonly IConfiguration _cfg; private readonly AppDbContext _db; private readonly ILogger<EmailService> _log;
    public EmailService(IConfiguration c, AppDbContext d, ILogger<EmailService> l){_cfg=c;_db=d;_log=l;}
    public async Task SendAsync(string toEmail, string toName, string subject, string body, int? reqId=null) {
        var s="Sent";
        try {
            var c=_cfg.GetSection("Email");
            var msg=new MimeMessage();
            msg.From.Add(new MailboxAddress(c["FromName"],c["FromEmail"]));
            msg.To.Add(new MailboxAddress(toName,toEmail));
            msg.Subject=subject; msg.Body=new TextPart("plain"){Text=body};
            using var smtp=new SmtpClient();
            await smtp.ConnectAsync(c["SmtpHost"],int.Parse(c["SmtpPort"]!),SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(c["Username"],c["Password"]);
            await smtp.SendAsync(msg); await smtp.DisconnectAsync(true);
        } catch(Exception ex){s="Failed: "+ex.Message;_log.LogError(ex,"Email failed to {E}",toEmail);}
        _db.EmailLogs.Add(new EmailLog{RequestId=reqId,ToEmail=toEmail,ToName=toName,Subject=subject,Body=body,Status=s});
        await _db.SaveChangesAsync();
    }
}