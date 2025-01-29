using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using To_Do_UI.Models;

namespace To_Do_UI.Controllers
{
    public class UserController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

        #region Constructor
        public UserController(IConfiguration configuration)
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

        #region Login

        [HttpPost]
        public async Task<IActionResult> UserLogin(UserLoginRequest userLogin)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", userLogin);
            }

            try
            {
                var json = JsonConvert.SerializeObject(userLogin);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("User/login", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Response JSON: " + responseString); // Debugging output

                    // Ensure response is not empty
                    if (!string.IsNullOrWhiteSpace(responseString))
                    {
                        var userResponse = JsonConvert.DeserializeObject<UserLoginResponse>(responseString);
                        Console.WriteLine(userResponse);
                        if (userResponse != null)
                        {
                           /* HttpContext.Session.SetInt32("UserID", userResponse.UserID);
                            HttpContext.Session.SetString("UserName", userResponse.UserName);
                            HttpContext.Session.SetString("Email", userResponse.Email);
*/
                            if (!userResponse.IsActive)
                            {
                                ModelState.AddModelError("", "Your account is inactive. Please contact support.");
                                return View("Login", userLogin);
                            }

                            return RedirectToAction("Index","Home");
                        }
                    }

                    ModelState.AddModelError("", "Invalid response from server.");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login credentials.");
                }
            }
            catch (JsonException jsonEx) // Catch deserialization errors
            {
                Console.WriteLine("JSON Deserialization Error: " + jsonEx.Message);
                ModelState.AddModelError("", "Invalid response format from server.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                ModelState.AddModelError("", "An error occurred while processing your request.");
            }

            return View("Login", userLogin);
        }

        public IActionResult Login()
        {
            return View();
        }


        #endregion
        #region Register

        public IActionResult Register()
        {
            return View();
        }
        #endregion
    }
}
