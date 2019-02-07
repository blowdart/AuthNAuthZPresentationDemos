using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthZ.Controllers
{
    public class HomeController : Controller
    {
        [Authorize(Policy= "BuildingEntry")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
