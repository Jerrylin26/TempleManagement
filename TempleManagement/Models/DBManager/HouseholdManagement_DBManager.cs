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
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace TempleManagement.Models.DBManager
{
    public class HouseholdManagement_DBManager
    {
        private readonly string connStr = "Data Source=(localdb)\\MSSQLLocalDB;Database=templeManagement;User ID=Jerry;Password=lccJerry1;Trusted_Connection=True";
        private readonly string connectionString_postgresql = "Host=localhost;Port=5432;Username=postgres;Password=2026fafafa;Database=templemanagement";

        /*------------------------------------*/
        //目前改用 postgresql，但某些MSSQL函式不會刪。
        //原由:想用mssql express ，因為tcp連接，然而，我電腦無法啟用。


        /*---------------------------------------------------------------------------------------*/
        /*
         Create: 
         Read: getHousehold()、getHousehold_by_basicinfo()
         Update: changeHead()
         Delete:
        */

        // 順序很重要，在create時，會mapping
        private static string[] column_array = { "householdid", "memberid", "member_no", "house_id", "is_head", "start_date", "end_date" };
        private static string column = string.Join(",", column_array);

        // getHousehold
        private static string column_for_create = string.Join(",", column_array.Skip(1));
        private static string column_for_create_value = "@" + string.Join(",  @", column_array.Skip(1));

        // 取得HouseholdMember資料表 資料 
        public async Task<List<HouseholdMember>> getHousehold(string house_id)
        {
            using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            NpgsqlCommand cmd;
            List<HouseholdMember> Infos = new List<HouseholdMember>();



            cmd = new NpgsqlCommand(
                $"SELECT * FROM HouseholdMember where house_id= @house_id ",
                conn);

            cmd.Parameters.AddWithValue("@house_id", int.Parse(house_id));


            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                int ordinal_householdid = reader.GetOrdinal("householdid");
                int ordinal_memberid = reader.GetOrdinal("memberid");
                int ordinal_member_no = reader.GetOrdinal("member_no");
                int ordinal_house_id = reader.GetOrdinal("house_id");
                int ordinal_is_head = reader.GetOrdinal("is_head");
                int ordinal_start_date = reader.GetOrdinal("start_date");
                int ordinal_end_date = reader.GetOrdinal("end_date");


                while (await reader.ReadAsync())
                {
                    HouseholdMember Info = new HouseholdMember
                    {

                        HouseholdID = reader.IsDBNull(ordinal_householdid) ? 0 : reader.GetInt32(ordinal_householdid),
                        MemberID = reader.IsDBNull(ordinal_memberid) ? 0 : reader.GetInt32(ordinal_memberid),
                        Member_no = reader.IsDBNull(ordinal_member_no) ? 0 : reader.GetInt32(ordinal_member_no),
                        House_ID = reader.IsDBNull(ordinal_house_id) ? 0 : reader.GetInt32(ordinal_house_id),
                        Is_head = reader.IsDBNull(ordinal_is_head) ? false : reader.GetBoolean(ordinal_is_head),
                        Start_date = reader.IsDBNull(ordinal_start_date) ? null : reader.GetDateTime(ordinal_start_date),
                        End_date = reader.IsDBNull(ordinal_end_date) ? null : reader.GetDateTime(ordinal_end_date),

                    };

                    Infos.Add(Info);

                }
                ;

            }
            Debug.WriteLine($"check return back to controller{JsonSerializer.Serialize(Infos)}");

            return Infos;

        }



        public async Task changeHead(int mid, int houseid)
        {
            Debug.WriteLine("start update");
            await using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@$"UPDATE householdmember SET  is_head = false where house_id = @house_id; UPDATE householdmember SET  is_head = true where memberid = @mid;", conn);

            cmd.Parameters.AddWithValue("@house_id", houseid);
            cmd.Parameters.AddWithValue("@mid", mid);


            Debug.WriteLine("完成update");

            await cmd.ExecuteNonQueryAsync();


        }

        // 取得HouseholdMember資料表 資料 by BasicInfo info
        public async Task<List<HouseholdMember>> getHousehold_by_basicinfo(BasicInfo info)
        {
            using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            NpgsqlCommand cmd;
            List<HouseholdMember> Infos = new List<HouseholdMember>();



            cmd = new NpgsqlCommand(
                $"SELECT * FROM HouseholdMember where house_id=(select house_id from HouseholdMember where memberid= @memberid) ",
                conn);

            cmd.Parameters.AddWithValue("@memberid", info.MID);
            Debug.WriteLine($"{info.MID}");
            Debug.WriteLine($"{cmd.CommandText}");


            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                int ordinal_householdid = reader.GetOrdinal("householdid");
                int ordinal_memberid = reader.GetOrdinal("memberid");
                int ordinal_member_no = reader.GetOrdinal("member_no");
                int ordinal_house_id = reader.GetOrdinal("house_id");
                int ordinal_is_head = reader.GetOrdinal("is_head");
                int ordinal_start_date = reader.GetOrdinal("start_date");
                int ordinal_end_date = reader.GetOrdinal("end_date");


                while (await reader.ReadAsync())
                {
                    HouseholdMember Info = new HouseholdMember
                    {

                        HouseholdID = reader.IsDBNull(ordinal_householdid) ? 0 : reader.GetInt32(ordinal_householdid),
                        MemberID = reader.IsDBNull(ordinal_memberid) ? 0 : reader.GetInt32(ordinal_memberid),
                        Member_no = reader.IsDBNull(ordinal_member_no) ? 0 : reader.GetInt32(ordinal_member_no),
                        House_ID = reader.IsDBNull(ordinal_house_id) ? 0 : reader.GetInt32(ordinal_house_id),
                        Is_head = reader.IsDBNull(ordinal_is_head) ? false : reader.GetBoolean(ordinal_is_head),
                        Start_date = reader.IsDBNull(ordinal_start_date) ? null : reader.GetDateTime(ordinal_start_date),
                        End_date = reader.IsDBNull(ordinal_end_date) ? null : reader.GetDateTime(ordinal_end_date),

                    };

                    Infos.Add(Info);
                    Debug.WriteLine($"getHousehold_by_basicinfo: info {JsonSerializer.Serialize(Info)}");
                }
                ;

            }
            Debug.WriteLine($"getHousehold_by_basicinfo: check return back to controller{JsonSerializer.Serialize(Infos)}");

            return Infos;

        }
    }
}
