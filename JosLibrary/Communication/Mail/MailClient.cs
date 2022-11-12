using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace JosLibrary.Communication.Mail
{
    public class MailClient
    {
        private SmtpClient client = new SmtpClient();
        private NetworkCredential credential = new NetworkCredential();

        public void Init(string username, string password, string domain, int port)
        {
            Console.WriteLine("Initializing with... username: {0}, password: {1}, domain: {2}, port: {3}", username, new string('*', password.Length), domain, port);
            client = new SmtpClient(domain, port);
            credential = new NetworkCredential(username, password);
            client.Credentials = credential;
            client.EnableSsl = true;
 
            client.Timeout = (60 * 1 * 1000);
        }

        public void Init(string username, SecureString password, string domain, int port)
        {
            Console.WriteLine("Initializing with... username: {0}, password: {1}, domain: {2}, port: {3}", username, new string('*', password.Length), domain, port);
            client = new SmtpClient(domain, port);
            credential = new NetworkCredential(username, password);
            client.Credentials = credential;
            client.EnableSsl = true;

            client.Timeout = (60 * 1 * 1000);
        }

        public void SendMail(string recipient, string subject, string body, string[] attachments)
        {
            MailMessage message = CreateMailMessage(recipient, subject, body, attachments);
            client.Send(message);
        }

        public void SendMail(string recipient, string cc, string subject, string body, string[] attachments)
        {
            MailMessage message = CreateMailMessage(recipient, cc, subject, body, attachments);
            client.Send(message);
        }

        public void SendAsyncMail(string recipient, string subject, string body, string[] attachments)
        {
            MailMessage message = CreateMailMessage(recipient, subject, body, attachments);
            client.SendMailAsync(message);
        }

        public void SendAsyncMail(string recipient, string cc, string subject, string body, string[] attachments)
        {
            MailMessage message = CreateMailMessage(recipient, cc, subject, body, attachments);
            client.SendMailAsync(message);
        }

        private MailMessage CreateMailMessage(string recipient, string subject, string body, string[] attachments)
        {
            MailMessage message = new MailMessage();
            MailAddress fromAddress = new MailAddress(credential.UserName);

            message.From = fromAddress;
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = body;
            message.To.Add(recipient);

            if (attachments != null)
            {
                foreach (string attachment in attachments)
                {
                    message.Attachments.Add(new Attachment(attachment));
                }
            }

            return message;
        }

        private MailMessage CreateMailMessage(string recipient, string cc, string subject, string body, string[] attachments)
        {
            MailMessage message = new MailMessage();
            MailAddress fromAddress = new MailAddress(credential.UserName);

            message.From = fromAddress;
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = body;
            message.To.Add(recipient);
            message.CC.Add(cc);

            if (attachments != null)
            {
                foreach (string attachment in attachments)
                {
                    message.Attachments.Add(new Attachment(attachment));
                }
            }

            return message;
        }
    }
}
