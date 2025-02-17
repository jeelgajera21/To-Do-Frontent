using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MimeKit;
using To_Do_UI.Models;

namespace To_Do_UI.Controllers
{
    [Route("[controller]")]
    [CheckAccess]
    public class EmailController : Controller
    {
        #region Constructor DI

        
        private readonly MailSettings _mailSettings;

        public EmailController(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        #endregion

        #region Index/testing page

        
        public IActionResult Index(EmailModel model)
        {

            if (ModelState.IsValid)
            {
                try
                {

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
                    message.To.Add(new MailboxAddress(model.Email, model.Email));
                    message.Subject = $"Hello {model.Email}";
                    message.Body = new TextPart("plain")
                    {
                        Text = $"Name: {model.Email}\nSubject: {model.Subject}"
                    };

                    using (var client = new SmtpClient())
                    {
                        client.Connect(_mailSettings.Host, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                        client.Authenticate("work247gj@gmail.com", "rqyk twht pbfa wklm");
                        client.Send(message);
                        client.Disconnect(true);
                    }

                    ViewBag.Message = "Email sent successfully!";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Error sending email: " + ex.Message;
                }
            }

            return View(model);
        }
        #endregion
    }
}
