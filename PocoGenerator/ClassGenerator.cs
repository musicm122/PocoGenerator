using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoGenerator
{
    public static class ClassGenerator
    {
        private static readonly Dictionary<Type, string> TypeAliases = new Dictionary<Type, string> {
        { typeof(int), "int" },
        { typeof(short), "short" },
        { typeof(byte), "byte" },
        { typeof(byte[]), "byte[]" },
        { typeof(long), "long" },
        { typeof(double), "double" },
        { typeof(decimal), "decimal" },
        { typeof(float), "float" },
        { typeof(bool), "bool" },
        { typeof(string), "string" }
    };

        private static readonly HashSet<Type> NullableTypes = new HashSet<Type> {
        typeof(int),
        typeof(short),
        typeof(long),
        typeof(double),
        typeof(decimal),
        typeof(float),
        typeof(bool),
        typeof(DateTime)
    };

        public static string DumpCSharpClass(this IDbConnection connection, string sql, string className = "")
        {

            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            var reader = cmd.ExecuteReader();

            var builder = new StringBuilder();
            do
            {
                if (reader.FieldCount <= 1) continue;

                className = String.IsNullOrWhiteSpace(className) ? "Info" : className;
                builder.AppendLine("public class " + className);

                builder.AppendLine("{");
                var schema = reader.GetSchemaTable();

                foreach (DataRow row in schema.Rows)
                {
                    var type = (Type)row["DataType"];
                    var name = TypeAliases.ContainsKey(type) ? TypeAliases[type] : type.Name;
                    var isNullable = (bool)row["AllowDBNull"] && NullableTypes.Contains(type);
                    var collumnName = (string)row["ColumnName"];

                    builder.AppendLine(string.Format("\tpublic {0}{1} {2} {{ get; set; }}", name, isNullable ? "?" : string.Empty, collumnName));
                }

                builder.AppendLine("}");
                builder.AppendLine();
            } while (reader.NextResult());

            return builder.ToString();
        }

        public static string DumpVbClass(this IDbConnection connection, string sql, string className = "")
        {

            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            var reader = cmd.ExecuteReader();

            var builder = new StringBuilder();
            do
            {
                if (reader.FieldCount <= 1) continue;

                className = String.IsNullOrWhiteSpace(className) ? "Info" : className;
                builder.AppendLine("Public Class " + className);

                var schema = reader.GetSchemaTable();

                foreach (DataRow row in schema.Rows)
                {
                    var type = (Type)row["DataType"];
                    var name = TypeAliases.ContainsKey(type) ? TypeAliases[type] : type.Name;

                    name = name.Trim() == "int" ? "Integer" : name;

                    var firstLetter = name.Substring(0, 1).ToUpper();
                    name = name.Remove(0, 1);
                    name = firstLetter + name;
                    var isNullable = (bool)row["AllowDBNull"] && NullableTypes.Contains(type);
                    var collumnName = (string)row["ColumnName"];

                    builder.AppendLine(string.Format("\tPublic Property {0} As {1}{2}", collumnName, name, isNullable ? "?" : string.Empty));
                }

                builder.AppendLine("End Class");
                builder.AppendLine();
            } while (reader.NextResult());

            return builder.ToString();
        }
    }
}
