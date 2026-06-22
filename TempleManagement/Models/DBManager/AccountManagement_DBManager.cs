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
    public class AccountManagement_DBManager
    {
        private readonly string connStr = "Data Source=(localdb)\\MSSQLLocalDB;Database=templeManagement;User ID=Jerry;Password=lccJerry1;Trusted_Connection=True";
        private readonly string connectionString_postgresql = "Host=localhost;Port=5432;Username=postgres;Password=2026fafafa;Database=templemanagement";

        /*------------------------------------*/
        //目前改用 postgresql，但某些MSSQL函式不會刪。
        //原由:想用mssql express ，因為tcp連接，然而，我電腦無法啟用。


        /*---------------------------------------------------------------------------------------*/
        /*
         Create: newAccount()
         Read: getAccount()、
         Update: 
         Delete:
        */

        // 順序很重要，在create時，會mapping
        private static string[] column_array = { "adminid", "account" , "password", "name"};
        private static string column = string.Join(",", column_array);

        // getAccount
        private static string column_for_create = string.Join(",", column_array.Skip(1));
        private static string column_for_create_value = "@" + string.Join(",  @", column_array.Skip(1));

        // 取得admin 資料表 資料 
        public async Task<List<Admin>> getAccount(Admin info)
        {
            using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();
            List<Admin> Infos = new List<Admin>();

            NpgsqlCommand cmd;
            Admin Info = new Admin();



            cmd = new NpgsqlCommand(
                $"SELECT * FROM admin where account= @account and password=@password",
                conn);

            cmd.Parameters.AddWithValue("@account", info.Account);
            cmd.Parameters.AddWithValue("@password", info.Password);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                int ordinal_AdminID = reader.GetOrdinal("adminid");
                int ordinal_Name = reader.GetOrdinal("name");
                int ordinal_Password = reader.GetOrdinal("password");
                int ordinal_Account = reader.GetOrdinal("account");


                while (await reader.ReadAsync())
                {
                    Info = new Admin
                    {

                        AdminID = reader.IsDBNull(ordinal_AdminID) ? 0 : reader.GetInt32(ordinal_AdminID),
                        Name = reader.IsDBNull(ordinal_Name) ? "" : reader.GetString(ordinal_Name),
                        Account = reader.IsDBNull(ordinal_Account) ? "" : reader.GetString(ordinal_Account),
                        Password = reader.IsDBNull(ordinal_Password) ? "" : reader.GetString(ordinal_Password),

                    };
                    Infos.Add(Info);
                    
                };
                
            }
            Debug.WriteLine($"check return back to contro{JsonSerializer.Serialize(Infos)}");

            return Infos;

        }


        public async Task newAccount(Admin user)
        {
            Debug.WriteLine("start insert");
            await using var conn = new NpgsqlConnection(connectionString_postgresql);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@$"INSERT INTO admin({column_for_create}) VALUES({column_for_create_value})", conn);


            cmd.Parameters.AddWithValue("@name", user.Name);
            cmd.Parameters.AddWithValue("@account", user.Account);
            cmd.Parameters.AddWithValue("@password", user.Password);



            Debug.WriteLine("完成insert");

            await cmd.ExecuteNonQueryAsync();


        }



    }
}
