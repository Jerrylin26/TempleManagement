using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Collections.Generic;
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

        // 更改 大小斗
        [HttpPost]
        public async Task<IActionResult> Change_Dipper_selectOption(List<DonationSubmit> DonationSubmit)
        {

            Debug.WriteLine("Change_Dipper_selectOption()");

            Debug.WriteLine(JsonSerializer.Serialize(DonationSubmit));

            if (!ModelState.IsValid) //驗證 Post 來的資料

            {
                Debug.WriteLine($"ModelState: {JsonSerializer.Serialize(DonationSubmit)}");
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

            // 從 Post_selected_option 借用來的 框架
            //傳給前端的，分成以戶為單位

            // 當安斗從 無 -> 大/小斗， 抓取DB資訊 (考量到管理者亂按，遺失上次資訊，而未需修改)
            bool Is_getDB = DonationSubmit.Any(x => x.DonateTypeId == 999);
            DonationSubmit = DonationSubmit.Where(x=>x.DonateTypeId!=999).ToList(); // 刻意設 DonateTypeId=999 為 選項[無] ，把它移掉

            
            List<DonateQuery> donateQuery;
            // 判斷回傳到html 資料模式
            if(Is_getDB)
            {
                Debug.WriteLine("NA_Is_getDB");
                donateQuery = await IDonateQuery_basedOn_DonationSubmit(DonationSubmit);
            }
            else // 無 -> 安斗
            {
                int Dipper = 0;
                if (DonationSubmit.Any(x => x.DonateTypeId <= 2))
                {
                    Dipper = DonationSubmit.FirstOrDefault(x => x.DonateTypeId <= 2).DonateTypeId;
                }

                if(Dipper == 0) { throw new Exception("錯誤 安斗"); }

                Debug.WriteLine("Is_getDB");
                List<List<DonateQuery>> donateQuery_all = await IDonateQuery(true, Dipper); // 加入true 解決還原當初DB資料 但是安斗會改變
                var houseId = DonationSubmit.FirstOrDefault()?.HouseId;

                donateQuery = donateQuery_all
                    .FirstOrDefault(x => x.Any() && x.First().HouseID == houseId)
                    ?? new List<DonateQuery>();
            }


                // 使用DonationViewModel 拿大資料
                DonationViewModel merge_info = new DonationViewModel();


            // DonateType
            DonateType_DBManager dBManager = new DonateType_DBManager();
            List<DonateType> info_donatetype = await dBManager.get_donatetype();
            info_donatetype = info_donatetype.OrderBy(g => g.Price).ToList();

            merge_info.DonateType = info_donatetype;
            merge_info.DonateQuery = donateQuery;

            Debug.WriteLine("Change_Dipper_selectOption: ");
            Debug.WriteLine(JsonSerializer.Serialize(donateQuery));

            return PartialView("_Create_Donation", merge_info);



        }

        // post Donation項目
        // 提交新增 選定人選 準備進入新增表單 
        [HttpPost]
        public async Task<IActionResult> Donation_Submit(List<DonationSubmit> DonationSubmit)
        {

            Debug.WriteLine("start post_selected_option()");

            Debug.WriteLine(JsonSerializer.Serialize(DonationSubmit));

            if (!ModelState.IsValid) //驗證 Post 來的資料

            {
                Debug.WriteLine($"ModelState: {JsonSerializer.Serialize(DonationSubmit)}");
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



            return Json(new {success = true });

         

        }

        // 即時更新總價格 
        [HttpPost]
        public IActionResult UpdateTotalPrice(List<int> prices)
        {
            int total = prices.Sum();

            return Json(new
            {
                success = true,
                price = total
            });
        }

        // 提交新增 選定人選 準備進入新增表單 
        public async Task<IActionResult> Post_selected_option(List<Donate_Insert_query> donateOperations) 
        {

            Debug.WriteLine("start post_selected_option()");

            Debug.WriteLine(JsonSerializer.Serialize(donateOperations));

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


            //改成DonateQuery
            //List<DonateQuery> Go_To_Insert_Donation = new List<DonateQuery>();

            //傳給前端的，分成以戶為單位
            List<List<DonateQuery>> donateQuery = await IDonateQuery();
            var donateQuery_dict = donateQuery.Where(x => x.Any()).ToDictionary(x => x.First().HouseID);
            // 使用DonationViewModel 拿大資料
            DonationViewModel merge_info = new DonationViewModel();
            var info_donateQuery = new List<DonateQuery>();


            // DonateQuery
            foreach (var info in donateOperations)
            {
                if (info.Dummy_Is_checked == "on") // 有勾選的
                {
                    DonateQuery don = new DonateQuery();
                    //Debug.WriteLine("yes");
                    info_donateQuery = donateQuery_dict[info.HouseID];
                }
            }

            // DonateType
            DonateType_DBManager dBManager = new DonateType_DBManager();
            List<DonateType> info_donatetype = await dBManager.get_donatetype();
            info_donatetype = info_donatetype.OrderBy(g => g.Price).ToList();

            merge_info.DonateType = info_donatetype;
            merge_info.DonateQuery = info_donateQuery;


            return PartialView("_Create_Donation", merge_info);

        }


        // 篩選button 不同方式顯示人選 (新增捐獻)
        public async Task<IActionResult> dropdown_insert_query(string option)
        {
            /*
             * all 跟 all-reverse移除，因為只讓管理者選取戶長(戶號)，進去再去選擇人員
             */
            switch (option)
            {
                case "household":

                    return await ShowOption("not-reverse",true);

                //case "all":
                //    return await ShowOption("not-reverse",false); //基本顯示方式
                    

                case "household-reverse":

                    return await ShowOption("reverse",true);

                //case "all-reverse":
                //    return await ShowOption("reverse",false);

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
        public async Task<IActionResult> ShowOption(string type="not-reverse",bool is_head=true)
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

            //傳給前端的，分成以戶為單位
            List<List<DonateQuery>> donateQuery = await IDonateQuery();

            DonationViewModel_DonateQuery merge_info = new DonationViewModel_DonateQuery();


            // DonateType
            DonateType_DBManager dBManager = new DonateType_DBManager();
            List<DonateType> info_donatetype = await dBManager.get_donatetype();
            info_donatetype = info_donatetype.OrderBy(g => g.Price).ToList();

            merge_info.DonateType = info_donatetype;
            merge_info.DonateQuery = donateQuery;

            return View(merge_info);

        }

        // 做成 Interface給 DonateQuery()
        public async Task<List<List<DonateQuery>>> IDonateQuery(bool Is_NoDipper = false, int Dipper=1)
        {
            // DB: householdmember 
            // DB: donation_individual 
            // DB: donation_household
            // DB: BasicInfo
            // DB: DonateType
            // DB: donation_operation !!!!可能不需要，為歷史單據
            // DB: DonateType

            //傳給前端的，分成以戶為單位
            List<List<DonateQuery>> donateQuery = new List<List<DonateQuery>>();


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
            var donateTypes_dict = donateTypes.ToDictionary(x => x.ID); // 更改為ＩＤ

            var householdmember_houseid = householdmember.OrderBy(x => x.House_ID).GroupBy(x => x.House_ID); //依照 house_id區分，並先按house_id排列

            Debug.WriteLine($"{JsonSerializer.Serialize(donateIndividuals)}");

            // 用戶號遞迴，依序列出每名成員
            foreach (var householdmembers in householdmember_houseid)
            {
                List<DonateQuery> don_list = new List<DonateQuery>();

                foreach (var member in householdmembers) // 排序按照申請先後順序 (也等同編號)
                {
                    //Debug.WriteLine($"memberid: {member.MemberID}");
                    var basicinfo_m = basicinfo_dict[member.MemberID];
                    // 用來解 DonationIndividual 沒有 donatetypeid
                    var donateIndividuals_m =
                        donateIndividuals_dict.TryGetValue(member.MemberID, out var value)
                        ? value
                        : new DonateIndividual
                        {
                            DonateItem_idv = new List<DonationItem>()
                        };
                    var donateHouseholds_m = donateHouseholds_dict[member.House_ID];

                    DonateQuery don = new DonateQuery();

                    //基本資訊
                    don.MID = member.MemberID;
                    don.HouseID = member.House_ID;
                    don.Member_number = Calculate_household(member.House_ID, member.Member_no);
                    don.Name = basicinfo_m.Name;
                    don.Note = donateIndividuals_m.Note; //從Donate_Individual抓取

                    // 用來放捐獻資訊 未來移轉至這
                    List<DonationItem> donateitem = new List<DonationItem>();

                    //判斷戶長與否
                    if (member.Is_head)
                    {
                        don.Is_head = true;
                    }

                    // 加入判斷 無 -> 安斗 復原DB資料 但Dipper有變更
                    if (Is_NoDipper)
                    {
                        foreach (var item in donateHouseholds_m.DonateItem_idv.Where(x => x.DonateTypeId <= 2))
                        {
                            item.DonateTypeId = Dipper;
                            item.SelectedPrice = donateTypes_dict[Dipper].Price;
                            item.Name_chinese = donateTypes_dict[Dipper].Name_chinese;
                        }
                    }


                    //判斷安斗 
                    // 照著 Donatetype 邏輯，所以沒有安斗，資料送不出來
                    if (donateHouseholds_m.DonateItem_idv.Any(x => x.DonateTypeId <= 2)) // 小斗:2    大斗:1
                    {
                        don.Is_dipper = true;

                        //才能點燈 大小斗
                        if (donateHouseholds_m.DonateItem_idv.Any(x => x.DonateTypeId == 1))
                        {

                            donateitem.Add(new DonationItem
                            {
                                DonateTypeId = donateTypes_dict[1].ID,
                                Name_chinese = donateTypes_dict[1].Name_chinese,
                                SelectedPrice = donateTypes_dict[1].Price,
                                Prototype_name = donateTypes_dict[1].Prototype_name,
                                Prototype = donateTypes_dict[1].Prototype,
                                Category = donateTypes_dict[1].Category,
                                Category_name = donateTypes_dict[1].Category_name,
                            });

                        }
                        else if (donateHouseholds_m.DonateItem_idv.Any(x => x.DonateTypeId == 2))
                        {

                            donateitem.Add(new DonationItem
                            {
                                DonateTypeId = donateTypes_dict[2].ID,
                                Name_chinese = donateTypes_dict[2].Name_chinese,
                                SelectedPrice = donateTypes_dict[2].Price,
                                Prototype_name = donateTypes_dict[2].Prototype_name,
                                Prototype = donateTypes_dict[2].Prototype,
                                Category = donateTypes_dict[2].Category,
                                Category_name = donateTypes_dict[2].Category_name,
                            });
                        }

                        /*
                        * 安斗 所有都能抓 (需安斗 + 不須安斗)
                        */

                        foreach (var dipper_case in donateTypes.Where(x => x.ID > 2)) // 排除安斗
                        {
                            if (donateIndividuals_m.DonateItem_idv.Any(x => x.DonateTypeId == dipper_case.ID) || donateHouseholds_m.DonateItem_idv.Any(x => x.DonateTypeId == dipper_case.ID))
                            {
                                var selected = donateTypes_dict[dipper_case.ID];

                                donateitem.Add(new DonationItem
                                {
                                    DonateTypeId = selected.ID,
                                    Name_chinese = selected.Name_chinese,
                                    SelectedPrice = selected.Price,
                                    Prototype_name = selected.Prototype_name,
                                    Prototype = selected.Prototype,
                                    Category = selected.Category,
                                    Category_name = selected.Category_name,
                                });
                            }

                        }


                    }
                    else
                    {
                        /*
                        * 把這邏輯改成 NeedDipper = false 
                        * 只要沒有安斗，只能抓不虛安斗的
                        */
                        foreach (var dipper_case in donateTypes.Where(x => x.NeedDipper == false && x.ID > 2)) // ID >2 為安斗除外
                        {
                            if (donateIndividuals_m.DonateItem_idv.Any(x => x.DonateTypeId == dipper_case.ID) || donateHouseholds_m.DonateItem_idv.Any(x => x.DonateTypeId == dipper_case.ID))
                            {
                                var selected = donateTypes_dict[dipper_case.ID];

                                donateitem.Add(new DonationItem
                                {
                                    DonateTypeId = selected.ID,
                                    Name_chinese = selected.Name_chinese,
                                    SelectedPrice = selected.Price,
                                    Prototype_name = selected.Prototype_name,
                                    Prototype = selected.Prototype,
                                    Category = selected.Category,
                                    Category_name = selected.Category_name,
                                });
                            }
                        }
                    }


                    don.Donate_item = donateitem;
                    //Debug.WriteLine($"檢查donateitem{JsonSerializer.Serialize(donateitem)}");

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
            return donateQuery;

        }


        // 為了重新計算 DonateQuery 基於給定特定 DonationSubmit
        public async Task<List<DonateQuery>> IDonateQuery_basedOn_DonationSubmit(List<DonationSubmit> donationSubmits)
        {
            // DB: householdmember 
            // DB: donation_individual 
            // DB: donation_household
            // DB: BasicInfo
            // DB: DonateType
            // DB: donation_operation !!!!可能不需要，為歷史單據
            // DB: DonateType

            //傳給前端的，分成以戶為單位
            List<DonateQuery> don_list = new List<DonateQuery>();


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
            var donateTypes_dict = donateTypes.ToDictionary(x => x.ID); // 更改為ＩＤ

            var householdmember_houseid = householdmember.OrderBy(x => x.House_ID).GroupBy(x => x.House_ID); //依照 house_id區分，並先按house_id排列

            Debug.WriteLine($"{JsonSerializer.Serialize(donateIndividuals)}");


            // 更改處在這
            var donationSubmits_dict_idv = donationSubmits.Where(x => x.MID != 0).GroupBy(x => x.MID).ToDictionary(x => x.Key, x=>x.ToList());
            var donationSubmits_dict_household = donationSubmits.Where(x => x.HouseId != 0).GroupBy(x => x.HouseId).ToDictionary(x => x.Key, x => x.ToList());

            // 用戶號遞迴，依序列出每名成員
            foreach (var householdmembers in householdmember_houseid)
            {
                if(householdmembers.FirstOrDefault()?.House_ID == donationSubmits.FirstOrDefault()?.HouseId ) // 找出要的 houseid
                {
                    foreach (var member in householdmembers) // 排序按照申請先後順序 (也等同編號)
                    {
                        //Debug.WriteLine($"memberid: {member.MemberID}");
                        var basicinfo_m = basicinfo_dict[member.MemberID];
                        // 用來解 DonationIndividual 沒有 donatetypeid
                        var donateIndividuals_m =
                            donateIndividuals_dict.TryGetValue(member.MemberID, out var value)
                            ? value
                            : new DonateIndividual
                            {
                                DonateItem_idv = new List<DonationItem>()
                            };
                        var donateHouseholds_m = donateHouseholds_dict[member.House_ID];

                        // 更改處在這
                        var donationSubmits_idv_m = donationSubmits_dict_idv[member.MemberID];
                        var donationSubmits_household_m = donationSubmits_dict_household[member.House_ID];

                        DonateQuery don = new DonateQuery();

                        //基本資訊
                        don.MID = member.MemberID;
                        don.HouseID = member.House_ID;
                        don.Member_number = Calculate_household(member.House_ID, member.Member_no);
                        don.Name = basicinfo_m.Name;
                        don.Note = donateIndividuals_m.Note; //從Donate_Individual抓取

                        // 用來放捐獻資訊 未來移轉至這
                        List<DonationItem> donateitem = new List<DonationItem>();

                        //判斷戶長與否
                        if (member.Is_head)
                        {
                            don.Is_head = true;
                        }


                        

                        //判斷安斗 
                        // 照著 Donatetype 邏輯，所以沒有安斗，資料送不出來
                        if (donationSubmits_household_m.Any(x=>x.DonateTypeId <= 2)) // 小斗:2    大斗:1
                        {
                            don.Is_dipper = true;

                            //才能點燈 大小斗
                            if (donationSubmits_household_m.Any(x=>x.DonateTypeId == 1))
                            {

                                donateitem.Add(new DonationItem
                                {
                                    DonateTypeId = donateTypes_dict[1].ID,
                                    Name_chinese = donateTypes_dict[1].Name_chinese,
                                    SelectedPrice = donateTypes_dict[1].Price,
                                    Prototype_name = donateTypes_dict[1].Prototype_name,
                                    Prototype = donateTypes_dict[1].Prototype,
                                    Category = donateTypes_dict[1].Category,
                                    Category_name = donateTypes_dict[1].Category_name,
                                });

                            }
                            else if (donationSubmits_household_m.Any(x => x.DonateTypeId == 2))
                            {

                                donateitem.Add(new DonationItem
                                {
                                    DonateTypeId = donateTypes_dict[2].ID,
                                    Name_chinese = donateTypes_dict[2].Name_chinese,
                                    SelectedPrice = donateTypes_dict[2].Price,
                                    Prototype_name = donateTypes_dict[2].Prototype_name,
                                    Prototype = donateTypes_dict[2].Prototype,
                                    Category = donateTypes_dict[2].Category,
                                    Category_name = donateTypes_dict[2].Category_name,
                                });
                            }


                            /*
                             * 抓 (需安斗 + 不需安斗)
                             */

                            foreach (var dipper_case in donateTypes.Where(x=>x.ID>2)) // 排除安斗
                            {
                                if (donationSubmits_idv_m.Any(x=>x.DonateTypeId == dipper_case.ID) || donationSubmits_household_m.Any(x => x.DonateTypeId == dipper_case.ID))
                                {
                                    var selected = donateTypes_dict[dipper_case.ID];

                                    donateitem.Add(new DonationItem
                                    {
                                        DonateTypeId = selected.ID,
                                        Name_chinese = selected.Name_chinese,
                                        SelectedPrice = selected.Price,
                                        Prototype_name = selected.Prototype_name,
                                        Prototype = selected.Prototype,
                                        Category = selected.Category,
                                        Category_name = selected.Category_name,
                                    });
                                }
                            }
                        }
                        else
                        {
                            /*
                            * 把這邏輯改成 NeedDipper = false 
                            * 只要沒有安斗，只能抓不虛安斗的
                            */
                            foreach (var dipper_case in donateTypes.Where(x => x.NeedDipper == false && x.ID > 2)) // ID >2 為安斗除外
                            {
                                if (donationSubmits_idv_m.Any(x => x.DonateTypeId == dipper_case.ID) || donationSubmits_household_m.Any(x => x.DonateTypeId == dipper_case.ID))
                                {
                                    var selected = donateTypes_dict[dipper_case.ID];

                                    donateitem.Add(new DonationItem
                                    {
                                        DonateTypeId = selected.ID,
                                        Name_chinese = selected.Name_chinese,
                                        SelectedPrice = selected.Price,
                                        Prototype_name = selected.Prototype_name,
                                        Prototype = selected.Prototype,
                                        Category = selected.Category,
                                        Category_name = selected.Category_name,
                                    });
                                }
                            }
                        }

                        don.Donate_item = donateitem;
                        //Debug.WriteLine($"檢查donateitem{JsonSerializer.Serialize(donateitem)}");

                        don_list.Add(don);
                    }

                    //  抓戶長放第一個  
                    don_list = don_list
                        .OrderBy(x => x.MID)
                        .OrderByDescending(x => x.Is_head)
                        .ToList();

                    Debug.WriteLine($"donateQuery check: {JsonSerializer.Serialize(don_list)}");
                    return don_list;
                } 

            }

            return don_list;
        }
    }
    

}
