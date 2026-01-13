using Microsoft.AspNetCore.Mvc;

namespace TempleManagement.Controllers
{
    public class FollowerListController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
