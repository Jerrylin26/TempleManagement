using Microsoft.AspNetCore.Mvc;

namespace TempleManagement.Controllers
{
    public class DonateOperation : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
