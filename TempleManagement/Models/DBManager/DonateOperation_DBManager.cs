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
        private static string[] column_array_donatetype = { "id", "name", "name_chinese", "price","prototype", "note", "modifydate" };
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
                int ordinal_blessinglight = reader.GetOrdinal("blessinglight");

                int ordinal_note = reader.GetOrdinal("note");


                while (await reader.ReadAsync())
                {
                    DonateIndividual Info = new DonateIndividual
                    {

                        DI_ID = reader.IsDBNull(ordinal_di_id) ? 0 : reader.GetInt32(ordinal_di_id),
                        MID = reader.IsDBNull(ordinal_mid) ? 0 : reader.GetInt32(ordinal_mid),
                        Blessinglight = reader.IsDBNull(ordinal_blessinglight) ? "" : reader.GetString(ordinal_blessinglight),

                        Note = reader.IsDBNull(ordinal_note) ? null : reader.GetString(ordinal_note),

                    };

                    Infos.Add(Info);

                }
                ;

            }
            Debug.WriteLine($"check return back to controller: get_donation_individual => {JsonSerializer.Serialize(Infos)}");

            return Infos;

        }

        // create donation_individual
        public async Task create_donation_individual(DonateIndividual user)
        {
            Debug.WriteLine("start insert");
            await using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@$"INSERT INTO donation_individual(mid, blessinglight, note) VALUES(@mid, @blessinglight, @note)", conn);

            // 抓取最新MID ，也就是剛建好的
            BasicInfo_DBManager basicInfo_DBManager = new BasicInfo_DBManager();
            List<BasicInfo> basicInfo = await basicInfo_DBManager.getBasicInfo(latest_MID : true);

            Debug.WriteLine("檢查create_donation_individual的basicinfo");
            Debug.WriteLine(JsonSerializer.Serialize(basicInfo));

            cmd.Parameters.AddWithValue("@mid", basicInfo[0].MID == null ? DBNull.Value : basicInfo[0].MID);
            cmd.Parameters.AddWithValue("@blessinglight", user.Blessinglight == null ? DBNull.Value : user.Blessinglight);
            cmd.Parameters.AddWithValue("@note", user.Note == null ? DBNull.Value : user.Note);


            Debug.WriteLine("完成insert");

            await cmd.ExecuteNonQueryAsync();

        }

        /*---------------------------------------------------------------------------------------*/

        /*
         Create: create_donation_household()
         Read: get_donation_household()
         Update: 
         Delete: 
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

        // create donation_household
        public async Task create_donation_household(DonateHousehold user)
        {
            Debug.WriteLine("start insert create_donation_household");
            await using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@$"INSERT INTO donation_household(houseid, is_dipper, is_taisui, is_peacelight, dipper_big, dipper_small, note) VALUES(@houseid, @is_dipper, @is_taisui, @is_peacelight, @dipper_big, @dipper_small, @note)", conn);


            cmd.Parameters.AddWithValue("@houseid", user.HouseID == null ? DBNull.Value : user.HouseID);
            cmd.Parameters.AddWithValue("@is_dipper", user.Is_dipper == null ? DBNull.Value : user.Is_dipper);
            cmd.Parameters.AddWithValue("@is_taisui", user.Is_taisui == null ? DBNull.Value : user.Is_taisui);
            cmd.Parameters.AddWithValue("@is_peacelight", user.Is_peacelight == null ? DBNull.Value : user.Is_peacelight);
            cmd.Parameters.AddWithValue("@dipper_big", user.Dipper_big == null ? DBNull.Value : user.Dipper_big);
            cmd.Parameters.AddWithValue("@dipper_small", user.Dipper_small == null ? DBNull.Value : user.Dipper_small);
            cmd.Parameters.AddWithValue("@note", user.Note == null ? DBNull.Value : user.Note);

            Debug.WriteLine("完成insert create_donation_household");

            await cmd.ExecuteNonQueryAsync();

        }


        
    }
}
