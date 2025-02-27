using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using To_Do_UI.Models;
using MailKit.Net.Smtp;
using System.Globalization;
using Newtonsoft.Json;

namespace To_Do_UI.Controllers
{
    [Route("[controller]")]
    [CheckAccess]
    public class HangFireEmailController : Controller
    {
        #region Constructor DI

        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly MailSettings _mailSettings;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

      
        public HangFireEmailController(IConfiguration configuration,IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager, IOptions<MailSettings> mailSettings)
        {
            if (backgroundJobClient == null)
            {
                throw new ArgumentNullException(nameof(backgroundJobClient), "IBackgroundJobClient was not provided.");
            }
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;

            if (mailSettings == null || mailSettings.Value == null)
            {
                throw new ArgumentNullException(nameof(mailSettings), "MailSettings is not configured properly.");
            }
            _mailSettings = mailSettings.Value;


            _configuration = configuration;
            _client = new HttpClient
            {

                BaseAddress = new System.Uri(_configuration["WebApiBaseUrl"])
            };

            // Log the values to confirm they are being injected
            //Console.WriteLine($"MailSettings - Mail: {_mailSettings.Mail}, Host: {_mailSettings.Host}, Port: {_mailSettings.Port}");
        }

        #endregion

        #region simple mail

       
        // 🔹 Fire-and-Forget Job (Runs Once Immediately)
        [HttpPost("send-email")]
        public IActionResult SendEmailReminder(string email)
        {
            _backgroundJobClient.Enqueue(() => SendEmail(email, "Reminder Email", "This is your reminder!"));

            
            ViewBag.RemMessage = "Email reminder sent!";
            ViewBag.ShowAlert = true; // Set flag
            return View("Index");
        }
        #endregion

        #region Schedule-email

       
        [HttpPost("schedule-email")]
        public IActionResult ScheduleEmailReminder(string dateTime,string Title)
        {
            var Name = HttpContext.Session.GetString("Name");
            var email = HttpContext.Session.GetString("Email");
            var userId = HttpContext.Session.GetInt32("UserID");

            if (Name == "noName" || email == "noEmail")
            {
                return RedirectToAction("Login", "User");
            }

            if (!DateTime.TryParseExact(dateTime, "yyyy-MM-ddTHH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime scheduledDateTime))
            {
                return BadRequest("Invalid date-time format. Use 'yyyy-MM-ddTHH:mm'.");
            }

            DateTime now = DateTime.Now;
            if (scheduledDateTime <= now)
            {
                TempData["ScheduleMessageE"] = "Scheduled time must be in the future.";
                // return BadRequest("Scheduled time must be in the future.");

                return RedirectToAction("ReminderListByUser", "Reminder");
                //return Json(new {message= "Scheduled time must be in the future." });
            }

            TimeSpan delay = scheduledDateTime - now;
            _backgroundJobClient.Schedule(() => SendEmail(email,
                $"Scheduled Email Reminder",
                $"Dear {Name},\r\n\r\nThis is a friendly reminder for your scheduled event.\r\n\r\n Date & Time: {scheduledDateTime}\r\n Scheduled At: {now}\r\n Details: {Title}\r\n\r\nIf you have any questions or need to reschedule, feel free to reach out.\r\n\r\nBest regards,\r\nTo-Do Mascot"), delay);

            TempData["ScheduleMessage"] = $"Email scheduled for {scheduledDateTime:dd-MM-yyyy HH:mm}";
            return RedirectToAction("ReminderListByUser", "Reminder"); // Redirect ensures TempData is passed correctly

            
        }
        #endregion

        #region daily-Email/Recurring-mail


        /*
            // 🔹 Recurring Job (Runs Repeatedly at a Specific Time)
            [HttpPost("set-daily-reminder")]
                public IActionResult SetDailyReminder(string email)
                {
                    _recurringJobManager.AddOrUpdate(
                        email,
                        () => SendEmail(email, "Daily Reminder", "This is your daily reminder!"),
                        Cron.Daily(9)  // Runs daily at 9 AM
                    );

                    return Ok("Daily email reminder set at 9 AM!");
                }*/

        #endregion

        #region Actual Mail Logic

        // 🔹 Email Sending Logic (Mock)
        [HttpPost]
        public IActionResult SendEmail(string email, string subject, string body)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body))
            {
                ViewBag.EmailMessage = "Email, Subject or Body is missing!";
                return View(); // Return to the same page without redirection
            }

            if (_mailSettings == null)
            {
                ViewBag.EmailMessage = "Mail settings are not properly configured.";
                return View(); // Return to the same page without redirection
            }

            try
            {
                var message = new MimeMessage();
                if (message == null)
                {
                    ViewBag.EmailMessage = "MimeMessage could not be created.";
                    return View(); // Return to the same page without redirection
                }

                message.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
                message.To.Add(new MailboxAddress(email, email));
                message.Subject = $"Hello {email}";
                message.Body = new TextPart("plain")
                {
                    Text = $"Email Sent to: {email}\nSubject: {subject}\nBody: {body}"
                };

                using (var client = new SmtpClient())
                {
                    if (client == null)
                    {
                        ViewBag.EmailMessage = "SMTP Client could not be created.";
                        return View(); // Return to the same page without redirection
                    }

                    client.Connect(_mailSettings.Host, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                    client.Send(message);
                    client.Disconnect(true);
                }

                ViewBag.EmailMessage = "Email sent successfully!";
                return View("Index"); // Return to the same page without redirection
            }
            catch (Exception ex)
            {
                ViewBag.EmailMessage = "Error sending email: " + ex.Message;
                return View("Index"); // Return to the same page without redirection
            }
        }

        public IActionResult Index()
        {
            // Retrieve ReminderModel from TempData if available (set in RbyRkey)
            if (TempData["ReminderModel"] != null)
            {
                var reminder = JsonConvert.DeserializeObject<ReminderModel>(TempData["ReminderModel"].ToString());
                return View(reminder);
            }

            // Default model when accessed directly
            var model = new ReminderModel
            {
                ReminderTime = DateTime.Now.AddMinutes(10) // Default time for new reminders
            };

            return View(model);
        }


        #endregion

        #region ReminderByView

        
        [Route("RbyRkey")]
        public async Task<IActionResult> RbyRkey(int ReminderID)
        {
            var userid = HttpContext.Session.GetInt32("UserID");
            Console.WriteLine("userid : " + userid);

            ReminderModel reminder = new ReminderModel();
            HttpResponseMessage response = await _client.GetAsync($"Reminder/{ReminderID}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                reminder = JsonConvert.DeserializeObject<ReminderModel>(data);

                // Store the retrieved reminder in TempData for redirect
                TempData["ReminderModel"] = JsonConvert.SerializeObject(reminder);
            }

            return RedirectToAction("Index"); // Redirect to Index() where it handles ReminderModel
           // return PartialView("_viewname"); // Return PartialView to render Index.cshtml
        }
        #endregion

    }
}
