using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace PocoGenerator
{
    public static class Dal
    {

        public static System.Tuple<string, bool> TestConnection(string connectionString)
        {
            try
            {
                var con = new SqlConnection(connectionString);
                con.Open();
                con.Close();
            }
            catch (SqlException ex)
            {
                return new Tuple<string, bool>("Sql Connection Error:" + ex.Message, false);
            }
            catch (Exception ex)
            {
                return new Tuple<string, bool>("Error:" + ex.Message, false);
            }
            return new Tuple<string, bool>("Connected Successfully", true);
        }

        public static List<string> GetTableNames(string dbName, string connectionString = "")
        {
            //string schema = "dbo"
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = ConfigurationManager.ConnectionStrings["Db"].ConnectionString;
            }

            var retval = new List<string>();

            var sql = @"SELECT 
                            [t0].[TABLE_NAME]
                        FROM [INFORMATION_SCHEMA].[TABLES] AS [t0] 
                        WHERE 
                            ([t0].[TABLE_CATALOG] = @dbName )";

            //AND ([t0].[TABLE_TYPE] = 'BASE TABLE' )";
            //AND ([t0].[TABLE_SCHEMA] = @schema) ";
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@dbName", dbName);
                var dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        retval.Add(dr["TABLE_NAME"].ToString());
                    }
                }
            }
            return retval;
        }

        public static List<string> GetDatabaseNames(string schema = "dbo", string connectionString = "")
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = ConfigurationManager.ConnectionStrings["Db"].ConnectionString;
            }

            var retval = new List<string>();

            var sql = @"select name from sys.databases";
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                var cmd = new SqlCommand(sql, con);
                var dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        retval.Add(dr["name"].ToString());
                    }
                }
            }

            return retval;
        }


    }
}