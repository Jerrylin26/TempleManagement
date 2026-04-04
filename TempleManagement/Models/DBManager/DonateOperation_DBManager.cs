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
    public class DonateOperation_DBManager
    {
        private readonly string connStr = "Data Source=(localdb)\\MSSQLLocalDB;Database=templeManagement;User ID=Jerry;Password=lccJerry1;Trusted_Connection=True";
        private readonly string connectionString_postgresql = "Host=localhost;Port=5432;Username=postgres;Password=2026fafafa;Database=templemanagement";

        /*------------------------------------*/
        //目前改用 postgresql，但某些MSSQL函式不會刪。
        //原由:想用mssql express ，因為tcp連接，然而，我電腦無法啟用。


        



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
        private static string[] column_array_donatetype = { "id", "name", "name_chinese", "price", "note", "modifydate" };
        private static string column_donatetype = string.Join(",", column_array_donatetype);

        // donatetype
        private static string column_for_create_donatetype = string.Join(",", column_array_donatetype.Skip(1));
        private static string column_for_create_value_donatetype = "@" + string.Join(",  @", column_array_donatetype.Skip(1));

        /*------------------------------------*/

        /*
            DB : donation_individual
         */

        // 順序很重要，在create時，會mapping
        private static string[] column_array_donation_individual = { "di_id", "mid", "blessinglight600", "blessinglight700", "blessinglight800", "blessinglight1000" };
        private static string column_donation_individual = string.Join(",", column_array_donation_individual);

        // donation_individual
        private static string column_for_create_donation_individual = string.Join(",", column_array_donation_individual.Skip(1));
        private static string column_for_create_value_donation_individual = "@" + string.Join(",  @", column_array_donation_individual.Skip(1));

        /*------------------------------------*/

        /*
            DB : donation_operation
         */

        // 順序很重要，在create時，會mapping
        private static string[] column_array_donation_operation = { "donationid", "mid", "date", "price", "donation_type", "note" };
        private static string column_donation_operation = string.Join(",", column_array_donation_operation);

        // donation_operation
        private static string column_for_create_donation_operation = string.Join(",", column_array_donation_operation.Skip(1));
        private static string column_for_create_value_donation_operation = "@" + string.Join(",  @", column_array_donation_operation.Skip(1));



        /*---------------------------------------------------------------------------------------*/
        /*
         Create: create_donateOperation()
         Read: get_donateOperation()
         Update: update_donateOperation()
         Delete: delete_donateOperation()
         特殊函式: show_record_donateOperation()
        */

        // 取得donation_operation資料表
        public async Task<List<DonateOperation>> get_donateOperation()
        {
            using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            NpgsqlCommand cmd;
            List<DonateOperation> Infos = new List<DonateOperation>();



            cmd = new NpgsqlCommand(
                $"SELECT * FROM donation_operation ",
                conn);


            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                int ordinal_donationid = reader.GetOrdinal("donationid");
                int ordinal_mid = reader.GetOrdinal("mid");
                int ordinal_date = reader.GetOrdinal("date");
                int ordinal_price = reader.GetOrdinal("price");
                int ordinal_donation_type = reader.GetOrdinal("donation_type");
                int ordinal_note = reader.GetOrdinal("note");


                while (await reader.ReadAsync())
                {
                    DonateOperation Info = new DonateOperation
                    {

                        DonationID = reader.IsDBNull(ordinal_donationid) ? 0 : reader.GetInt32(ordinal_donationid),
                        MID = reader.IsDBNull(ordinal_mid) ? 0 : reader.GetInt32(ordinal_mid),
                        Donation_type = reader.IsDBNull(ordinal_donation_type) ? "" : reader.GetString(ordinal_donation_type),
                        Price = reader.IsDBNull(ordinal_price) ? 0 : reader.GetInt32(ordinal_price),
                        Note = reader.IsDBNull(ordinal_note) ? "" : reader.GetString(ordinal_note),
                        Date = reader.IsDBNull(ordinal_date) ? null : reader.GetDateTime(ordinal_date),

                    };

                    Infos.Add(Info);

                }
                ;

            }
            Debug.WriteLine($"check return back to controller: get_donation_operation => {JsonSerializer.Serialize(Infos)}");

            return Infos;

        }
        /*---------------------------------------------------------------------------------------*/

        /*
         Create: create_donation_individual()
         Read: get_donation_individual()
         Update: update_donation_individual()
         Delete: delete_donation_individual()
        */
        // 取得donation_individual資料表
        public async Task<List<DonateIndividual>> get_donation_individual()
        {
            using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            NpgsqlCommand cmd;
            List<DonateIndividual> Infos = new List<DonateIndividual>();



            cmd = new NpgsqlCommand(
                $"SELECT * FROM donation_individual ",
                conn);


            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                int ordinal_di_id = reader.GetOrdinal("di_id");
                int ordinal_mid = reader.GetOrdinal("mid");
                int ordinal_blessinglight600 = reader.GetOrdinal("blessinglight600");
                int ordinal_blessinglight700 = reader.GetOrdinal("blessinglight700");
                int ordinal_blessinglight800 = reader.GetOrdinal("blessinglight800");
                int ordinal_blessinglight1000 = reader.GetOrdinal("blessinglight1000");
                int ordinal_note = reader.GetOrdinal("note");


                while (await reader.ReadAsync())
                {
                    DonateIndividual Info = new DonateIndividual
                    {

                        DI_ID = reader.IsDBNull(ordinal_di_id) ? 0 : reader.GetInt32(ordinal_di_id),
                        MID = reader.IsDBNull(ordinal_mid) ? 0 : reader.GetInt32(ordinal_mid),
                        Blessinglight1000 = reader.IsDBNull(ordinal_blessinglight1000) ? false : reader.GetBoolean(ordinal_blessinglight1000),
                        Blessinglight800 = reader.IsDBNull(ordinal_blessinglight800) ? false : reader.GetBoolean(ordinal_blessinglight800),
                        Blessinglight700 = reader.IsDBNull(ordinal_blessinglight700) ? false : reader.GetBoolean(ordinal_blessinglight700),
                        Blessinglight600 = reader.IsDBNull(ordinal_blessinglight600) ? false : reader.GetBoolean(ordinal_blessinglight600),
                        Note = reader.IsDBNull(ordinal_note) ? null : reader.GetString(ordinal_note),

                    };

                    Infos.Add(Info);

                }
                ;

            }
            Debug.WriteLine($"check return back to controller: get_donation_individual => {JsonSerializer.Serialize(Infos)}");

            return Infos;

        }
        /*---------------------------------------------------------------------------------------*/

        /*
         Create: create_donation_household()
         Read: get_donation_household()
         Update: update_donation_household()
         Delete: delete_donation_household()
        */
        // 取得donation_household資料表
        public async Task<List<DonateHousehold>> get_donation_household()
        {
            using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            NpgsqlCommand cmd;
            List<DonateHousehold> Infos = new List<DonateHousehold>();



            cmd = new NpgsqlCommand(
                $"SELECT * FROM donation_household ",
                conn);


            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                int ordinal_dh_id = reader.GetOrdinal("dh_id");
                int ordinal_houseid = reader.GetOrdinal("houseid");
                int ordinal_is_dipper = reader.GetOrdinal("is_dipper");
                int ordinal_is_taisui = reader.GetOrdinal("is_taisui");
                int ordinal_is_peacelight = reader.GetOrdinal("is_peacelight");
                int ordinal_dipper_big = reader.GetOrdinal("dipper_big");
                int ordinal_dipper_small = reader.GetOrdinal("dipper_small");
                int ordinal_note = reader.GetOrdinal("note");


                while (await reader.ReadAsync())
                {
                    DonateHousehold Info = new DonateHousehold
                    {

                        DH_ID = reader.IsDBNull(ordinal_dh_id) ? 0 : reader.GetInt32(ordinal_dh_id),
                        HouseID = reader.IsDBNull(ordinal_houseid) ? 0 : reader.GetInt32(ordinal_houseid),
                        Is_dipper = reader.IsDBNull(ordinal_is_dipper) ? false : reader.GetBoolean(ordinal_is_dipper),
                        Is_peacelight = reader.IsDBNull(ordinal_is_peacelight) ? false : reader.GetBoolean(ordinal_is_peacelight),
                        Is_taisui = reader.IsDBNull(ordinal_is_taisui) ? false : reader.GetBoolean(ordinal_is_taisui),
                        Dipper_big = reader.IsDBNull(ordinal_dipper_big) ? false : reader.GetBoolean(ordinal_dipper_big),
                        Dipper_small = reader.IsDBNull(ordinal_dipper_small) ? false : reader.GetBoolean(ordinal_dipper_small),
                        Note = reader.IsDBNull(ordinal_note) ? null : reader.GetString(ordinal_note),

                    };

                    Infos.Add(Info);

                }
                ;

            }
            Debug.WriteLine($"check return back to controller: get_donation_household => {JsonSerializer.Serialize(Infos)}");

            return Infos;

        }


        /********************************************************************************************************************************/


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
            foreach (var donateType in donateTypes)
            {
                int DB_id = donateType.ID;
                not_to_insert.Add(DB_id);

                if (dict_info.TryGetValue(DB_id, out var data))
                {
                    if (data.Name_chinese == "刪除" && data.Price == 111)
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

                        await dBManager.update_donatetype(donateType);
                    }

                }


            }

            // DB.id迴圈結束 id不存在DB insert
            foreach (var d in info)
            {
                if (!not_to_insert.Contains(d.ID))
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

            await using var cmd = new NpgsqlCommand(@$"INSERT INTO donatetype(name, name_chinese, price, note) VALUES(@name, @name_chinese, @price, @note)", conn);


            cmd.Parameters.AddWithValue("@name", user.Name == null ? DBNull.Value : user.Name);
            cmd.Parameters.AddWithValue("@name_chinese", user.Name_chinese);
            cmd.Parameters.AddWithValue("@price", user.Price);
            cmd.Parameters.AddWithValue("@note", user.Note == null ? DBNull.Value : user.Note);

            Debug.WriteLine("完成insert");

            await cmd.ExecuteNonQueryAsync();

        }

        // update: 頁面上 id 在 DB
        public async Task update_donatetype(DonateType user)
        {
            Debug.WriteLine("start update");
            await using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@$"UPDATE donatetype SET  name_chinese=@name_chinese, price=@price,  note=@note  where id = @id", conn);

            cmd.Parameters.AddWithValue("@id", user.ID);
            cmd.Parameters.AddWithValue("@name_chinese", user.Name_chinese);
            cmd.Parameters.AddWithValue("@price", user.Price);
            ;
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
