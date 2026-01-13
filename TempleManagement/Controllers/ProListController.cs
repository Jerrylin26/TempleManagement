using Microsoft.AspNetCore.Mvc;

namespace TempleManagement.Controllers
{
    public class ProListController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
