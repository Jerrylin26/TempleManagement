using Microsoft.AspNetCore.Mvc;

namespace TempleManagement.Controllers
{
    public class PrayerListController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
