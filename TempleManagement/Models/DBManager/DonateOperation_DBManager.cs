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
        private static string[] column_array = { "dh_id", "houseid", "donatetype_id", "note" };
        private static string column = string.Join(",", column_array);

        // donation_household
        private static string column_for_create = string.Join(",", column_array.Skip(1));
        private static string column_for_create_value = "@" + string.Join(",  @", column_array.Skip(1));

        /*------------------------------------*/


        /*
            DB : donatetype
         */

        // 順序很重要，在create時，會mapping
        private static string[] column_array_donatetype = { "id", "name", "name_chinese", "price","prototype", "note", "modifydate", "needdipper", "category" };
        private static string column_donatetype = string.Join(",", column_array_donatetype);

        // donatetype
        private static string column_for_create_donatetype = string.Join(",", column_array_donatetype.Skip(1));
        private static string column_for_create_value_donatetype = "@" + string.Join(",  @", column_array_donatetype.Skip(1));

        /*------------------------------------*/

        /*
            DB : donation_individual
         */

        // 順序很重要，在create時，會mapping
        private static string[] column_array_donation_individual = { "di_id", "mid", "donatetypeid" };
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
         Update: 
         Delete: 
        */
        // 取得donation_individual資料表
        public async Task<List<DonateIndividual>> get_donation_individual()
        {
            using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            NpgsqlCommand cmd;

            cmd = new NpgsqlCommand(
                """
                SELECT
                    di.di_id,
                    di.mid,
                    di.note AS individual_note,

                    dt.id AS donate_type_id,
                    dt.name,
                    dt.name_chinese,
                    dt.price,
                    dt.note AS donate_type_note,
                    dt.needdipper,

                    dp.id AS prototype_id,
                    dp.prototype_name,

                    dc.id AS category_id,
                    dc.category_name
                FROM donation_individual di
                JOIN donatetype dt
                    ON di.donatetypeid = dt.id
                JOIN donatetype_prototype dp
                    ON dt.prototype = dp.id
                JOIN donatetype_category dc
                    ON dc.id = dt.category;

                """,
                conn);

            // 使用dict 一筆筆蒐集，以MID當作key
            var donateDict = new Dictionary<int, DonateIndividual>();

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                int ordinal_di_id = reader.GetOrdinal("di_id");
                int ordinal_mid = reader.GetOrdinal("mid");
                int ordinal_prototype_name = reader.GetOrdinal("prototype_name");
                int ordinal_prototype_id = reader.GetOrdinal("prototype_id");
                int ordinal_category_name = reader.GetOrdinal("category_name");
                int ordinal_category_id = reader.GetOrdinal("category_id");
                int ordinal_price = reader.GetOrdinal("price");
                int ordinal_name_chinese = reader.GetOrdinal("name_chinese");
                int ordinal_note = reader.GetOrdinal("donate_type_note");
                int ordinal_donate_type_id = reader.GetOrdinal("donate_type_id");

                
                while (await reader.ReadAsync())
                {
                    int mid = reader.IsDBNull(ordinal_mid) ? 0 : reader.GetInt32(ordinal_mid);

                    if (!donateDict.ContainsKey(mid))
                    {
                        donateDict[mid] = new DonateIndividual
                        {
                            DI_ID = reader.IsDBNull(ordinal_di_id) ? 0 : reader.GetInt32(ordinal_di_id),

                            MID = mid,

                            Note = reader.IsDBNull(ordinal_note) ? null : reader.GetString(ordinal_note),

                            DonateItem_idv = new List<DonationItem>()
                        };
                    }

                    donateDict[mid].DonateItem_idv.Add(
                        new DonationItem
                        {
                            DonateTypeId = reader.IsDBNull(ordinal_donate_type_id) ? 0 : reader.GetInt32(ordinal_donate_type_id),

                            Name_chinese = reader.IsDBNull(ordinal_name_chinese) ? null : reader.GetString(ordinal_name_chinese),

                            Prototype_name = reader.IsDBNull(ordinal_prototype_name) ? null : reader.GetString(ordinal_prototype_name),

                            Prototype = reader.IsDBNull(ordinal_prototype_id) ? 0 : reader.GetInt32(ordinal_prototype_id),

                            Category_name= reader.IsDBNull(ordinal_category_name) ? null : reader.GetString(ordinal_category_name),

                            Category = reader.IsDBNull(ordinal_category_id) ? 0 : reader.GetInt32(ordinal_category_id),

                            SelectedPrice = reader.IsDBNull(ordinal_price) ? 0 : reader.GetInt32(ordinal_price)
                        }
                    );
                }
            }
            var Infos = donateDict.Values.ToList();

            Debug.WriteLine($"check return back to controller: get_donation_individual => {JsonSerializer.Serialize(Infos)}");

            return Infos;

        }

        // create donation_individual
        // 設計成一次一筆DonateType，而非一起insert
        public async Task create_donation_individual(DonateIndividual user)
        {
            Debug.WriteLine("start insert");
            await using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@$"INSERT INTO donation_individual(mid, donatetypeid, note) VALUES(@mid, @donatetypeid, @note)", conn);

            // 抓取最新MID ，也就是剛建好的
            BasicInfo_DBManager basicInfo_DBManager = new BasicInfo_DBManager();
            List<BasicInfo> basicInfo = await basicInfo_DBManager.getBasicInfo(latest_MID : true);

            Debug.WriteLine("檢查create_donation_individual的basicinfo");
            Debug.WriteLine(JsonSerializer.Serialize(basicInfo));

            cmd.Parameters.AddWithValue("@mid", basicInfo[0].MID == null ? DBNull.Value : basicInfo[0].MID);
            // 每次呼叫此函式，只會傳送一筆 donatetypeid
            cmd.Parameters.AddWithValue("@donatetypeid", user.DonateItem_idv.First().DonateTypeId == null ? DBNull.Value : user.DonateItem_idv.First().DonateTypeId);
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

            cmd = new NpgsqlCommand(
                """
                SELECT
                    dh.dh_id,
                    dh.houseid,

                    dt.id AS donate_type_id,
                    dt.name,
                    dt.name_chinese,
                    dt.price,
                    dt.note AS donate_type_note,
                    dt.needdipper,

                    dp.id AS prototype_id,
                    dp.prototype_name,

                    dc.id AS category_id,
                    dc.category_name
                FROM donation_household dh
                JOIN donatetype dt
                    ON dh.donatetype_id = dt.id
                JOIN donatetype_prototype dp
                    ON dt.prototype = dp.id
                JOIN donatetype_category dc
                    ON dc.id = dt.category;

                """,
                conn);

            // 使用dict 一筆筆蒐集，以houseid當作key
            var donateDict = new Dictionary<int, DonateHousehold>();

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                int ordinal_dh_id = reader.GetOrdinal("dh_id");
                int ordinal_houseid = reader.GetOrdinal("houseid");
                int ordinal_prototype_name = reader.GetOrdinal("prototype_name");
                int ordinal_prototype_id = reader.GetOrdinal("prototype_id");
                int ordinal_category_name = reader.GetOrdinal("category_name");
                int ordinal_category_id = reader.GetOrdinal("category_id");
                int ordinal_price = reader.GetOrdinal("price");
                int ordinal_name_chinese = reader.GetOrdinal("name_chinese");
                int ordinal_note = reader.GetOrdinal("donate_type_note");
                int ordinal_donate_type_id = reader.GetOrdinal("donate_type_id");


                while (await reader.ReadAsync())
                {
                    int houseid = reader.IsDBNull(ordinal_houseid) ? 0 : reader.GetInt32(ordinal_houseid);

                    if (!donateDict.ContainsKey(houseid))
                    {
                        donateDict[houseid] = new DonateHousehold
                        {
                            DH_ID = reader.IsDBNull(ordinal_dh_id) ? 0 : reader.GetInt32(ordinal_dh_id),

                            HouseID = reader.IsDBNull(ordinal_houseid) ? 0 : reader.GetInt32(ordinal_houseid),

                            Note = reader.IsDBNull(ordinal_note) ? null : reader.GetString(ordinal_note),

                            DonateItem_idv = new List<DonationItem>()
                        };
                    }

                    donateDict[houseid].DonateItem_idv.Add(
                        new DonationItem
                        {
                            DonateTypeId = reader.IsDBNull(ordinal_donate_type_id) ? 0 : reader.GetInt32(ordinal_donate_type_id),

                            Name_chinese = reader.IsDBNull(ordinal_name_chinese) ? null : reader.GetString(ordinal_name_chinese),

                            Prototype_name = reader.IsDBNull(ordinal_prototype_name) ? null : reader.GetString(ordinal_prototype_name),

                            Prototype = reader.IsDBNull(ordinal_prototype_id) ? 0 : reader.GetInt32(ordinal_prototype_id),

                            Category_name = reader.IsDBNull(ordinal_category_name) ? null : reader.GetString(ordinal_category_name),

                            Category = reader.IsDBNull(ordinal_category_id) ? 0 : reader.GetInt32(ordinal_category_id),

                            SelectedPrice = reader.IsDBNull(ordinal_price) ? 0 : reader.GetInt32(ordinal_price)
                        }
                    );

                }
            }
            var Infos = donateDict.Values.ToList();
            Debug.WriteLine($"check return back to controller: get_donation_household => {JsonSerializer.Serialize(Infos)}");

            return Infos;

        }

        // create donation_household
        public async Task create_donation_household(DonateHousehold user)
        {
            Debug.WriteLine("start insert create_donation_household");
            await using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@$"INSERT INTO donation_household(houseid, donatetype_id, note) VALUES(@houseid, @donatetype_id, @note)", conn);


            cmd.Parameters.AddWithValue("@houseid", user.HouseID == null ? DBNull.Value : user.HouseID);
            cmd.Parameters.AddWithValue("@donatetype_id", user.DonateItem_idv.First().DonateTypeId == null ? DBNull.Value : user.DonateItem_idv.First().DonateTypeId);
            cmd.Parameters.AddWithValue("@note", user.Note == null ? DBNull.Value : user.Note);

            Debug.WriteLine("完成insert create_donation_household");

            await cmd.ExecuteNonQueryAsync();

        }


        
    }
}
