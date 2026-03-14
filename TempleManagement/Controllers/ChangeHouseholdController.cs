using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using TempleManagement.Models;
using TempleManagement.Models.DBManager;

namespace TempleManagement.Controllers
{
    public class ChangeHouseholdController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> GetHousehold(string household_id)
        {
            if (!ModelState.IsValid) //驗證 Post 來的資料

            {
                Debug.WriteLine($"ModelState: {JsonSerializer.Serialize(household_id)}");
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

            Debug.WriteLine($"GetHousehold_info_check:{JsonSerializer.Serialize(household_id)}");

            try
            {
                HouseholdManagement_DBManager dbManager = new HouseholdManagement_DBManager();
                Debug.WriteLine("HouseholdManagement_DBManager");
                List<HouseholdMember> infos = await dbManager.getHousehold(household_id);

                if (infos != null) {
                    return Json(new { success = true, household_id = household_id, info = infos });
                }

                return Json(new { success = false, message = "找不到該戶號成員" });
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
