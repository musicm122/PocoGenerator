using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace PocoGenerator
{
    public static class Dal
    {
        public static List<string> GetTableNames(string dbName, string schema = "dbo")
        {
            var retval = new List<string>();

            var sql = @"SELECT [t0].[TABLE_NAME]
                        FROM [INFORMATION_SCHEMA].[TABLES] AS [t0] 
                        WHERE ([t0].[TABLE_CATALOG] = @dbName ) AND ([t0].[TABLE_TYPE] = 'BASE TABLE' ) AND ([t0].[TABLE_SCHEMA] = @schema) ";
            var con = new SqlConnection(ConfigurationManager.ConnectionStrings["Db"].ConnectionString);
            con.Open();
            var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@dbName", dbName);
            cmd.Parameters.AddWithValue("@schema", schema);
            var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    retval.Add(dr["TABLE_NAME"].ToString());
                }
            }
            return retval;
        }
    }
}