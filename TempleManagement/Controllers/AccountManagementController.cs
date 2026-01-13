using Microsoft.AspNetCore.Mvc;

namespace TempleManagement.Controllers
{
    public class AccountManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
