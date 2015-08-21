using System;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PocoGenerator
{
    public partial class Form1 : Form
    {
        public enum ProgrammingLanguage
        {
            CSharp = 0,
            VbNet = 1
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void WriteMessageToDebugLog(string message)
        {
            textBox2.Text += "------------------------------------------\r\n";
            textBox2.Text += message;
            textBox2.Text += "------------------------------------------\r\n";
        }

        private void WriteLineMessageToDebugLog(string message)
        {
            textBox2.Text += "------------------------------------------\r\n";
            textBox2.Text += message + Environment.NewLine;
            textBox2.Text += "------------------------------------------\r\n";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "Select location you wish to output classes to.";
            //saveFileDialog1.AddExtension = true;
            //saveFileDialog1.CheckPathExists = true;
            //saveFileDialog1.CheckPathExists = true;
            //saveFileDialog1.DefaultExt = "vb";
            //saveFileDialog1.Filter = "|*.vb";

            var dialogResult = folderBrowserDialog1.ShowDialog();

            var isOk = (dialogResult.ToString().ToLower() == "ok");
            if (!isOk)
            {
                textBox2.Text += "Invalid directory\r\n";
            }
            textBox1.Text = folderBrowserDialog1.SelectedPath;
            WriteLineMessageToDebugLog("Selected Directory = " + textBox1.Text);
        }

        private void GenerateTableClass(string tableName, string outputLocation, ProgrammingLanguage lang)
        {
            var result = string.Empty;
            WriteLineMessageToDebugLog("Executing GenerateTableClass");
            //sb.AppendLine(Connection.DumpVbClass("select * from " + table.TABLE_NAME, table.TABLE_NAME, true));
            var con = new SqlConnection(ConfigurationManager.ConnectionStrings["Db"].ConnectionString);
            con.Open();
            switch (lang)
            {
                case ProgrammingLanguage.CSharp:
                    result = con.DumpCSharpClass("select * from " + tableName, tableName);
                    File.AppendAllText(outputLocation + "\\" + tableName + ".cs", result);
                    WriteLineMessageToDebugLog("Created class " + tableName + ".cs");
                    break;
                case ProgrammingLanguage.VbNet:
                    result = con.DumpVbClass("select * from " + tableName, tableName);
                    File.AppendAllText(outputLocation + "\\" + tableName + ".vb", result);
                    WriteLineMessageToDebugLog("Created class " + tableName + ".vb");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lang), lang, null);
            }


            con.Close();
        }

        private void ExecutePocoGen()
        {
            var selectedLang = comboBox1.SelectedItem.ToString() == "C#"
                ? ProgrammingLanguage.CSharp
                : ProgrammingLanguage.VbNet;

            WriteLineMessageToDebugLog("Fetching Table Names");
            var tableNames = Dal.GetTableNames("CT4");
            WriteLineMessageToDebugLog(tableNames.Count() + " Tables Found.");
            foreach (var table in tableNames)
            {
                GenerateTableClass(table, folderBrowserDialog1.SelectedPath, selectedLang);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            WriteLineMessageToDebugLog("Starting Code Generation");
            try
            {
                WriteLineMessageToDebugLog("Fetching Table Names");
                var tableNames = Dal.GetTableNames("CT4");
                WriteLineMessageToDebugLog(tableNames.Count() + " Tables Found.");
                ExecutePocoGen();
                //GenerateTableClasses(tableNames, this.folderBrowserDialog1.SelectedPath);
                WriteLineMessageToDebugLog("Code Generation Complete.");
            }
            catch (Exception ex)
            {
                textBox2.Text += "------------------------------------------\r\n";
                textBox2.Text += ex.Message;
                textBox2.Text += ex.StackTrace;
                textBox2.Text += "------------------------------------------\r\n";
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
        }
    }
}