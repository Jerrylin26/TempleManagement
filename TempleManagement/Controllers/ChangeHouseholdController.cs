using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using TempleManagement.Models;
using TempleManagement.Models.DBManager;

namespace TempleManagement.Controllers
{
    public class ChangeHouseholdController : Controller
    {

        public async Task<List<ChangeHousehold>> Changehousehold(List<HouseholdMember> infos)
        {
            BasicInfo_DBManager dbMgr = new BasicInfo_DBManager();

            List<ChangeHousehold> b_infos = new List<ChangeHousehold>();

            foreach (HouseholdMember info in infos)
            {
                List<BasicInfo> b_info = await dbMgr.getBasicInfo(member_id: info.MemberID);
                b_infos.Add(new ChangeHousehold
                {
                    Name = b_info[0].Name,
                    Age = b_info[0].Age,

                    HouseholdID = info.HouseholdID,
                    House_ID = info.House_ID,
                    Is_head = info.Is_head,
                    End_date = info.End_date,
                    MemberID = info.MemberID,
                    Member_no = info.Member_no,
                    Start_date = info.Start_date

                });
            }

            return b_infos;

        }

        public async Task<IActionResult> Index()
        {

            HouseholdManagement_DBManager dbManager = new HouseholdManagement_DBManager();
            Debug.WriteLine("HouseholdManagement_DBManager");
            List<HouseholdMember> infos = await dbManager.getHousehold("3");

            List<ChangeHousehold> b_infos = await Changehousehold(infos); // 合成一個func 讓大家使用

            return View(b_infos);
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

                Debug.WriteLine($"GetHousehold_infos_checker:{JsonSerializer.Serialize(infos)}");


                List<ChangeHousehold> b_infos = await Changehousehold(infos); // BasicInfo + HouseholdMember
                Debug.WriteLine($"b_infos check:{JsonSerializer.Serialize(b_infos)}");
                if (infos?.Any() == true) { 
                    return Json(new { success = true, household_id = household_id, info = b_infos });
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


        // 下次更改戶長實際作業
    }

    
}
