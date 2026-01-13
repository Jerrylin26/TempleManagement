using Microsoft.AspNetCore.Mvc;

namespace TempleManagement.Controllers
{
    public class BasicInfoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
