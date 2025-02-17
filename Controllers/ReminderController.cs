using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using To_Do_UI.Models;

namespace To_Do_UI.Controllers
{
    [Route("[controller]")]
    [CheckAccess]
    public class ReminderController : Controller
    {
        #region Constructor DI

        

        private readonly ApiAuthBearer _apiAuthBearer;
        private readonly HttpClient _client;

        public ReminderController(ApiAuthBearer apiAuthBearer)
        {
            _apiAuthBearer = apiAuthBearer;
            _client = _apiAuthBearer.GetHttpClient();
        }

        #endregion

        #region Index

        
        public IActionResult Index()
        {
            return View();
        }
        #endregion

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

        #endregion

        #region ReminderByUser

        [Route("ReminderListByUser")]
        [HttpGet]
        public IActionResult ReminderListByUser()
        {
            var userid = HttpContext.Session.GetInt32("UserID");
            var token = HttpContext.Session.GetString("Token");




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

        #region temp

       
        /* public async Task<IActionResult> ReminderListByUserid(int ReminderID)
         {
             var userid = HttpContext.Session.GetInt32("UserID");
             Console.WriteLine("userid : " + userid);


             ReminderModel reminder = new ReminderModel();
             HttpResponseMessage response =  _client.GetAsync($"Reminder/{ReminderID}").Result;
             if (response.IsSuccessStatusCode)
             {
                 string data = response.Content.ReadAsStringAsync().Result;
                 *//*  dynamic jsonObject = JsonConvert.DeserializeObject(data);*//*

                 reminder = JsonConvert.DeserializeObject<ReminderModel>(data);
             }


             return RedirectToAction("Index","HangFireEmail",reminder);
         }*/

        #endregion

        #region Add / Edit Reminder
        [HttpGet("{ReminderID}")]
        [Route("AddReminder")]
        public IActionResult AddReminder(int? ReminderID)
        {
            ReminderModel reminderbyid = null;
            ViewBag.TaskList = TaskDropDown().Result;

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

        #region Add Reminder by Task
        [HttpGet("{RTaskID}")]
        [Route("AddReminderTaskID")]
        public IActionResult AddReminderTaskID(int RTaskID)
        {
            ReminderModel reminderbyid = null;

            if (RTaskID != null)
            {
                HttpResponseMessage response = _client.GetAsync($"Task/{RTaskID}").Result;

                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;

                    // Deserialize the data as a list of TaskModel
                    var reminders = JsonConvert.DeserializeObject<ReminderModel>(data);

                    // Get the first task from the list if it exists
                    reminderbyid = reminders;
                }

                return View("AddReminderByTask", reminderbyid);
            }
            return RedirectToAction("TaskListByUser", "Task" );
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
                    return RedirectToAction("ReminderListByUser");
            }

            return View("AddReminder", reminder);
        }
        #endregion

        #region Delete Reminder

        [Route("DeleteReminder/{ReminderID}")]
        public async Task<IActionResult> Delete(int ReminderID)
        {
            var response = await _client.DeleteAsync($"Reminder/?ReminderID={ReminderID}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Msg"] = "Deleted Successfully"; // Use TempData instead
            }
            else
            {
                TempData["Msg"] = "Delete Failed";
            }
            return RedirectToAction("ReminderListByUser");
        }

        #endregion

        #region partialView
        [HttpGet("GetPartialView")]
        public async Task<IActionResult> GetPartialView(int ReminderID)
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
               // TempData["ReminderModel"] = JsonConvert.SerializeObject(reminder);
            }
            return PartialView("_viewname" ,reminder); // Loads the partial view
        }
        #endregion

        #region CategoryDropDown

        [HttpGet]
        [Route("TaskDDListByUser")]
        public async Task<List<TaskDropDownByUser>> TaskDropDown()
        {
            var userid = HttpContext.Session.GetInt32("UserID");
            Console.WriteLine("userid : " + userid);


            List<TaskDropDownByUser> task = new List<TaskDropDownByUser>();
            HttpResponseMessage response = _client.GetAsync($"Task/dd-by-user/{userid}").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                task = JsonConvert.DeserializeObject<List<TaskDropDownByUser>>(data);
                Console.WriteLine(task);
            }
            return task;
        }

        #endregion


    }
}
