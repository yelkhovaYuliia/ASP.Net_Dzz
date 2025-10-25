using System.Net;
using System.Net.Mail;
using System.Text;
using WebATB.Interfaces;

namespace WebATB.Services
{
    public class EmailService(IConfiguration configuration) : IEmailService
    {
        public Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            return Task<bool>.Run(() =>
            {
                try
                {
                    string login = configuration["MailLogin"]!;
                    string password = configuration["MailPassword"]!;
                    string host = configuration["MailHost"]!;
                    short port = short.Parse(configuration["MailPort"]!);
                    bool enableSSL = bool.Parse(configuration["MailEnableSSL"]!);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Login: {login} \nPassword: {password} \nHost: {host} \nPort: {port} \nSSL: {enableSSL}");
                    Console.ResetColor();

                    // Message
                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(login, "WebATB");
                    mailMessage.To.Add(to);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;
                    mailMessage.BodyEncoding = Encoding.UTF8;
                    mailMessage.SubjectEncoding = Encoding.UTF8;

                    // SMTP client
                    using (SmtpClient smtpClient = new SmtpClient(host, port))
                    {
                        smtpClient.EnableSsl = enableSSL;
                        smtpClient.Credentials = new NetworkCredential(login, password);
                        smtpClient.Send(mailMessage);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error send message: {ex.Message} {ex.Source} {ex.Data} {ex.StackTrace} {ex.TargetSite} {ex.HelpLink}");
                    Console.ResetColor();
                    return false;
                }
            });


        }
    }
}