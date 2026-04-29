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
using TempleManagement.Exceptions;

namespace TempleManagement.Models.DBManager
{
    public class DonateType_DBManager
    {
        private readonly string connStr = "Data Source=(localdb)\\MSSQLLocalDB;Database=templeManagement;User ID=Jerry;Password=lccJerry1;Trusted_Connection=True";
        private readonly string connectionString_postgresql = "Host=localhost;Port=5432;Username=postgres;Password=2026fafafa;Database=templemanagement";

        /*------------------------------------*/
        //目前改用 postgresql，但某些MSSQL函式不會刪。
        //原由:想用mssql express ，因為tcp連接，然而，我電腦無法啟用。


        /*---------------------------------------------------------------------------------------*/
        /*
         Create: create_donatetype()
         Read: get_donatetype()
         Update: update_donatetype()
         Delete: delete_donatetype()
         特殊函式: modify_donatetype()
        */


        /*
            DB : donation_household
         */
        

        // 順序很重要，在create時，會mapping
        private static string[] column_array = { "dh_id", "houseid", "is_dipper", "is_taisui", "is_peacelight", "dipper_big", "dipper_small", "note" };
        private static string column = string.Join(",", column_array);

        // donation_household
        private static string column_for_create = string.Join(",", column_array.Skip(1));
        private static string column_for_create_value = "@" + string.Join(",  @", column_array.Skip(1));

        /*------------------------------------*/


        /*
            DB : donatetype
         */

        // 順序很重要，在create時，會mapping
        private static string[] column_array_donatetype = { "id", "name", "name_chinese", "price","prototype", "needdipper","category", "note", "modifydate" };
        private static string column_donatetype = string.Join(",", column_array_donatetype);

        // donatetype
        private static string column_for_create_donatetype = string.Join(",", column_array_donatetype.Skip(1));
        private static string column_for_create_value_donatetype = "@" + string.Join(",  @", column_array_donatetype.Skip(1));



        // 取得donatetype資料表
        public async Task<List<DonateType>> get_donatetype()
        {
            using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            NpgsqlCommand cmd;
            List<DonateType> Infos = new List<DonateType>();



            cmd = new NpgsqlCommand(
                $"SELECT * FROM donatetype JOIN donatetype_prototype ON donatetype.prototype=donatetype_prototype.id JOIN DonateType_Category ON DonateType_Category.id=donatetype.category",
                conn);


            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                int ordinal_id = reader.GetOrdinal("id");
                int ordinal_name = reader.GetOrdinal("name");
                int ordinal_name_chinese = reader.GetOrdinal("name_chinese");
                int ordinal_price = reader.GetOrdinal("price");
                int ordinal_prototype = reader.GetOrdinal("prototype");
                int ordinal_prototype_name = reader.GetOrdinal("prototype_name");
                int ordinal_category = reader.GetOrdinal("category");
                int ordinal_category_name = reader.GetOrdinal("category_name");
                int ordinal_note = reader.GetOrdinal("note");
                int ordinal_needdipper = reader.GetOrdinal("needdipper");
                int ordinal_modifydate = reader.GetOrdinal("modifydate");


                while (await reader.ReadAsync())
                {
                    DonateType Info = new DonateType
                    {

                        ID = reader.IsDBNull(ordinal_id) ? 0 : reader.GetInt32(ordinal_id),
                        Name = reader.IsDBNull(ordinal_name) ? "" : reader.GetString(ordinal_name),
                        Name_chinese = reader.IsDBNull(ordinal_name_chinese) ? "" : reader.GetString(ordinal_name_chinese),
                        Price = reader.IsDBNull(ordinal_price) ? 0 : reader.GetInt32(ordinal_price),
                        Prototype = reader.IsDBNull(ordinal_prototype) ? 0 : reader.GetInt32(ordinal_prototype),
                        NeedDipper = reader.IsDBNull(ordinal_needdipper) ? false : reader.GetBoolean(ordinal_needdipper),
                        Prototype_name = reader.IsDBNull(ordinal_prototype_name) ? "" : reader.GetString(ordinal_prototype_name),
                        Note = reader.IsDBNull(ordinal_note) ? "" : reader.GetString(ordinal_note),
                        ModifyDate = reader.IsDBNull(ordinal_modifydate) ? null : reader.GetDateTime(ordinal_modifydate),
                        Category = reader.IsDBNull(ordinal_category) ? 0 : reader.GetInt32(ordinal_category),
                        Category_name = reader.IsDBNull(ordinal_category_name) ? "" : reader.GetString(ordinal_category_name),

                    };

                    Infos.Add(Info);

                }
                ;

            }
            Debug.WriteLine($"check return back to controller: get_donatetype => {JsonSerializer.Serialize(Infos)}");

            return Infos;

        }



        // 決定把新增、修改、刪除 包成一起
        // update: 頁面上 id 在 DB
        // insert: 頁面上 id 不在 DB
        // delete: DB id 不在 頁面上
        public async Task modify_donatetype(List<DonateType> info)
        {
   
            if (info.Any(x => x.Name_chinese == null))
            {
                throw new NeedNameChineseException("防呆! 避免無類別名稱Name_chinese");
            }
            if (info.Any(x => x.Price == null))
            {
                throw new NeedPriceException("防呆! 避免無類別名稱Price");
            }



            DonateType_DBManager dBManager = new DonateType_DBManager();
            Dictionary<int, DonateType> dict_info = info.ToDictionary(x => x.ID);
            List<int> not_to_insert = new List<int>();

            // 取得DB資料
            List<DonateType> donateTypes = await dBManager.get_donatetype();

            // 進行比對 foreach
            foreach (var donateType in donateTypes) {
                int DB_id = donateType.ID;
                not_to_insert.Add(DB_id);

                if (dict_info.TryGetValue(DB_id, out var data))
                {
                    if (data.Name_chinese == "刪除" && data.Price == 111 )
                    {
                        // delete
                        Debug.WriteLine("刪已存在的資料，要刪除的");
                        await dBManager.delete_donatetype(donateType); //刪已存在的資料，要刪除的
                    }
                    else 
                    {
                        // id == DB.id update
                        donateType.Price = data.Price;
                        donateType.Note = data.Note;
                        donateType.Name = data.Name;
                        donateType.Name_chinese = data.Name_chinese;
                        donateType.Prototype = data.Prototype; // 這與Category其實不能讓管理者update

                        await dBManager.update_donatetype(donateType);
                    }
                        
                }
                
                    
            }

            // DB.id迴圈結束 id不存在DB insert
            foreach (var d in info)
            {
                if (! not_to_insert.Contains(d.ID))
                {
                    if (d.Name_chinese == "刪除" && d.Price == 111) //刪新增，又刪除的
                    {
                        // delete
                        Debug.WriteLine("刪新增，又刪除的");
                        await dBManager.delete_donatetype(d);
                    }
                    else
                    {
                        await dBManager.create_donatetype(d);
                    }
                        
                }
            }

            Debug.WriteLine("modify_donatetype done!!!");

        }

        // insert: 頁面上 id 不在 DB
        public async Task create_donatetype(DonateType user)
        {
            Debug.WriteLine("start insert");
            await using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@$"INSERT INTO donatetype(name, name_chinese, price, note, prototype, needdipper, category) VALUES(@name, @name_chinese, @price, @note, @prototype, @needdipper, @category)", conn);


            cmd.Parameters.AddWithValue("@name", user.Name == null ? DBNull.Value : user.Name);
            cmd.Parameters.AddWithValue("@name_chinese", user.Name_chinese);
            cmd.Parameters.AddWithValue("@price", user.Price);
            cmd.Parameters.AddWithValue("@prototype", user.Prototype);
            cmd.Parameters.AddWithValue("@note", user.Note == null ? DBNull.Value : user.Note);
            cmd.Parameters.AddWithValue("@needdipper", user.NeedDipper);
            cmd.Parameters.AddWithValue("@category", user.Category);

            Debug.WriteLine("完成insert");

            await cmd.ExecuteNonQueryAsync();

        }

        // update: 頁面上 id 在 DB
        public async Task update_donatetype(DonateType user)
        {
            Debug.WriteLine("start update");
            await using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@$"UPDATE donatetype SET  name_chinese=@name_chinese, price=@price,  note=@note where id = @id", conn);

            cmd.Parameters.AddWithValue("@id", user.ID);
            cmd.Parameters.AddWithValue("@name_chinese", user.Name_chinese);
            cmd.Parameters.AddWithValue("@price", user.Price);
            //cmd.Parameters.AddWithValue("@prototype", user.Prototype);
            cmd.Parameters.AddWithValue("@note", user.Note == null ? DBNull.Value : user.Note);


            Debug.WriteLine("完成update");

            await cmd.ExecuteNonQueryAsync();

        }

        // delete: DB id 不在 頁面上
        public async Task delete_donatetype(DonateType user)
        {
            Debug.WriteLine("start delete");
            await using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@$"DELETE FROM donatetype where id = @id", conn);

            cmd.Parameters.AddWithValue("@id", user.ID);


            Debug.WriteLine("完成delete");

            await cmd.ExecuteNonQueryAsync();

        }

        /*
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
        */
    }
}
