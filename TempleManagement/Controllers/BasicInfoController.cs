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

                //之後要，寫入Donate_individual Donate_household
                DonateOperation_DBManager donateOperation_dbManager = new DonateOperation_DBManager();
                Debug.WriteLine("BDManager_newDonate_individual");
                await donateOperation_dbManager.create_donation_individual(new DonateIndividual
                {
                    //MID = info.MID, 有更正寫在此函式庫裡，抓的MID
                    DonateItem_idv = new List<DonationItem>(),
                    Note = null
                });

                // 只有戶長才需要
                if (info.Is_head)
                {
                    Debug.WriteLine("BDManager_newDonate_household");

                    HouseholdManagement_DBManager householdManagement_dbManager = new HouseholdManagement_DBManager();
                    List<HouseholdMember> house_info = await householdManagement_dbManager.getHousehold_by_basicinfo(info); //實際上info沒作用，特地抓MID

                    await donateOperation_dbManager.create_donation_household(new DonateHousehold
                    {
                        HouseID = house_info[0].House_ID,
                    });
                }
                

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
            List<HouseholdMember> member_no_db = await dBManager.getHouseholdMember("get_houseid_member_no"); // 找出最大member_no
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
                member_no = member_no_db[0].Member_no + 1;
                househead_head = basicInfo[0].Name;
            }
            Debug.WriteLine("Update House_id 1111:"+ house_id);
            Debug.WriteLine("Update member_no 1111:" + member_no);

            return Json(new { success = true, message = "成功更新戶長資料", house_id = house_id, member_no = member_no, househead_head = househead_head });
        }

        
    }
}
