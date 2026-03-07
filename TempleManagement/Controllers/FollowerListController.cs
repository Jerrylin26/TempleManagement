using Microsoft.AspNetCore.Mvc;
using TempleManagement.Models;
using TempleManagement.Models.DBManager;

namespace TempleManagement.Controllers
{
    public class FollowerListController : Controller
    {
        public IActionResult GetBasicInfo()
        {
            BasicInfo_DBManager db = new BasicInfo_DBManager();
            Task<List<BasicInfo>> Info = db.getBasicInfo(); //已改成 postgresql
            return View(Info);

        }
    }
}
