using Microsoft.AspNetCore.Mvc;

namespace TempleManagement.Controllers
{
    public class AccountManagementController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }
    }
}
