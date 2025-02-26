using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using To_Do_UI.Models;

namespace To_Do_UI.Controllers
{
    public class HomeController : Controller
    {
        #region Logger

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        #endregion

        #region index

        public IActionResult Index()
        {
            return View();
        }
        #endregion

        #region profile

        public IActionResult Profile()
        {
            return View();
        }
        #endregion

        #region privacy

        public IActionResult Privacy()
        {
            return View();
        }
        #endregion
        #region _404PageNotfound

        public IActionResult PageNotFound()
        {
            return View("_404PageNotfound","Shared" );
        }
        #endregion

        #region errorviewmodel


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion
    }
}
