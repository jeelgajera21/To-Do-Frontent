﻿using Microsoft.AspNetCore.Mvc;
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
                            Console.WriteLine("Setting session values...");

                            HttpContext.Session.SetInt32("UserID", userResponse.UserID);
                            HttpContext.Session.SetString("UserName", userResponse.UserName);
                            HttpContext.Session.SetString("Email", userResponse.Email);
                            HttpContext.Session.SetString("Name", userResponse.Name);

                            Console.WriteLine("Session UserID: " + HttpContext.Session.GetInt32("UserID"));
                            Console.WriteLine("Session UserName: " + HttpContext.Session.GetString("UserName"));
                            Console.WriteLine("Session Email: " + HttpContext.Session.GetString("Email"));
                            Console.WriteLine("Session Name: " + HttpContext.Session.GetString("Name"));

                            if (!userResponse.IsActive)
                            {
                                ModelState.AddModelError("", "Your account is inactive. Please contact support.");
                                return View("Login", userLogin);
                            }

                            return RedirectToAction("Index", "Home");
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

        [HttpPost]
        public async Task<IActionResult> UserRegister(UserModel user)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", user);
            }
            try
            {
                var json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync("User/register", content);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred while processing your request.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                ModelState.AddModelError("", "An error occurred while processing your request.");
            }
            return View("Register", user);
        }
        public IActionResult Register()
        {
            return View();
        }
        #endregion

        #region Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }
        #endregion
    }
}
