using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using NuGet.Protocol;
using System;
using System.Collections.Generic;
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
        // 順序很重要，在create時，會mapping
        private static string[] column_array = { "MID", "Name", "Sex", "Zodiac", "Age", "Home_num", "Phone", "Job", "Character_type", "Lunar_birthday", "Birthday", "Identical_num", "Household_address", "Current_address", "Postal_code_cur", "Postal_code_household", "Note" };
        private static string column = string.Join(",", column_array);

        // newBasicInfo
        private static string column_for_create = string.Join(",", column_array.Skip(1));
        private static string column_for_create_value = "@" + string.Join(",@", column_array.Skip(1));

        /*---------------------------------------------------------------------------------------*/
        /*
         Create: newBasicInfo()
         Read: getBasicInfo()、
         Update:
         Delete:
        */

        // 取得BasicInfo 資料表 資料 
        public List<BasicInfo> getBasicInfo()
        {
            List<BasicInfo> Infos = new List<BasicInfo>();

            SqlConnection sqlConnection = new SqlConnection(connStr);
            sqlConnection.Open();

            SqlCommand sqlCommand = new SqlCommand($"SELECT {column}   FROM BasicInfo");

            sqlCommand.Connection = sqlConnection;

            SqlDataReader reader = sqlCommand.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    BasicInfo Info = new BasicInfo
                    {

                        MID = reader.GetInt32(reader.GetOrdinal("MID")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Sex = reader.GetBoolean(reader.GetOrdinal("Sex")),
                        Zodiac = reader.GetString(reader.GetOrdinal("Zodiac")),
                        Age = reader.GetInt32(reader.GetOrdinal("Age")),
                        Home_num = reader.GetString(reader.GetOrdinal("Home_num")),
                        Phone = reader.GetString(reader.GetOrdinal("Phone")),
                        Job = reader.GetString(reader.GetOrdinal("Job")),
                        Character_type = reader.GetString(reader.GetOrdinal("Character_type")),
                        Lunar_birthday = reader.IsDBNull(reader.GetOrdinal("Lunar_birthday")) ? null: reader.GetDateTime(reader.GetOrdinal("Lunar_birthday")),
                        Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday")) ? null: reader.GetDateTime(reader.GetOrdinal("Birthday")),
                        ID_num = reader.GetString(reader.GetOrdinal("Identical_num")),
                        Household_address = reader.GetString(reader.GetOrdinal("Household_address")),
                        Current_address = reader.GetString(reader.GetOrdinal("Current_address")),
                        Postal_code_cur = reader.GetString(reader.GetOrdinal("Postal_code_cur")),
                        Postal_code_household = reader.GetString(reader.GetOrdinal("Postal_code_household")),
                        Note = reader.GetString(reader.GetOrdinal("Note"))

                    };
                    Infos.Add(Info);
                }
            }
            else
            {
                Console.WriteLine("BasicInfo資料表為空！");
            }
            sqlConnection.Close();
            return Infos;
        }

        // 寫入 BasicInfo 資料表
       
        public void newBasicInfo(BasicInfo user)
        {

            SqlConnection sqlconnection = new SqlConnection(connStr);
            SqlCommand sqlcommand = new SqlCommand(@$"INSERT INTO BasicInfo({column_for_create}) VALUES$({column_for_create_value})");
            sqlcommand.Connection = sqlconnection;

            sqlcommand.Parameters.Add(new SqlParameter("@Name", user.Name));
            sqlcommand.Parameters.Add(new SqlParameter("@Sex", user.Sex));
            sqlcommand.Parameters.Add(new SqlParameter("@Zodiac", user.Zodiac));
            sqlcommand.Parameters.Add(new SqlParameter("@Age", user.Age));
            sqlcommand.Parameters.Add(new SqlParameter("@Home_num", user.Home_num));
            sqlcommand.Parameters.Add(new SqlParameter("@Phone", user.Phone));
            sqlcommand.Parameters.Add(new SqlParameter("@Job", user.Job));
            sqlcommand.Parameters.Add(new SqlParameter("@Character_type", user.Character_type));
            sqlcommand.Parameters.Add(new SqlParameter("@Lunar_birthday", user.Lunar_birthday));
            sqlcommand.Parameters.Add(new SqlParameter("@Birthday", user.Birthday));
            sqlcommand.Parameters.Add(new SqlParameter("@ID_num", user.ID_num));
            sqlcommand.Parameters.Add(new SqlParameter("@Household_address", user.Household_address));
            sqlcommand.Parameters.Add(new SqlParameter("@Current_address", user.Current_address));
            sqlcommand.Parameters.Add(new SqlParameter("@Postal_code_cur", user.Postal_code_cur));
            sqlcommand.Parameters.Add(new SqlParameter("@Postal_code_household", user.Postal_code_household));
            sqlcommand.Parameters.Add(new SqlParameter("@Note", user.Note));

            sqlconnection.Open();
            sqlcommand.ExecuteNonQuery();
            sqlconnection.Close();
        }
    }
}
