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


        public int MID { get; set; }
        public string Real_MID { get; set; }
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


    }
}
