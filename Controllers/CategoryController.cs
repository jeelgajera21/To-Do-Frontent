using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Index()
        {
            return View();
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
                    var categories = JsonConvert.DeserializeObject<List<CategoryModel>>(data);

                    // Get the first task from the list if it exists
                    categorybyid = categories?.FirstOrDefault();
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
                    return RedirectToAction("CategoryList");
            }

            return View("AddCategory", category);
        }
        #endregion

        #region Delete Category

        public async Task<IActionResult> Delete(int CategoryID)
        {
            var response = await _client.DeleteAsync($"Category/?CategoryID={CategoryID}");
            return RedirectToAction("CategoryList");
        }

        #endregion


    }
}
