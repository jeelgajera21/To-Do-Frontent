﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using To_Do_UI.Models;

namespace To_Do_UI.Controllers
{
    [Route("[controller]")]
    public class CategoryController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

        #region Constructor
        public CategoryController(IConfiguration configuration)
        {
            _configuration = configuration;
            _client = new HttpClient
            {
                BaseAddress = new System.Uri(_configuration["WebApiBaseUrl"])
            };
        }
        #endregion
      /*  public IActionResult Index()
        {
            return View();
        }*/

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
            Console.WriteLine("userid : " + userid);

            // Check if the user is logged in (replace "Guest" if you want to treat guests differently)
            if (userid == null)
            {
                // Handle cases where the user is not logged in, e.g., redirect to login or show an error
                return RedirectToAction("Login", "User"); // Assuming you have a login action
            }
            List<CategoryModel> category = new List<CategoryModel>();
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
            }

            return View("AddCategory", category);
        }
        #endregion

        #region Delete Category

        public async Task<IActionResult> Delete(int CategoryID)
        {
            var response = await _client.DeleteAsync($"Category/?CategoryID={CategoryID}");
            return RedirectToAction("CategoryListByUser");
        }

        #endregion

       
    }
}
