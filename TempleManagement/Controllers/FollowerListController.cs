using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using TempleManagement.Models;
using TempleManagement.Models.DBManager;

namespace TempleManagement.Controllers
{
    public class FollowerListController : Controller
    {
        public async Task<IActionResult> GetBasicInfo()
        {
            BasicInfo_DBManager db = new BasicInfo_DBManager();
            List<BasicInfo> Info = await db.getBasicInfo(); //已改成 postgresql
            return View(Info);

        }

        // 按下新增捐獻 顯示列表 (為初始導入)
        // 後，把篩選的option加入此函式，擴建多樣性
        public async Task<IActionResult> ShowOption()
        {
            BasicInfo_DBManager db = new BasicInfo_DBManager();
            List<BasicInfo> Info = await db.getBasicInfo(); //已改成 postgresql

            Debug.WriteLine("showoption()");
            Debug.WriteLine($"{JsonSerializer.Serialize(Info)}");
            return PartialView("_Show_details", Info);
        }

        public async Task<IActionResult> Detail(int mid)
        {
            BasicInfo_DBManager db = new BasicInfo_DBManager();
            List<BasicInfo> Info = await db.getBasicInfo(member_id:mid); //抓單筆

            Debug.WriteLine("show details");
            Debug.WriteLine($"{JsonSerializer.Serialize(Info)}");

            return PartialView("_Show_details", Info[0]);
        }
    }
}
