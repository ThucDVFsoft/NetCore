using Microsoft.AspNetCore.Mvc;

namespace Movies.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Get", "Movies");
        }
    }
}
