using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using To_Do_UI.Models;

namespace To_Do_UI.Controllers
{
    public class ReminderController : Controller
    {
        

        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

        #region Constructor
        public ReminderController(IConfiguration configuration)
        {
            _configuration = configuration;
            _client = new HttpClient
            {
               
                BaseAddress = new System.Uri(_configuration["WebApiBaseUrl"])
            };
        }
        #endregion
        public IActionResult Index()
        {
            return View();
        }

        #region ReminderList

        [HttpGet]
        /*[Route("categorylist")]*/
        public IActionResult ReminderList()
        {
            List<ReminderModel> reminder = new List<ReminderModel>();
            HttpResponseMessage response = _client.GetAsync("Reminder").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                /*  dynamic jsonObject = JsonConvert.DeserializeObject(data);*/

                reminder = JsonConvert.DeserializeObject<List<ReminderModel>>(data);
            }


            return View("ReminderList", reminder);


        }

        [Route("ReminderListByUser")]
        [HttpGet]
        public IActionResult ReminderListByUser()
        {
            var userid = HttpContext.Session.GetInt32("UserID");
            Console.WriteLine("userid : " + userid);

            // Check if the user is logged in (replace "Guest" if you want to treat guests differently)
            if (userid == null)
            {
                // Handle cases where the user is not logged in, e.g., redirect to login or show an error
                return RedirectToAction("Login", "User"); // Assuming you have a login action
            }
            List<ReminderModel> reminder = new List<ReminderModel>();
            HttpResponseMessage response = _client.GetAsync($"Reminder/by-user/{userid}").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                /*  dynamic jsonObject = JsonConvert.DeserializeObject(data);*/

                reminder = JsonConvert.DeserializeObject<List<ReminderModel>>(data);
            }


            return View("ReminderList", reminder);
        }

        #endregion

        #region Add / Edit Reminder
        [HttpGet("{ReminderID}")]
        [Route("AddReminder")]
        public IActionResult AddReminder(int? ReminderID)
        {
            ReminderModel reminderbyid = null;

            if (ReminderID != null)
            {
                HttpResponseMessage response = _client.GetAsync($"Reminder/{ReminderID}").Result;

                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;

                    // Deserialize the data as a list of TaskModel
                    var reminders = JsonConvert.DeserializeObject<ReminderModel>(data);

                    // Get the first task from the list if it exists
                    reminderbyid = reminders;
                }

                return View("AddReminder", reminderbyid);
            }
            return View("AddReminder", new ReminderModel());
        }
        #endregion

        #region Save Reminder
        [HttpPost]
        public async Task<IActionResult> Save(ReminderModel reminder)
        {
            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(reminder);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (reminder.ReminderID== null)
                    response = await _client.PostAsync($"Reminder", content);
                else
                    response = await _client.PutAsync($"Reminder", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("ReminderList");
            }

            return View("AddReminder", reminder);
        }
        #endregion

        #region Delete Reminder

        public async Task<IActionResult> Delete(int ReminderID)
        {
            var response = await _client.DeleteAsync($"Reminder/?ReminderID={ReminderID}");
            return RedirectToAction("ReminderList");
        }

        #endregion

        
    }
}
