using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        // insert: (頁面上 id 不在 DB) && 舊的prototype
        // delete: DB id 不在 頁面上
        // insert_new_prototype: (頁面上 id 不在 DB) && 新的prototype
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
            Dictionary<int, DonateType> dict_info = info.Where(x => x.ID > 0).ToDictionary(x => x.ID); // 拿來update、delete
            var insert_data = info.Where(x => x.ID == 0).ToList();

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
                        donateType.Category = data.Category;
                        donateType.NeedDipper = data.NeedDipper;

                        await dBManager.update_donatetype(donateType);
                    }
                        
                }
                
                    
            }

            // DB.id迴圈結束 id不存在DB 排除錯誤ID
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
                        
                }
            }

            // 新增 insert 判斷(ID==0)
            foreach (var data in insert_data)
            {
                await dBManager.create_donatetype(data);
            }



            Debug.WriteLine("modify_donatetype done!!!");

        }

        // insert: 頁面上 id 不在 DB
        public async Task create_donatetype(DonateType user)
        {
            Debug.WriteLine("start insert");
            await using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            // 因為 新增的類別沒有prototype，使用postgresql組合技
            int prototype_ID;

            await using var cmd_prototype = new NpgsqlCommand(@"
                INSERT INTO donatetype_prototype (prototype_name)
                VALUES (@prototype_name)
                ON CONFLICT (prototype_name)
                DO UPDATE SET prototype_name = EXCLUDED.prototype_name
                RETURNING id;
            ", conn);

            cmd_prototype.Parameters.AddWithValue("prototype_name", user.Prototype_name ?? (object)DBNull.Value);
            var result = await cmd_prototype.ExecuteScalarAsync();
            prototype_ID = Convert.ToInt32(result);


            // 新增進入 donatetype
            await using var cmd = new NpgsqlCommand(@$"INSERT INTO donatetype(name, name_chinese, price, note, prototype, needdipper, category) VALUES(@name, @name_chinese, @price, @note, @prototype, @needdipper, @category)", conn);


            cmd.Parameters.AddWithValue("@name", user.Name == null ? DBNull.Value : user.Name);
            cmd.Parameters.AddWithValue("@name_chinese", user.Name_chinese);
            cmd.Parameters.AddWithValue("@price", user.Price);
            cmd.Parameters.AddWithValue("@prototype", prototype_ID);
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

            await using var cmd = new NpgsqlCommand(@$"UPDATE donatetype SET modifydate=NOW(),  needdipper=@needdipper, name_chinese=@name_chinese, price=@price, category=@category,  note=@note where id = @id", conn);

            cmd.Parameters.AddWithValue("@id", user.ID);
            cmd.Parameters.AddWithValue("@needdipper", user.NeedDipper);
            cmd.Parameters.AddWithValue("@category", user.Category);
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

        
    }
}
