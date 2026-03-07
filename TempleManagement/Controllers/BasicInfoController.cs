using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using System.Diagnostics;
using TempleManagement.Models;
using TempleManagement.Models.DBManager;
using System.Text.Json;

namespace TempleManagement.Controllers
{
    public class BasicInfoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> NewBasicInfo(BasicInfo info)
        {
            if (!ModelState.IsValid) //驗證 Post 來的資料
            {
                Debug.WriteLine($"ModelState: {JsonSerializer.Serialize(info)}");
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        Debug.WriteLine($"欄位: {key}, 錯誤: {error.ErrorMessage}");
                    }
                }

                return Json(new { success = false, message = "資料驗證失敗" });
            }

            Debug.WriteLine($"NewBasicInfo_info_check:{JsonSerializer.Serialize(info)}");

            try
            {
                BasicInfo_DBManager dbManager = new BasicInfo_DBManager();
                Debug.WriteLine("BDManager_newBasicInfo");
                await dbManager.newBasicInfo(info);

                Debug.WriteLine("BDManager_newHouseholdMember");
                await dbManager.newHouseholdMember(info);
                

                return Json(new { success = true, name = info.Name });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("catch error");
                // 可記錄錯誤 log
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
