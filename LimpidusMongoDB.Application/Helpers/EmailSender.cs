using System.Net;
using System.Net.Mail;
using LimpidusMongoDB.Application.Settings;
using MimeMapping;

namespace LimpidusMongoDB.Application.Helpers
{
    public static class EmailSender
    {
        public static async Task SendAsync(
            EmailSettings settings,
            IEnumerable<string> emails,
            IEnumerable<string> cc,
            string subject,
            string message,
            IEnumerable<(string fileName, Stream file)> attachments = null,
            CancellationToken cancellationToken = default)
        {
            if (emails == null || !emails.Any())
                return;

            var body = $"<html><head></head><body>{message}</body></html>";

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(settings.From, settings.FromDisplay),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };

            foreach (var email in emails)
                mailMessage.To.Add(new MailAddress(email));

            if (cc != null && cc.Any())
            {
                foreach (var email in cc)
                    mailMessage.CC.Add(new MailAddress(email));
            }

            if (attachments != null && attachments.Any())
            {
                foreach (var (fileName, file) in attachments)
                {
                    file.Position = 0;
                    mailMessage.Attachments.Add(new Attachment(file, fileName, MimeUtility.GetMimeMapping(fileName)));
                }
            }

            using var smtp = new SmtpClient(settings.SMTP, settings.Port);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Credentials = new NetworkCredential(settings.From, settings.Password);

            await smtp.SendMailAsync(mailMessage, cancellationToken);
        }
    }
}
