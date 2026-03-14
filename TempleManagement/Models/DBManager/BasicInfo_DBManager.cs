using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Npgsql;
using NuGet.Protocol;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace TempleManagement.Models.DBManager
{
    public class BasicInfo_DBManager
    {
        private readonly string connStr = "Data Source=(localdb)\\MSSQLLocalDB;Database=templeManagement;User ID=Jerry;Password=lccJerry1;Trusted_Connection=True";
        private readonly string connectionString_postgresql ="Host=localhost;Port=5432;Username=postgres;Password=2026fafafa;Database=templemanagement";
        
        /*------------------------------------*/
        //目前改用 postgresql，但某些MSSQL函式不會刪。
        //原由:想用mssql express ，因為tcp連接，然而，我電腦無法啟用。


        /*---------------------------------------------------------------------------------------*/
        /*
         Create: newBasicInfo()
         Read: getBasicInfo()、
         Update: AddAge()
         Delete:
        */

        // 順序很重要，在create時，會mapping
        private static string[] column_array = { "MID", "Name", "Sex", "Zodiac", "Age", "Home_num", "Phone", "Job", "Character_type", "Lunar_birthday", "Birthday", "Identical_num", "Household_address", "Current_address", "Postal_code_cur", "Postal_code_household", "Note" };
        private static string column = string.Join(",", column_array);

        // newBasicInfo
        private static string column_for_create = string.Join(",", column_array.Skip(1));
        private static string column_for_create_value = "@" + string.Join(",  @", column_array.Skip(1));

        // 取得BasicInfo 資料表 資料 
        public async Task<List<BasicInfo>> getBasicInfo(bool latest_MID = false, int member_id = 0)
        {
            using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();
            List<BasicInfo> Infos = new List<BasicInfo>();

            NpgsqlCommand cmd;

            if (latest_MID) // 拿來 newHouseholdMember()用
            {
                cmd = new NpgsqlCommand(
                    "SELECT * FROM BasicInfo ORDER BY MID DESC LIMIT 1",
                    conn
                 );
            }
            else
            {
                cmd = new NpgsqlCommand(
                    $"SELECT {column} FROM BasicInfo",
                    conn);
            }

            if (member_id != 0) // 為了單獨抓，用 member_id
            {
                cmd = new NpgsqlCommand(
                    $"SELECT {column} FROM BasicInfo where mid = {member_id}",
                    conn);
            }

            

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                int ordinal_MID = reader.GetOrdinal("MID");
                int ordinal_Name = reader.GetOrdinal("Name");
                int ordinal_Sex = reader.GetOrdinal("Sex");
                int ordinal_Zodiac = reader.GetOrdinal("Zodiac");
                int ordinal_Age = reader.GetOrdinal("Age");

                int ordinal_Home_num = reader.GetOrdinal("Home_num");
                int ordinal_phone = reader.GetOrdinal("Phone");
                int ordinal_job = reader.GetOrdinal("Job");
                int ordinal_Character_type = reader.GetOrdinal("Character_type");
                int ordinal_Identical_num = reader.GetOrdinal("Identical_num");
                int ordinal_Household_address = reader.GetOrdinal("Household_address");
                int ordinal_Current_address = reader.GetOrdinal("Current_address");
                int ordinal_Postal_code_cur = reader.GetOrdinal("Postal_code_cur");
                int ordinal_Postal_code_household = reader.GetOrdinal("Postal_code_household");
                int ordinal_Note = reader.GetOrdinal("Note");

                while (await reader.ReadAsync())
                {
                    BasicInfo Info = new BasicInfo
                    {

                        MID = reader.IsDBNull(ordinal_MID) ? 0 : reader.GetInt32(ordinal_MID),
                        Name = reader.IsDBNull(ordinal_Name) ? "" : reader.GetString(ordinal_Name),
                        Sex = reader.IsDBNull(ordinal_Sex) ? false : reader.GetBoolean(ordinal_Sex),
                        Zodiac = reader.IsDBNull(ordinal_Zodiac) ? "" : reader.GetString(ordinal_Zodiac),
                        Age = reader.IsDBNull(ordinal_Age) ? 0 : reader.GetInt32(ordinal_Age),

                        Home_num = reader.IsDBNull(ordinal_Home_num) ? "" : reader.GetString(ordinal_Home_num),
                        Phone = reader.IsDBNull(ordinal_phone) ? "" : reader.GetString(ordinal_phone),
                        Job = reader.IsDBNull(ordinal_job) ? "" : reader.GetString(ordinal_job),
                        Character_type = reader.IsDBNull(ordinal_Character_type) ? "" : reader.GetString(ordinal_Character_type),
                        Lunar_birthday = reader.IsDBNull(reader.GetOrdinal("Lunar_birthday")) ? null : reader.GetDateTime(reader.GetOrdinal("Lunar_birthday")),
                        Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday")) ? null : reader.GetDateTime(reader.GetOrdinal("Birthday")),

                        ID_num = reader.IsDBNull(ordinal_Identical_num) ? "" : reader.GetString(ordinal_Identical_num),
                        Household_address = reader.IsDBNull(ordinal_Household_address) ? "" : reader.GetString(ordinal_Household_address),
                        Current_address = reader.IsDBNull(ordinal_Current_address) ? "" : reader.GetString(ordinal_Current_address),
                        Postal_code_cur = reader.IsDBNull(ordinal_Postal_code_cur) ? "" : reader.GetString(ordinal_Postal_code_cur),
                        Postal_code_household = reader.IsDBNull(ordinal_Postal_code_household) ? "" : reader.GetString(ordinal_Postal_code_household),
                        Note = reader.IsDBNull(ordinal_Note) ? "" : reader.GetString(ordinal_Note)
                    };
                    Infos.Add(Info);
                };
            }

            return Infos;
        }


        public async Task newBasicInfo(BasicInfo user)
        {
            Debug.WriteLine("start insert");
            await using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@$"INSERT INTO BasicInfo({column_for_create}) VALUES({column_for_create_value})", conn);


            cmd.Parameters.AddWithValue("@Name", user.Name);
            cmd.Parameters.AddWithValue("@Sex", user.Sex);
            cmd.Parameters.AddWithValue("@Zodiac", user.Zodiac);
            cmd.Parameters.AddWithValue("@Age", user.Age);
            cmd.Parameters.AddWithValue("@Home_num", user.Home_num == null ? DBNull.Value : user.Home_num);
            cmd.Parameters.AddWithValue("@Phone", user.Phone == null ? DBNull.Value : user.Phone);
            cmd.Parameters.AddWithValue("@Job", user.Job == null ? DBNull.Value : user.Job);
            cmd.Parameters.AddWithValue("@Character_type", user.Character_type ?? "信徒");
            cmd.Parameters.AddWithValue("@Lunar_birthday", user.Lunar_birthday == null ? DBNull.Value : user.Lunar_birthday);
            cmd.Parameters.AddWithValue("@Birthday", user.Birthday == null ? DBNull.Value : user.Birthday);
            cmd.Parameters.AddWithValue("@Identical_num", user.ID_num == null ? DBNull.Value : user.ID_num);
            cmd.Parameters.AddWithValue("@Household_address", user.Household_address == null ? DBNull.Value : user.Household_address);
            cmd.Parameters.AddWithValue("@Current_address", user.Current_address == null ? DBNull.Value : user.Current_address);
            cmd.Parameters.AddWithValue("@Postal_code_cur", user.Postal_code_cur == null ? DBNull.Value : user.Postal_code_cur);
            cmd.Parameters.AddWithValue("@Postal_code_household", user.Postal_code_household == null ? DBNull.Value : user.Postal_code_household);
            cmd.Parameters.AddWithValue("@Note", user.Note == null ? DBNull.Value : user.Note);

            Debug.WriteLine("完成insert");

            await cmd.ExecuteNonQueryAsync();


        }
        

        //  Age 全 +1
        public async Task AddAge()
        {
            Debug.WriteLine("開始 update");
            await using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@$"Update BasicInfo set age = age+1", conn);
                  

            Debug.WriteLine("完成 update");

            await cmd.ExecuteNonQueryAsync();
            

        }

        //  Age 全 -1
        public async Task MinusAge()
        {
            Debug.WriteLine("開始 update");
            await using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@$"Update BasicInfo set age = age-1", conn);


            Debug.WriteLine("完成 update");

            await cmd.ExecuteNonQueryAsync();


        }





        /*---------------------------------------------------------------------------------------*/
        /*
         Create: newHouseholdMember()
         Read: getHouseholdMember()、
         Update:
         Delete:
        */

        // 順序很重要，在create時，會mapping
        private static string[] column_array_HouseholdMember = { "HouseholdID", "MemberID", "Member_no", "House_ID", "Is_head", "Start_date", "End_date" };
        private static string column_HouseholdMember = string.Join(",", column_array_HouseholdMember);

        // newHouseholdMember
        private static string column_for_create_HouseholdMember = string.Join(",", column_array_HouseholdMember.Skip(1));
        private static string column_for_create_value_HouseholdMember = "@" + string.Join(",  @", column_array_HouseholdMember.Skip(1));

        // 取得HouseholdMember 資料表 資料 
        public async Task<List<HouseholdMember>> getHouseholdMember(string type = "default")
        {
            List<HouseholdMember> Infos = new List<HouseholdMember>();

            using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();
            

            NpgsqlCommand cmd = new NpgsqlCommand();
            if (type == "default")
            {
                cmd = new NpgsqlCommand(
                    $"SELECT {column_HouseholdMember}   FROM HouseholdMember",
                    conn
                 );
            }else if (type == "get_head")
            {
                cmd = new NpgsqlCommand(
                    $"select {column_HouseholdMember} from householdmember where member_no=1 order by memberid desc limit 1;",
                    conn
                 );
            }

            Debug.WriteLine(cmd.CommandText);



            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                int ordinal_HouseholdID = reader.GetOrdinal("HouseholdID");
                int ordinal_MemberID = reader.GetOrdinal("MemberID");
                int ordinal_Member_no = reader.GetOrdinal("Member_no");
                int ordinal_House_ID = reader.GetOrdinal("House_ID");
                int ordinal_Is_head = reader.GetOrdinal("Is_head");
                int ordinal_Start_date = reader.GetOrdinal("Start_date");
                int ordinal_End_date = reader.GetOrdinal("End_date");

                while (await reader.ReadAsync())
                {
                    HouseholdMember Info = new HouseholdMember
                    {


                        HouseholdID = reader.IsDBNull(ordinal_HouseholdID) ? 0 : reader.GetInt32(ordinal_HouseholdID),
                        MemberID = reader.IsDBNull(ordinal_MemberID) ? 0 : reader.GetInt32(ordinal_MemberID),
                        Member_no = reader.IsDBNull(ordinal_Member_no) ? 0 : reader.GetInt32(ordinal_Member_no),
                        House_ID = reader.IsDBNull(ordinal_House_ID) ? 0 : reader.GetInt32(ordinal_House_ID),
                        Start_date = reader.IsDBNull(ordinal_Start_date) ? null : reader.GetDateTime(ordinal_Start_date),
                        End_date = reader.IsDBNull(ordinal_End_date) ? null : reader.GetDateTime(ordinal_End_date),
                        Is_head = reader.IsDBNull(ordinal_Is_head) ? false : reader.GetBoolean(ordinal_Is_head),


                    };
                    Infos.Add(Info);
                };
            };

            Debug.WriteLine(JsonSerializer.Serialize(Infos));

            return Infos;
        }

        // 寫入 HouseholdMember 資料表
        public async Task newHouseholdMember(BasicInfo user)
        {
            using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            NpgsqlCommand cmd;

            // 抓 BasicInfo 的 MID
            List<BasicInfo> basicInfo = await getBasicInfo(latest_MID:true); //因這是系統自動加一的流水號
            Debug.WriteLine($"getBasicInfo檢查--:{basicInfo}");
            List<BasicInfo> basicInfo_test = await getBasicInfo(); //因這是系統自動加一的流水號
            Debug.WriteLine($"getBasicInfo檢查test--:{JsonSerializer.Serialize(basicInfo_test)}");
            //Debug.WriteLine($"basicInfo檢查2:{JsonSerializer.Serialize(basicInfo)}");

            /*----------------------------------*/


            // 抓 getHouseholdMember Max(House_ID)
            cmd = new NpgsqlCommand(
                    @$"select MAX(House_ID) AS House_ID from HouseholdMember",
                    conn
                 );


            int houseId = 1; //系統怕空值用

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    int ordinal = reader.GetOrdinal("House_ID");
                    houseId = reader.IsDBNull(ordinal) ? 1 : reader.GetInt32(ordinal);

                    Debug.WriteLine("檢查BasicInfo: ");
                    if (user == null)
                    {
                        Debug.WriteLine("user 是 null");
                    }
                    else
                    {
                        Debug.WriteLine(JsonSerializer.Serialize(user));
                    }


                    if (user.Is_head) //是戶長 +1 戶號
                    {
                        houseId += 1;
                    }
                }
            };



            /*---------------------------------*/


            // 抓 getHouseholdMember Max(Member_no)
            cmd = new NpgsqlCommand(
                    @$"select MAX(Member_no) as Member_no from HouseholdMember where House_ID={houseId}",
                    conn
                 );
 

            int Member_no = 1; //系統怕空值用 

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    int ordinal = reader.GetOrdinal("Member_no");
                    Member_no = reader.IsDBNull(ordinal) ? 1 : reader.GetInt32(ordinal) + 1;
                }
                

            };

            /* ----------------------------------  */


            cmd = new NpgsqlCommand(@$"INSERT INTO HouseholdMember({column_for_create_HouseholdMember}) VALUES({column_for_create_value_HouseholdMember})", conn);


            cmd.Parameters.AddWithValue("@MemberID", basicInfo[0].MID); //BasicInfo
            cmd.Parameters.AddWithValue("@Member_no", Member_no); //getHouseholdMember
            cmd.Parameters.AddWithValue("@House_ID", houseId); //getHouseholdMember
            cmd.Parameters.AddWithValue("@Is_head", user.Is_head);
            cmd.Parameters.AddWithValue("@Start_date", DateTime.Now);
            cmd.Parameters.AddWithValue("@End_date", DBNull.Value);


            await cmd.ExecuteNonQueryAsync();
        }

        /*
        // 取得HouseholdMember 資料表 資料 
        public List<HouseholdMember> getHouseholdMember_MSSQL()
        {
            List<HouseholdMember> Infos = new List<HouseholdMember>();

            SqlConnection sqlConnection = new SqlConnection(connStr);
            sqlConnection.Open();
            SqlCommand sqlCommand;


            sqlCommand = new SqlCommand($"SELECT {column_HouseholdMember}   FROM HouseholdMember");


            sqlCommand.Connection = sqlConnection;

            SqlDataReader reader = sqlCommand.ExecuteReader();
            if (reader.HasRows)
            {
                int ordinal_HouseholdID = reader.GetOrdinal("HouseholdID");
                int ordinal_MemberID = reader.GetOrdinal("MemberID");
                int ordinal_Member_no = reader.GetOrdinal("Member_no");
                int ordinal_House_ID = reader.GetOrdinal("House_ID");
                int ordinal_Is_head = reader.GetOrdinal("Is_head");
                int ordinal_Start_date = reader.GetOrdinal("Start_date");
                int ordinal_End_date = reader.GetOrdinal("End_date");

                while (reader.Read())
                {
                    HouseholdMember Info = new HouseholdMember
                    {


                        HouseholdID = reader.IsDBNull(ordinal_HouseholdID) ? 0 : reader.GetInt32(ordinal_HouseholdID),
                        MemberID = reader.IsDBNull(ordinal_MemberID) ? 0 : reader.GetInt32(ordinal_MemberID),
                        Member_no = reader.IsDBNull(ordinal_Member_no) ? 0 : reader.GetInt32(ordinal_Member_no),
                        House_ID = reader.IsDBNull(ordinal_House_ID) ? 0 : reader.GetInt32(ordinal_House_ID),
                        Start_date = reader.IsDBNull(ordinal_Start_date) ? null : reader.GetDateTime(ordinal_Start_date),
                        End_date = reader.IsDBNull(ordinal_End_date) ? null : reader.GetDateTime(ordinal_End_date),
                        Is_head = reader.IsDBNull(ordinal_Is_head) ? false : reader.GetBoolean(ordinal_Is_head),


                    };
                    Infos.Add(Info);
                }
            }
            else
            {
                Debug.WriteLine("HouseholdMember資料表為空！");
            }
            sqlConnection.Close();
            return Infos;
        }*/

        /*
        // 寫入 HouseholdMember 資料表
        public async void newHouseholdMember_MSSQL(BasicInfo user)
        {
            SqlConnection sqlConnection = new SqlConnection(connStr);

            // 抓 BasicInfo 的 MID
            List<BasicInfo> basicInfo = await getBasicInfo(latest_MID: true); //因這是系統自動加一的流水號

            /------------------------------------/

            // 抓 getHouseholdMember Max(House_ID)
            sqlConnection.Open();
            SqlCommand sqlcommand_1 = new SqlCommand(@$"select MAX(House_ID) AS House_ID from HouseholdMember");
            Debug.WriteLine("select MAX(House_ID) AS House_ID from HouseholdMember");
            sqlcommand_1.Connection = sqlConnection;

            SqlDataReader reader = sqlcommand_1.ExecuteReader();
            int houseId = 1; //系統怕空值用

            if (reader.Read())
            {
                int ordinal = reader.GetOrdinal("House_ID");
                houseId = reader.IsDBNull(ordinal) ? 1 : reader.GetInt32(ordinal);

                Debug.WriteLine("檢查BasicInfo: ");
                if (user == null)
                {
                    Debug.WriteLine("user 是 null");
                }
                else
                {
                    Debug.WriteLine(JsonSerializer.Serialize(user));
                }


                if (user.Is_head) //是戶長 +1 戶號
                {
                    houseId += 1;
                }
            }
            sqlConnection.Close();

            /---------------------------------/


            // 抓 getHouseholdMember Max(Member_no)
            sqlConnection.Open();
            SqlCommand sqlcommand_2 = new SqlCommand(@$"select MAX(Member_no) as Member_no from HouseholdMember where House_ID={houseId}");
            sqlcommand_2.Connection = sqlConnection;

            SqlDataReader reader_1 = sqlcommand_2.ExecuteReader();
            int Member_no = 1; //系統怕空值用 

            if (reader_1.Read())
            {
                int ordinal = reader_1.GetOrdinal("Member_no");
                Member_no = reader_1.IsDBNull(ordinal) ? 1 : reader_1.GetInt32(ordinal) + 1;
            }
            sqlConnection.Close();

            / ----------------------------------  /

            SqlConnection sqlconnection = new SqlConnection(connStr);
            SqlCommand sqlcommand = new SqlCommand(@$"INSERT INTO HouseholdMember({column_for_create_HouseholdMember}) VALUES({column_for_create_value_HouseholdMember})");
            Debug.WriteLine(@$"INSERT INTO HouseholdMember({column_for_create_HouseholdMember}) VALUES({column_for_create_value_HouseholdMember})");
            sqlcommand.Connection = sqlconnection;


            sqlcommand.Parameters.Add(new SqlParameter("@MemberID", basicInfo[0].MID)); //BasicInfo
            sqlcommand.Parameters.Add(new SqlParameter("@Member_no", Member_no)); //getHouseholdMember
            sqlcommand.Parameters.Add(new SqlParameter("@House_ID", houseId)); //getHouseholdMember
            sqlcommand.Parameters.Add(new SqlParameter("@Is_head", user.Is_head));
            sqlcommand.Parameters.Add(new SqlParameter("@Start_date", DateTime.Now));
            sqlcommand.Parameters.Add(new SqlParameter("@End_date", DBNull.Value));


            sqlconnection.Open();
            sqlcommand.ExecuteNonQuery();
            sqlconnection.Close();
        }
        */

        /*
        public List<BasicInfo> getBasicInfo_MSSQL(bool latest_MID = false)
        {
            List<BasicInfo> Infos = new List<BasicInfo>();
            SqlConnection sqlConnection = new SqlConnection(connStr);
            sqlConnection.Open();
            SqlCommand sqlCommand;

            if (latest_MID) //給 newHouseholdMember() 用的，取最新的MID
            {
                sqlCommand = new SqlCommand($"SELECT TOP 1 * FROM BasicInfo ORDER BY MID DESC");
            }
            else
            {
                sqlCommand = new SqlCommand($"SELECT {column}   FROM BasicInfo");
            }

            sqlCommand.Connection = sqlConnection;

            SqlDataReader reader = sqlCommand.ExecuteReader();
            if (reader.HasRows)
            {
                int ordinal_MID = reader.GetOrdinal("MID");
                int ordinal_Name = reader.GetOrdinal("Name");
                int ordinal_Sex = reader.GetOrdinal("Sex");
                int ordinal_Zodiac = reader.GetOrdinal("Zodiac");
                int ordinal_Age = reader.GetOrdinal("Age");

                int ordinal_Home_num = reader.GetOrdinal("Home_num");
                int ordinal_phone = reader.GetOrdinal("Phone");
                int ordinal_job = reader.GetOrdinal("Job");
                int ordinal_Character_type = reader.GetOrdinal("Character_type");
                int ordinal_Identical_num = reader.GetOrdinal("Identical_num");
                int ordinal_Household_address = reader.GetOrdinal("Household_address");
                int ordinal_Current_address = reader.GetOrdinal("Current_address");
                int ordinal_Postal_code_cur = reader.GetOrdinal("Postal_code_cur");
                int ordinal_Postal_code_household = reader.GetOrdinal("Postal_code_household");
                int ordinal_Note = reader.GetOrdinal("Note");
                while (reader.Read())
                {
                    BasicInfo Info = new BasicInfo
                    {


                        MID = reader.IsDBNull(ordinal_MID) ? 0 : reader.GetInt32(ordinal_MID),
                        Name = reader.IsDBNull(ordinal_Name) ? "" : reader.GetString(ordinal_Name),
                        Sex = reader.IsDBNull(ordinal_Sex) ? false : reader.GetBoolean(ordinal_Sex),
                        Zodiac = reader.IsDBNull(ordinal_Zodiac) ? "" : reader.GetString(ordinal_Zodiac),
                        Age = reader.IsDBNull(ordinal_Age) ? 0 : reader.GetInt32(ordinal_Age),

                        Home_num = reader.IsDBNull(ordinal_Home_num) ? "" : reader.GetString(ordinal_Home_num),
                        Phone = reader.IsDBNull(ordinal_phone) ? "" : reader.GetString(ordinal_phone),
                        Job = reader.IsDBNull(ordinal_job) ? "" : reader.GetString(ordinal_job),
                        Character_type = reader.IsDBNull(ordinal_Character_type) ? "" : reader.GetString(ordinal_Character_type),
                        Lunar_birthday = reader.IsDBNull(reader.GetOrdinal("Lunar_birthday")) ? null : reader.GetDateTime(reader.GetOrdinal("Lunar_birthday")),
                        Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday")) ? null : reader.GetDateTime(reader.GetOrdinal("Birthday")),

                        ID_num = reader.IsDBNull(ordinal_Identical_num) ? "" : reader.GetString(ordinal_Identical_num),
                        Household_address = reader.IsDBNull(ordinal_Household_address) ? "" : reader.GetString(ordinal_Household_address),
                        Current_address = reader.IsDBNull(ordinal_Current_address) ? "" : reader.GetString(ordinal_Current_address),
                        Postal_code_cur = reader.IsDBNull(ordinal_Postal_code_cur) ? "" : reader.GetString(ordinal_Postal_code_cur),
                        Postal_code_household = reader.IsDBNull(ordinal_Postal_code_household) ? "" : reader.GetString(ordinal_Postal_code_household),
                        Note = reader.IsDBNull(ordinal_Note) ? "" : reader.GetString(ordinal_Note),

                    };
                    Infos.Add(Info);
                }
            }
            else
            {
                Debug.WriteLine("BasicInfo資料表為空！");
            }
            sqlConnection.Close();
            return Infos;
        }

        // 寫入 BasicInfo 資料表
        public void newBasicInfo_MSSQL(BasicInfo user)
        {

            SqlConnection sqlconnection = new SqlConnection(connStr);
            SqlCommand sqlcommand = new SqlCommand(@$"INSERT INTO BasicInfo({column_for_create}) VALUES({column_for_create_value})");
            Debug.WriteLine(@$"INSERT INTO BasicInfo({column_for_create}) VALUES({column_for_create_value})");
            sqlcommand.Connection = sqlconnection;

            sqlcommand.Parameters.Add(new SqlParameter("@Name", user.Name));
            sqlcommand.Parameters.Add(new SqlParameter("@Sex", user.Sex));
            sqlcommand.Parameters.Add(new SqlParameter("@Zodiac", user.Zodiac));
            sqlcommand.Parameters.Add(new SqlParameter("@Age", user.Age ));
            sqlcommand.Parameters.Add(new SqlParameter("@Home_num", user.Home_num == null ? DBNull.Value : user.Home_num));
            sqlcommand.Parameters.Add(new SqlParameter("@Phone", user.Phone == null ? DBNull.Value : user.Phone));
            sqlcommand.Parameters.Add(new SqlParameter("@Job", user.Job == null ? DBNull.Value : user.Job));
            sqlcommand.Parameters.Add(new SqlParameter("@Character_type", user.Character_type ?? "信徒"));
            sqlcommand.Parameters.Add(new SqlParameter("@Lunar_birthday", user.Lunar_birthday == null ? DBNull.Value : user.Lunar_birthday));
            sqlcommand.Parameters.Add(new SqlParameter("@Birthday", user.Birthday == null ? DBNull.Value : user.Birthday));
            sqlcommand.Parameters.Add(new SqlParameter("@Identical_num", user.ID_num == null ? DBNull.Value : user.ID_num));
            sqlcommand.Parameters.Add(new SqlParameter("@Household_address", user.Household_address == null ? DBNull.Value : user.Household_address));
            sqlcommand.Parameters.Add(new SqlParameter("@Current_address", user.Current_address == null ? DBNull.Value : user.Current_address));
            sqlcommand.Parameters.Add(new SqlParameter("@Postal_code_cur", user.Postal_code_cur == null ? DBNull.Value : user.Postal_code_cur));
            sqlcommand.Parameters.Add(new SqlParameter("@Postal_code_household", user.Postal_code_household == null ? DBNull.Value : user.Postal_code_household));
            sqlcommand.Parameters.Add(new SqlParameter("@Note", user.Note == null ? DBNull.Value : user.Note));

            sqlconnection.Open();
            sqlcommand.ExecuteNonQuery();
            sqlconnection.Close();
        }
        */
    }
}
