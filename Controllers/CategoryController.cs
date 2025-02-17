using Azure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using To_Do_UI.Models;

namespace To_Do_UI.Controllers
{
    [Route("[controller]")]
    public class CategoryController : Controller
    {
        /* private readonly IConfiguration _configuration;
         private readonly HttpClient _client;


         #region Constructor
         public CategoryController(IConfiguration configuration)
         {
             var token = HttpContext.Session.GetString("Token");

             _configuration = configuration;
             _client = new HttpClient
             {
                 BaseAddress = new System.Uri(_configuration["WebApiBaseUrl"])
             };
             _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

         }
         #endregion*/
        private readonly ApiAuthBearer _apiAuthBearer;
        private readonly HttpClient _client;

        public CategoryController(ApiAuthBearer apiAuthBearer)
        {
            _apiAuthBearer = apiAuthBearer;
            _client = _apiAuthBearer.GetHttpClient();
        }



        #region CategoryList

        [HttpGet]
        /*[Route("categorylist")]*/
        public IActionResult CategoryList()
        {
            
            List<CategoryModel> category = new List<CategoryModel>();
            HttpResponseMessage response = _client.GetAsync("Category").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                /*  dynamic jsonObject = JsonConvert.DeserializeObject(data);*/

                category = JsonConvert.DeserializeObject<List<CategoryModel>>(data);
            }


            return View("CategoryList", category);


        }

        [HttpGet]
        [Route("CategoryListByUser")]
        public IActionResult CategoryListByUser()
        {
            var userid = HttpContext.Session.GetInt32("UserID");
            var token = HttpContext.Session.GetString("Token");

          


            Console.WriteLine("userid : " + userid);

            // Check if the user is logged in (replace "Guest" if you want to treat guests differently)
            if (userid == null && token==null)
            {
                // Handle cases where the user is not logged in, e.g., redirect to login or show an error
                return RedirectToAction("Login", "User"); // Assuming you have a login action
            }
            List<CategoryModel> category = new List<CategoryModel>();

            // Set up HttpClient with Authorization Header
            //_client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            

            HttpResponseMessage response = _client.GetAsync($"Category/by-user/{userid}").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                /*  dynamic jsonObject = JsonConvert.DeserializeObject(data);*/

                category = JsonConvert.DeserializeObject<List<CategoryModel>>(data);
            }


            return View("CategoryList", category);
        }

        #endregion

        #region Add Category
        [HttpGet("{CategoryID}")]
        [Route("AddCategory")]
        public IActionResult AddCategory(int? CategoryID)
        {
            CategoryModel categorybyid = null;

            if (CategoryID != null)
            {
                HttpResponseMessage response = _client.GetAsync($"Category/{CategoryID}").Result;

                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;

                    // Deserialize the data as a list of TaskModel
                    var categories = JsonConvert.DeserializeObject<CategoryModel>(data);

                    // Get the first task from the list if it exists
                    categorybyid = categories;
                }

                return View("AddCategory", categorybyid);
            }
            return View("AddCategory", new CategoryModel());
        }
        #endregion

        #region Save Category
        [HttpPost]
        public async Task<IActionResult> Save(CategoryModel category)
        {
            // Retrieve UserID from Session
            var userid = HttpContext.Session.GetInt32("UserID");
            if (userid.HasValue)
            {
                category.UserID = userid.Value;
            }
            else
            {
                // Handle the case where UserID is null, e.g., redirect to login or show an error
                return RedirectToAction("Login", "User");
            }

            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(category);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (category.CategoryID== null)
                    response = await _client.PostAsync($"Category", content);
                else
                    response = await _client.PutAsync($"Category", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("CategoryListByUser");

                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    // Parse API error response
                    var data = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(data);
                    Console.WriteLine(result);
                    if (result.errors != null)
                    {
                        foreach (var errorKey in result.errors)
                        {
                            string fieldName = errorKey.Name; // Get field name like "CategoryName"
                            foreach (var errorMessage in errorKey.Value)
                            {
                                ModelState.AddModelError(fieldName, errorMessage.ToString());
                            }
                        }
                    }

                    return View("AddCategory", category); // Return the same view with validation errors
                }
            }
            

            return View("AddCategory", category);
        }
        #endregion

        #region Delete Category
        [Route("DeleteCat/{CategoryID}")]
        public async Task<IActionResult> Delete(int CategoryID)
        {
            var response = await _client.DeleteAsync($"Category/?CategoryID={CategoryID}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Msg"] = "Deleted Successfully"; // Use TempData instead
            }
            else
            {
                TempData["Msg"] = "Delete Failed";
            }
            return RedirectToAction("CategoryListByUser");
        }

        #endregion

       
    }
}
