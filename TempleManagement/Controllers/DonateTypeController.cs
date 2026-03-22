using Microsoft.AspNetCore.Mvc;
using TempleManagement.Models;
using TempleManagement.Models.DBManager;

namespace TempleManagement.Controllers
{
    public class DonateTypeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            DonateType_DBManager dbmanager = new DonateType_DBManager();
            List<DonateType> infos = await dbmanager.get_donatetype();

            return View(infos);
        }
    }
}
