using Microsoft.AspNetCore.Mvc;
using TempleManagement.Models;
using TempleManagement.Models.DBManager;

namespace TempleManagement.Controllers
{
    public class ModifyAgeController : Controller
    {
                

        // 下次從 ModifyAge
        // changehousehold 
        // login 小功能做起

        public async Task<IActionResult> AddAge()
        {
            BasicInfo_DBManager db = new BasicInfo_DBManager();
            await db.AddAge();


            return Json(new { success = true, message = "成功" });
        }

        public async Task<IActionResult> MinusAge()
        {
            BasicInfo_DBManager db = new BasicInfo_DBManager();
            await db.MinusAge();


            return Json(new { success = true, message = "成功" });
        }

        public async Task<IActionResult> Index()
        {
            BasicInfo_DBManager db = new BasicInfo_DBManager();
            List<BasicInfo> Info = await db.getBasicInfo(); //已改成 postgresql

            var rand = new Random();
            var random5 = Info
                .Take(5)
                .ToList();


            return View(random5);

        }
    }
}
