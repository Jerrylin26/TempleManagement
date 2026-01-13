using Microsoft.AspNetCore.Mvc;

namespace TempleManagement.Controllers
{
    public class MailLabelController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
