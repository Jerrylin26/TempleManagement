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


        public async Task<IActionResult> ChangeHousehold_head(int mid, string name, int houseid)
        {
            Debug.WriteLine("ChangeHousehold_head");
            Debug.WriteLine($"{mid},{name},{houseid}");
            HouseholdManagement_DBManager dbManager = new HouseholdManagement_DBManager();
            Debug.WriteLine("HouseholdManagement_DBManager_ChangeHousehold_head");
            try
            {
                await dbManager.changeHead(mid, houseid);
                return Json(new { success =  true, message = $"更換戶長為{name}" });
            }
            catch
            {
                return Json(new { success = false, message = "更換戶長失敗" });
            }

            
        }



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
            b_infos.Reverse();
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
                b_infos.Reverse();
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


        // 上一戶
        public async Task<IActionResult> pre_house(int house_id)
        {

            HouseholdManagement_DBManager dbManager = new HouseholdManagement_DBManager();
            Debug.WriteLine("HouseholdManagement_pre_house_DBManager");
            var house_string = "0";
            if (house_id > 2) 
            {
                house_string = (house_id-1).ToString();
            }
            
            List<HouseholdMember> infos = await dbManager.getHousehold(house_string);

            List<ChangeHousehold> b_infos = await Changehousehold(infos); // 合成一個func 讓大家使用
            b_infos.Reverse();
            if (b_infos?.Any() == true)
            {
                return Json(new { info = b_infos, message = "成功", success = true });
            }
            return Json(new { message = "失敗", success = false }); 
        }

        // 下一戶
        public async Task<IActionResult> next_house(int house_id)
        {
            List<HouseholdMember> infos;
            HouseholdManagement_DBManager dbManager = new HouseholdManagement_DBManager();
            Debug.WriteLine("HouseholdManagement_pre_house_DBManager");
            var house_string = "";
            house_string = (house_id + 1).ToString();

  
            infos = await dbManager.getHousehold(house_string);
            Debug.WriteLine($"下一戶:{JsonSerializer.Serialize(infos)}");

            List<ChangeHousehold> b_infos = await Changehousehold(infos); // 合成一個func 讓大家使用
            b_infos.Reverse();
            if (b_infos?.Any() == true)
            {
                return Json(new { info = b_infos, message = "成功", success = true });
            }
            return Json(new { message = "失敗", success = false }); 
        }

        // 姓名查詢
        public async Task<IActionResult> search_name(string name)
        {
            List<BasicInfo> infos;
            List<List<ChangeHousehold>> fianl_infos = new List<List<ChangeHousehold>>();
            BasicInfo_DBManager dbManager = new BasicInfo_DBManager();
            List<int> repeated_house_id = new List<int>();
            HouseholdManagement_DBManager dgr = new HouseholdManagement_DBManager();
            Debug.WriteLine("-----------------------search_name----------------------");


            infos = await dbManager.search_name(name);
            Debug.WriteLine($"search_name:{JsonSerializer.Serialize(infos)}");

            // 篩選出，以houseID為group 的組合               
            foreach (BasicInfo info in infos) {
                List<HouseholdMember> householdMembers = await dgr.getHousehold_by_basicinfo(info);
                if (!repeated_house_id.Contains(householdMembers[0].House_ID))
                {
                    Debug.WriteLine("In repeated flow");
                    repeated_house_id.Add(householdMembers[0].House_ID);
                    List < ChangeHousehold > bb_infos = new List<ChangeHousehold>();
                    List<ChangeHousehold> b_infos = await Changehousehold(householdMembers);

                    fianl_infos.Add(b_infos);
                }   
            }

            //b_infos.Reverse();
            if (fianl_infos?.Any() == true)
            {
                return Json(new { info = fianl_infos, message = "成功", success = true });
            }
            return Json(new { message = "查無此人", success = false });
        }
    }

    
}
