using Core.Interfaces;
using Core.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class MailingService : IMailingService
    {

        private readonly MailSettings _mailSettings;

        public MailingService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public string GetMailBody([Optional] string userName, string Email, string tokenLink,string TemplateNameView)
        {
            var filePath = $"{Directory.GetCurrentDirectory()}\\Templates\\{TemplateNameView}.cshtml";
            var str = new StreamReader(filePath);

            var mailText = str.ReadToEnd();
            str.Close();
            return mailText.Replace("{username}", userName).Replace("{Email}", Email).Replace("{link}", tokenLink);
        }

        public async Task SendEmailAsync( string mailto, string subject, string body)
        {
            var email = new MimeMessage
            {
                Sender = MailboxAddress.Parse(_mailSettings.Email),
                Subject = subject,

            };

            email.To.Add(MailboxAddress.Parse(mailto));

            var builder = new BodyBuilder();



            builder.HtmlBody = body;
            email.Body = builder.ToMessageBody();
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Email));

            using var smtp = new SmtpClient();
            smtp.CheckCertificateRevocation = false;
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.Auto);

            smtp.Authenticate(_mailSettings.Email, _mailSettings.Password);     //Must be Authenticated First

            await smtp.SendAsync(email);

            smtp.Disconnect(true);
        }


    }
}
