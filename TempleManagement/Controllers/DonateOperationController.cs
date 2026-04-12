using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Diagnostics;
using System.Text.Json;
using TempleManagement.Models;
using TempleManagement.Models.DBManager;

namespace TempleManagement.Controllers
{

    /*
     合併DonateOperation 跟 DonateQuery
     因為發現說，終究是會合併一起的。
     且功能直覺上，也比較清晰。
     
     */
    public class DonateOperationController : Controller
    {

        // 提交新增 選定人選 準備進入新增表單
        public IActionResult post_selected_option(List<Donate_Insert_query> donateOperations)
        {
            if (!ModelState.IsValid) //驗證 Post 來的資料

            {
                Debug.WriteLine($"ModelState: {JsonSerializer.Serialize(donateOperations)}");
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

            Debug.WriteLine("start post_selected_option()");
            
            Debug.WriteLine(JsonSerializer.Serialize(donateOperations));
            return Json(new { message = "yes" });

        }


        // 篩選button 不同方式顯示人選 (新增捐獻)
        public async Task<IActionResult> dropdown_insert_query(string option)
        {
            switch (option)
            {
                case "household":

                    return await ShowOption("not-reverse",true);

                case "all":
                    return await ShowOption("not-reverse",false); //基本顯示方式
                    

                case "household-reverse":

                    return await ShowOption("reverse",true);

                case "all-reverse":
                    return await ShowOption("reverse",false);

                default:
                    return await ShowOption();
            }

           
        }

        // 以戶號查詢
        public async Task<IActionResult> search_household(string household_id)
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

            Debug.WriteLine("start search_household()");
            HouseholdManagement_DBManager HM_DBmgr = new HouseholdManagement_DBManager();
            List<HouseholdMember> householdmember = await HM_DBmgr.getHousehold(household_id);

            BasicInfo_DBManager B_DBmgr = new BasicInfo_DBManager();

            List<Donate_Insert_query> don_list = new List<Donate_Insert_query> ();

            foreach (var member in householdmember)
            {
                Donate_Insert_query don = new Donate_Insert_query();
                List<BasicInfo> basicinfo = await B_DBmgr.getBasicInfo(member_id:member.MemberID);
                don.Name = basicinfo[0].Name;
                don.MID = member.MemberID;
                don.Member_number = Calculate_household(member.House_ID, member.Member_no);
                don.HouseID = member.House_ID;
                don.Is_head = member.Is_head;

                don_list.Add(don);
            }
            don_list = don_list.OrderBy(x => x.MID).OrderByDescending(x=>x.Is_head).ToList();
            Debug.WriteLine(JsonSerializer.Serialize(don_list));

            if (don_list.Count == 0)
            {
                return Json(new { message = "失敗", houseid = household_id });
            }

            return PartialView("_Household_Query",don_list);
 
        }

        // 按下新增捐獻 顯示列表 (為初始導入)
        // 後，把篩選的option加入此函式，擴建多樣性
        public async Task<IActionResult> ShowOption(string type="not-reverse",bool is_head=false)
        {
            HouseholdManagement_DBManager HM_DBmgr = new HouseholdManagement_DBManager();
            List<HouseholdMember> householdmember = await HM_DBmgr.getHousehold_all();

            // 判斷是否只需戶長
            IEnumerable<IGrouping<int, HouseholdMember>> householdmember_houseid;
            if (is_head == true)
            {
                 householdmember_houseid = householdmember.Where(x=>x.Is_head).OrderBy(x => x.House_ID).GroupBy(x => x.House_ID); //只要戶長is_head=true
            }
            else
            {
                householdmember_houseid = householdmember.OrderBy(x => x.House_ID).GroupBy(x => x.House_ID); //依照 house_id區分，並先按house_id排列
            }
                

            BasicInfo_DBManager B_DBmgr = new BasicInfo_DBManager();
            List<BasicInfo> basicinfo = await B_DBmgr.getBasicInfo();
            var basicinfo_dict = basicinfo.ToDictionary(x => x.MID);

            List<List<Donate_Insert_query>> don_insert = new List<List<Donate_Insert_query>>();

            foreach (var householdmembers in householdmember_houseid)
            {
                List<Donate_Insert_query> don_list = new List<Donate_Insert_query>();

                foreach (var member in householdmembers) // 排序按照申請先後順序 (也等同編號)
                {
                    var basicinfo_m = basicinfo_dict[member.MemberID];

                    Donate_Insert_query don = new Donate_Insert_query();

                    don.MID = member.MemberID;
                    don.HouseID = member.House_ID;
                    don.Name = basicinfo_m.Name;
                    don.Member_number = Calculate_household(member.House_ID, member.Member_no);
                    don.Is_head = member.Is_head;

                    don_list.Add(don);

                }

                // 判斷 option方式
                if (type == "not-reverse")
                {

                    //  抓戶長放第一個  
                    don_list = don_list
                        .OrderBy(x => x.MID)
                        .OrderByDescending(x => x.Is_head)
                        .ToList();
                    don_insert.Add(don_list);
                }else if(type == "reverse")
                {

                    //  抓戶長放第一個  
                    don_list = don_list
                        .OrderBy(x => x.MID)
                        .OrderByDescending(x => x.Is_head)
                        .ToList();
                    don_insert.Insert(0,don_list); //倒著放，成為reverse
                }
                
            }
            Debug.WriteLine("showoption()");
            return PartialView("_ShowDonateOption",don_insert);
        }

        // 組合編號
        public string Calculate_household(int house_id, int member_no)
        {
            string Member_number = "";

            if (house_id >= 10)
            {
                Member_number = "0" + (house_id.ToString()) + "_" + (member_no.ToString());
            }
            else
            {
                Member_number = "00" + (house_id.ToString()) + "_" + (member_no.ToString());
            }
            return Member_number;

        }

        // 提取當下狀態
        public async Task<IActionResult> DonateQuery()
        {
            // DB: householdmember 
            // DB: donation_individual 
            // DB: donation_household
            // DB: BasicInfo
            // DB: DonateType
            // DB: donation_operation !!!!可能不需要，為歷史單據
            // DB: DonateType

            HouseholdManagement_DBManager HM_DBmgr = new HouseholdManagement_DBManager();
            BasicInfo_DBManager B_DBmgr = new BasicInfo_DBManager();
            DonateOperation_DBManager DO_DBmgr = new DonateOperation_DBManager();
            DonateType_DBManager DT_DBmgr = new DonateType_DBManager();

            // 1.先取得所有人資料，以及戶號
            List<BasicInfo> basicinfo = await B_DBmgr.getBasicInfo();
            List<HouseholdMember> householdmember = await HM_DBmgr.getHousehold_all();

            // 2.取得捐獻資訊
            List<DonateIndividual> donateIndividuals = await DO_DBmgr.get_donation_individual();
            List<DonateHousehold> donateHouseholds = await DO_DBmgr.get_donation_household();
            List<DonateType> donateTypes = await DT_DBmgr.get_donatetype();

            // 轉成dict方便只進入DB一次
            var basicinfo_dict = basicinfo.ToDictionary(x => x.MID);
            var donateIndividuals_dict = donateIndividuals.ToDictionary(x => x.MID);
            var donateHouseholds_dict = donateHouseholds.ToDictionary(x => x.HouseID);
            var donateTypes_dict = donateTypes.ToDictionary(x => x.Name); //使用管理者不可去變動的Name

            var householdmember_houseid = householdmember.OrderBy(x => x.House_ID).GroupBy(x => x.House_ID); //依照 house_id區分，並先按house_id排列

            //傳給前端的，分成以戶為單位
            List<List<DonateQuery>> donateQuery = new List<List<DonateQuery>>();

            // 用戶號遞迴，依序列出每名成員
            foreach(var householdmembers in householdmember_houseid)
            {
                List<DonateQuery> don_list = new List<DonateQuery>();

                foreach (var member in householdmembers) // 排序按照申請先後順序 (也等同編號)
                {
                    var basicinfo_m = basicinfo_dict[member.MemberID];
                    var donateIndividuals_m = donateIndividuals_dict[member.MemberID];
                    var donateHouseholds_m = donateHouseholds_dict[member.House_ID];

                    DonateQuery don = new DonateQuery();

                    //基本資訊
                    don.MID = member.MemberID;
                    don.HouseID = member.House_ID;
                    don.Member_number = Calculate_household(member.House_ID, member.Member_no);
                    don.Name = basicinfo_m.Name;
                    don.Note = donateIndividuals_m.Note; //從Donate_Individual抓取

                    //判斷戶長與否
                    if (member.Is_head)
                    {
                        don.Is_head = true;
                    }

                    //判斷安斗
                    if (donateHouseholds_m.Is_dipper)
                    {
                        don.Is_dipper = true;

                        //才能點燈 大小斗
                        if (donateHouseholds_m.Dipper_big)
                        {
                            don.Dipper_price = donateTypes_dict["Dipper_big"].Price; //未來開放新增修改donatetype有問題
                            don.Dipper_name = donateTypes_dict["Dipper_big"].Name_chinese;
                        }
                        else if (donateHouseholds_m.Dipper_small)
                        {
                            don.Dipper_price = donateTypes_dict["Dipper_small"].Price;
                            don.Dipper_name = donateTypes_dict["Dipper_small"].Name_chinese;
                        }

                        // 判斷光明燈價位
                        if ((donateHouseholds_m.Dipper_big || donateHouseholds_m.Dipper_small)&&(donateIndividuals_m.Blessinglight!=null || donateIndividuals_m.Blessinglight!=""))
                        {
                            don.Blessinglight_price = donateTypes_dict[donateIndividuals_m.Blessinglight].Price; //未來 donateIndividual 存入的必須是 donateType名稱
                        }

                        //才能點燈 平安燈
                        if (donateHouseholds_m.Is_peacelight)
                        {
                            don.Peacelight_price = donateTypes_dict["PeaceLight"].Price;
                            don.Is_peacelight = true;
                        }
                    }
                    
                    if (donateHouseholds_m.Is_taisui) //判斷安太歲
                    {
                        don.Is_taisui = true;
                        don.Taisui_price = donateTypes_dict["Taisui"].Price;
                        Debug.WriteLine("有太歲");
                    }

                    don_list.Add(don);
                }

                //  抓戶長放第一個  
                don_list = don_list
                    .OrderBy(x => x.MID)
                    .OrderByDescending(x => x.Is_head)
                    .ToList();

                donateQuery.Add(don_list);
            }
            Debug.WriteLine($"donateQuery check: {JsonSerializer.Serialize(donateQuery)}");
            return View(donateQuery);

        }


    }
    

}
