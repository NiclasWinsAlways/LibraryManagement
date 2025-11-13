using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace backendLibraryManagement.Services
{
    public class EmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _fromAddress;
        private readonly string _password;

        public EmailService(string host, int port, string fromAddress, string password)
        {
            _host = host;
            _port = port;
            _fromAddress = fromAddress;
            _password = password;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MailMessage(_fromAddress, to, subject, body)
            {
                IsBodyHtml = true
            };

            using (var smtp = new SmtpClient(_host, _port))
            {
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(_fromAddress, _password);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Timeout = 20000; // 20 seconds

                await smtp.SendMailAsync(message);
            }
        }
    }
}
