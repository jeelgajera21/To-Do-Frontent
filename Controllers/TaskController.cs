using To_Do_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using ToDo_API.Models;
using System.Text;

namespace To_Do_UI.Controllers
{
    public class TaskController : Controller
    {
         Uri baseAddress = new Uri("http://localhost:5028");

        private readonly HttpClient _client;

        public TaskController()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
        }

        [HttpGet]
        public IActionResult TaskList()
        {
            List<TaskModel> task = new List<TaskModel>();
            HttpResponseMessage response = _client.GetAsync($"{_client.BaseAddress}Task").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                /*  dynamic jsonObject = JsonConvert.DeserializeObject(data);*/

                task = JsonConvert.DeserializeObject<List<TaskModel>>(data);
            }

            return View("TaskList", task);
        }


        [HttpGet("{TaskID}")]
        [Route("addtask")]
        public IActionResult AddTask(int? TaskID)
        {
            TaskModel taskbyid = null;

            if (TaskID != null)
            {
                HttpResponseMessage response = _client.GetAsync($"{_client.BaseAddress}Task/{TaskID}").Result;

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


        [HttpPost]
        public async Task<IActionResult> Save(TaskModel task)
        {
            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(task);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                 if (task.TaskID == null)
                    response = await _client.PostAsync($"{_client.BaseAddress}Task", content);
                else
                    response = await _client.PutAsync($"{_client.BaseAddress}Task", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("TaskList");
            }
          
            return View("AddTask", task);
        }


    }
}
