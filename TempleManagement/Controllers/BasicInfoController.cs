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

        // 新增 BasicInfo
        public async Task<IActionResult> NewBasicInfo(BasicInfo info)
        {
            if (!ModelState.IsValid) //驗證 Post 來的資料
            
             {   Debug.WriteLine($"ModelState: {JsonSerializer.Serialize(info)}");
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

        // 即時更新戶長資訊 (在BasicInfo 建立資料時)
        [HttpPost]
        public async Task<IActionResult> UpdateHousehold(bool Is_head)
        {
            if (!ModelState.IsValid) //驗證 Post 來的資料
            {
                Debug.WriteLine($"ModelState: {JsonSerializer.Serialize(Is_head)}");
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

            BasicInfo_DBManager dBManager = new BasicInfo_DBManager();
            List<HouseholdMember> member_info = await dBManager.getHouseholdMember("get_head");
            int house_id = 0;
            int member_no = 0;
            string househead_head = "";

            // 取得戶長姓名
            int member_id = member_info[0].MemberID;
            List<BasicInfo> basicInfo = await dBManager.getBasicInfo(member_id:member_id);

            Debug.WriteLine("Update House_id:" + JsonSerializer.Serialize(member_info[0]));

            if (Is_head) //戶長
            {
                house_id = member_info[0].House_ID + 1; //戶長+1
                member_no = 1;
                househead_head = "你本人!!!";
                
            }
            else
            {
                house_id = member_info[0].House_ID;
                member_no = member_info[0].Member_no + 1;
                househead_head = basicInfo[0].Name;
            }
            Debug.WriteLine("Update House_id 1111:"+ house_id);
            Debug.WriteLine("Update member_no 1111:" + member_no);

            return Json(new { success = true, message = "成功更新戶長資料", house_id = house_id, member_no = member_no, househead_head = househead_head });
        }

        
    }
}
