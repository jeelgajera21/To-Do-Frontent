using To_Do_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using To_Do_UI.Models;
using System.Text;

namespace To_Do_UI.Controllers
{
    [Route("[controller]")]
    public class TaskController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

        #region Constructor
        public TaskController(IConfiguration configuration)
        {
            _configuration = configuration;
            _client = new HttpClient
            {
                BaseAddress = new System.Uri(_configuration["WebApiBaseUrl"])
            };
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

        #region Add Task
        [HttpGet("{TaskID}")]
        [Route("AddTask")]
        public IActionResult AddTask(int? TaskID)
        {
            TaskModel taskbyid = null;

            if (TaskID != null)
            {
                HttpResponseMessage response = _client.GetAsync($"Task/{TaskID}").Result;

                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;

                    // Deserialize the data as a list of TaskModel
                    var tasks = JsonConvert.DeserializeObject<List<TaskModel>>(data);

                    // Get the first task from the list if it exists
                    taskbyid = tasks?.FirstOrDefault();
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
            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(task);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                 if (task.TaskID == null)
                    response = await _client.PostAsync($"Task", content);
                else
                    response = await _client.PutAsync($"Task", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("TaskList");
            }
          
            return View("AddTask", task);
        }
        #endregion

        #region Delete Task
        public async Task<IActionResult> DeleteTask(int TaskID)
        {
            var response = await _client.DeleteAsync($"Task/?TaskID={TaskID}");
            return RedirectToAction("TaskList");
        }
        #endregion
    }
}
