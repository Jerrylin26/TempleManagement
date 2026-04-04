using Microsoft.AspNetCore.Mvc;
using TempleManagement.Models;
using TempleManagement.Models.DBManager;

namespace TempleManagement.Controllers
{

    /*
     合併DonateOperation 跟 DonateQuery
     因為發現說，終究是會合併一起的。
     且功能直覺上，也比較清晰。
     
     */
    public class DonateOperation : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // 提取當下狀態
        public async Task DonateQuery()
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
            var donateTypes_dict = donateTypes.ToDictionary(x => x.Name_chinese);

            var householdmember_houseid = householdmember.GroupBy(x => x.House_ID); //依照 house_id區分

            foreach(var householdmembers in householdmember_houseid)
            {
                foreach(var member in householdmembers)
                {
                    var basicinfo_m = basicinfo_dict[member.MemberID];
                    var donateIndividuals_m = donateIndividuals_dict[member.MemberID];
                    var donateHouseholds_m = donateHouseholds_dict[member.HouseholdID];
                }
            }



        }
    }


}
