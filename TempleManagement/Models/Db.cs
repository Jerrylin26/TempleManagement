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


    public class DonateType
    {
        public int ID { get; set; } // primary key 
        public string? Name { get; set; } //需要必須存在，不讓管理者動這欄位，給開發者調用資訊使用
        public string? Name_chinese { get; set; }
        public int Price { get; set; } 
        public string? Note { get; set; }
        public DateTime? ModifyDate { get; set; }

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
        public string? Blessinglight { get; set; }


        public string? Note { get; set; }

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
        public bool Is_head { get; set; }

        public bool Is_dipper { get; set; }
        public bool Is_taisui { get; set; }
        public bool Is_peacelight { get; set; }

        public string? Dipper_name { get; set; }
        public int Dipper_price { get; set; }
        public int Peacelight_price { get; set; }
        public int Blessinglight_price { get; set; }
        public int Taisui_price { get; set; }
        
        public string? Note { get; set; }

    }
}
