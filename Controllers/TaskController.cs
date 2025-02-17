using To_Do_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using To_Do_UI.Models;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;

namespace To_Do_UI.Controllers
{
    [CheckAccess]
    [Route("[controller]")]
    public class TaskController : Controller
    {
        #region constructor DI

       
        private readonly ApiAuthBearer _apiAuthBearer;
        private readonly HttpClient _client;

        public TaskController(ApiAuthBearer apiAuthBearer)
        {
            _apiAuthBearer = apiAuthBearer;
            _client = _apiAuthBearer.GetHttpClient();
        }
        #endregion

        #region List of Tasks
        [HttpGet]
        public IActionResult TaskList()
        {
            List<TaskModel> task = new List<TaskModel>();
            HttpResponseMessage response = _client.GetAsync("Task").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                /*  dynamic jsonObject = JsonConvert.DeserializeObject(data);*/

                task = JsonConvert.DeserializeObject<List<TaskModel>>(data);
            }


            return View("TaskList", task);
        }
        #endregion

        #region Task list by user

        
        [HttpGet]
        [Route("TaskListByUser")]
        public async Task<IActionResult> TaskListByUser()

        {
            var userid = HttpContext.Session.GetInt32("UserID");
            var token = HttpContext.Session.GetString("Token");

            // Check if the user is logged in (replace "Guest" if you want to treat guests differently)
            if (userid == null)
            {
                // Handle cases where the user is not logged in, e.g., redirect to login or show an error
                return RedirectToAction("Login", "User"); // Assuming you have a login action
            }
            List<TaskModel> task = new List<TaskModel>();
            
            HttpResponseMessage response = _client.GetAsync($"Task/by-user/{userid}").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                /*  dynamic jsonObject = JsonConvert.DeserializeObject(data);*/

                task = JsonConvert.DeserializeObject<List<TaskModel>>(data);
            }
           


            return View("TaskList", task);
        }


        #endregion

        #region Add / Edit Task
        [HttpGet("{TaskID}")]
        [Route("AddEditTask")]
        public IActionResult AddTask(int? TaskID)
        {
            TaskModel taskbyid = null;
            ViewBag.CategoryList = CategoryDropDown().Result;
            if (TaskID != null)
            {
                HttpResponseMessage response = _client.GetAsync($"Task/{TaskID}").Result;

                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;

                    // Deserialize the data as a list of TaskModel
                    var tasks = JsonConvert.DeserializeObject<TaskModel>(data);

                    // Get the first task from the list if it exists
                    taskbyid = tasks;
                    /*taskbyid = tasks?.FirstOrDefault();*/
                }

                return View("AddTask", taskbyid);
            }
            return View("AddTask", new TaskModel());
        }
        #endregion

        #region Save Task
        [HttpPost]
        public async Task<IActionResult> Save(TaskModel task)
        {
            // Retrieve UserID from Session
            var userid = HttpContext.Session.GetInt32("UserID");
            if (userid.HasValue)
            {
                task.UserID = userid.Value;
            }
            else
            {
                // Handle the case where UserID is null, e.g., redirect to login or show an error
                return RedirectToAction("Login", "User");
            }
           /* if (ModelState.IsValid)
            {*/
                var json = JsonConvert.SerializeObject(task);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                 if (task.TaskID == null)
                    response = await _client.PostAsync($"Task", content);
                else
                    response = await _client.PutAsync($"Task", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("TaskListByUser");

            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var data = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(data);

                foreach (var errorKey in result.errors)
                {
                    string fieldName = errorKey.Name; // Get field name like "DueDate"
                    foreach (var errorMessage in errorKey.Value)
                    {
                        ModelState.AddModelError(fieldName, errorMessage.ToString());
                    }
                }
                ViewBag.CategoryList = await CategoryDropDown(); // ✅ Ensure dropdown is not null
                return View("AddTask", task); // Return the same view with ModelState errors
            }

           //}


            return View("AddTask", task);
        }
        #endregion

        #region Delete Task

        [Route("DeleteTask/{TaskID}")]
        public async Task<IActionResult> DeleteTask(int TaskID)
        {
            var response = await _client.DeleteAsync($"Task/?TaskID={TaskID}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Msg"] = "Deleted Successfully"; // Use TempData instead
            }
            else
            {
                TempData["Msg"] = "Delete Failed";
            }
            return RedirectToAction("TaskListByUser");
        }
        #endregion

        #region CategoryDropDown

        [HttpGet]
        [Route("CategoryDDListByUser")]
        public async Task<List<CategoryDropDownByUser>> CategoryDropDown()
        {
            var userid = HttpContext.Session.GetInt32("UserID");
            /*Console.WriteLine("userid : " + userid);
*/

            List<CategoryDropDownByUser> category = new List<CategoryDropDownByUser>();
            HttpResponseMessage response = _client.GetAsync($"Category/dd-by-user/{userid}").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                category = JsonConvert.DeserializeObject<List<CategoryDropDownByUser>>(data);
               /* Console.WriteLine(category);*/
            }
            return category;
        }

        #endregion

        #region Index
        public IActionResult Index()
        {
            return View();
        }
        #endregion
    }
}
