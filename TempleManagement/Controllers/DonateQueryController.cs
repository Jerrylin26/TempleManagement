using Microsoft.AspNetCore.Mvc;

namespace TempleManagement.Controllers
{
    public class DonateQueryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
