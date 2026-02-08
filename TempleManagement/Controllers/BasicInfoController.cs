using Microsoft.AspNetCore.Mvc;
using TempleManagement.Models;
using TempleManagement.Models.DBManager;

namespace TempleManagement.Controllers
{
    public class BasicInfoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult NewBasicInfo(BasicInfo info)
        {
            if (!ModelState.IsValid) //驗證 Post 來的資料
            {
                return Json(new { success = false, message = "資料驗證失敗" });
            }

            try
            {
                BasicInfo_DBManager dbManager = new BasicInfo_DBManager();
                dbManager.newBasicInfo(info);

                return Json(new { success = true, name = info.Name });
            }
            catch (Exception ex)
            {
                // 可記錄錯誤 log
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
