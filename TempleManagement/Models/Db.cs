using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text.Json.Serialization;

namespace TempleManagement.Models
{
    public class BasicInfo    
    {
        // BasicInfo 資料表


        public int MID { get; set; } // primary key
        public int Age { get; set; } //
        public bool Sex { get; set; }
        public string? Character_type { get; set; }
        public string? Job { get; set; } // 
        public string? Phone { get; set; } // 
        public string? Home_num { get; set; } // 
        public string Zodiac { get; set; } // 
        public string Name { get; set; } // 
        public DateTime? Lunar_birthday { get; set; }
        public DateTime? Birthday { get; set; } // 
        public string? ID_num { get; set; } // 身分證字號
        public string? Household_address { get; set; } // 
        public string? Current_address { get; set; } // 
        public string? Postal_code_cur { get; set; } //
        public string? Postal_code_household { get; set; }
        public string? Note { get; set; }
        public bool Is_head { get; set; } // 使否為戶長


    }
    public class HouseholdMember
    {
        // HouseholdMember 資料表
        // 基本上只有讀，沒有 create (依附於BasicInfo)


        public int HouseholdID { get; set; } // primary key 1
        public int MemberID { get; set; } 
        public int Member_no { get; set; }
        public int House_ID { get; set; }
        public bool Is_head { get; set; } 
        public DateTime? Start_date { get; set; }
        public DateTime? End_date { get; set; } 

    }

    public class Admin
    {
        public int AdminID { get; set; } // primary key
        public string Account { get; set; }
        public string Password { get; set; }
        public string? Name { get; set; }

    }

    // 為了給 ChangeHousehold 頁面，BasicInfo 跟 HouseholdMember 資料
    public class ChangeHousehold
    {
        public int HouseholdID { get; set; } // primary key 1
        public int MemberID { get; set; } 
        public int Member_no { get; set; }
        public int House_ID { get; set; }
        public bool Is_head { get; set; }
        public DateTime? Start_date { get; set; }
        public DateTime? End_date { get; set; }

        public int Age { get; set; }
        public string Name { get; set; }
    }

    // Donate 選項
    public class DonateType
    {
        public int ID { get; set; } // primary key 
        public string? Name { get; set; } //需要必須存在，不讓管理者動這欄位，給開發者調用資訊使用
        public string? Name_chinese { get; set; }
        public int Price { get; set; } 
        public string? Note { get; set; }
        public DateTime? ModifyDate { get; set; }

        public int Prototype { get; set; } // DB: DonateType_Prototype 拿來分類所屬類別
        public string Prototype_name { get; set; } 
        public bool NeedDipper { get; set; } // 標記需要Dipper(有安斗的項目) *大小斗我都設false

    }

    // 用來記錄歷史單據，而非現狀捐獻情況
    public class DonateOperation
    {
        public int DonationID { get; set; } // primary key 
        public int MID { get; set; } //與 basicInfo primary key對應
        public DateTime? Date { get; set; }
        public int Price { get; set; }
        public string? Donation_type { get; set; }

        public string? Note { get; set; }

    }

    // DonateHousehold + DonateIndividual 紀錄當前狀態
    public class DonateIndividual
    {
        public int DI_ID { get; set; } // primary key 
        public int MID { get; set; } //與 basicInfo primary key對應
        //public string? Blessinglight { get; set; }

        public string? Note { get; set; }

        // V1 改資料表結構 : 會動到系統運作
        public List<DonationItem> DonateItem_idv { get; set; }

    }

    public class DonateHousehold
    {
        public int DH_ID { get; set; } // primary key 
        public int HouseID { get; set; } //與 basicInfo primary key對應
        public bool Is_dipper { get; set; }
        public bool Is_taisui { get; set; }
        public bool Is_peacelight { get; set; }
        public bool Dipper_big { get; set; }
        public bool Dipper_small { get; set; }

        public string? Note { get; set; }

    }

    // 顯示Donate查詢 顯示資料
    public class DonateQuery
    {
        public int MID { get; set; } //MemberId
        public int HouseID { get; set; }
        public string Name { get; set; }
        public string? Member_number { get; set; }
        public bool? Is_head { get; set; }

        public bool Is_dipper { get; set; }
        public bool Is_taisui { get; set; }
        public bool Is_peacelight { get; set; }

        public string? Dipper_name { get; set; }
        public int Dipper_price { get; set; }
        public int Peacelight_price { get; set; }
        public int Blessinglight_price { get; set; }
        public int Taisui_price { get; set; }

        // 即將把DonateItem 整合一起 
        public List<DonationItem>? Donate_item { get; set; }

        public string? Note { get; set; }

    }

    // 加入至DonateQuery的類型List
    public class DonationItem
    {
        public int DonateTypeId { get; set; } // 用DonateTypeId來 group ，以便在下拉式選單加入 
        public string Name_chinese { get; set; } // 自己捐贈名稱
        public string Prototype_name { get; set; } //類別名稱
        public int Prototype { get; set; } //類別名稱

        public int SelectedPrice { get; set; }


    }

    // 顯示Donate查詢 顯示資料
    public class Donate_Insert_query
    {
        public int MID { get; set; } //MemberId
        public int HouseID { get; set; }
        public string? Name { get; set; }
        public string? Member_number { get; set; }
        public bool? Is_head { get; set; }
        public bool? Is_checked { get; set; } // 判斷此人是否要新增
        public string? Dummy_Is_checked { get; set; } // 因為javascript接收是on 就用這個變數接收吧
    }

    // 在 Create_Donation 需要 DonateQuery + DonateType
    public class DonationViewModel
    {
        public List<DonateQuery> DonateQuery { get; set; }
        public List<DonateType> DonateType { get; set; }
    }
}
