using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PocoGenerator
{
    public partial class Form1 : Form
    {

        public bool HasGoodTestConnection { get; set; } = false;

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

            var dialogResult = folderBrowserDialog1.ShowDialog();

            var isOk = (dialogResult.ToString().ToLower() == "ok");
            if (!isOk)
            {
                textBox2.Text += "Invalid directory\r\n";
            }
            outputLocationTxt.Text = folderBrowserDialog1.SelectedPath;
            WriteLineMessageToDebugLog("Selected Directory = " + outputLocationTxt.Text);
        }

        private void GenerateTableClass(string tableName, string outputLocation, ProgrammingLanguage lang)
        {
            var result = string.Empty;
            WriteLineMessageToDebugLog("Executing GenerateTableClass");
            using (var con = new SqlConnection(this.conStringTxtBox.Text))
            {
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
            }
        }

        private void ExecutePocoGen()
        {
            var selectedLang = selectedLanguageComboBox.SelectedItem.ToString() == "C#"
                ? ProgrammingLanguage.CSharp
                : ProgrammingLanguage.VbNet;

            WriteLineMessageToDebugLog("Fetching Table Names");
            //var tableNames = Dal.GetTableNames(dbName: this.databaseComboBox.SelectedText, connectionString: conStringTxtBox.Text);

            WriteLineMessageToDebugLog(tableListView.CheckedItems.Count + " Tables Found.");

            foreach (ListViewItem tableName in tableListView.CheckedItems)
            {
                GenerateTableClass(tableName.Text, folderBrowserDialog1.SelectedPath, selectedLang);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            WriteLineMessageToDebugLog("Starting Code Generation");
            try
            {
                WriteLineMessageToDebugLog("Fetching Table Names");

                var tableNames = new List<string>();
                foreach (var item in tableListView.CheckedItems)
                {
                    tableNames.Add(item.ToString());
                }

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

        void TestConnection()
        {
            WriteLineMessageToDebugLog("Testing Connection");
            var result = Dal.TestConnection(conStringTxtBox.Text);
            HasGoodTestConnection = result.Item2;
            WriteLineMessageToDebugLog("Test Connection Result:" + result.Item1);

        }
        private void DbConnectButton_Click(object sender, EventArgs e)
        {
            TestConnection();
            if (HasGoodTestConnection)
            {
                PopulateDbFields();
            }
        }

        void PopulateDbFields()
        {
            var dbNames = Dal.GetDatabaseNames(connectionString: conStringTxtBox.Text).ToArray();
            this.databaseComboBox.Items.Clear();
            this.databaseComboBox.Items.AddRange(dbNames);
        }

        void PopulateAvailableTables()
        {
            var tableNames = Dal.GetTableNames(databaseComboBox.Text, connectionString: conStringTxtBox.Text);
            var items = tableNames.Select(x => new ListViewItem() { Name = x, Text = x }).ToArray<ListViewItem>();
            tableListView.Items.Clear();
            tableListView.Items.AddRange(items);
        }

        private void databaseComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (databaseComboBox.SelectedIndex != -1)
            {
                PopulateAvailableTables();
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            conStringTxtBox.Text = Properties.Settings.Default.ConnectionString;
            outputLocationTxt.Text = Properties.Settings.Default.OutputFolder;
            folderBrowserDialog1.SelectedPath = Properties.Settings.Default.OutputFolder;
            selectedLanguageComboBox.SelectedItem = Properties.Settings.Default.SelectedLanguage;
            if (!String.IsNullOrWhiteSpace(conStringTxtBox.Text))
            {
                TestConnection();
                if (HasGoodTestConnection)
                {
                    PopulateDbFields();
                }
            }
            LinkLabel.Link link = new LinkLabel.Link();
            link.LinkData = "http://www.hackerferret.com/";
            linkLabel1.Links.Add(link);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.ConnectionString = conStringTxtBox.Text;
            Properties.Settings.Default.OutputFolder = outputLocationTxt.Text;
            Properties.Settings.Default.SelectedLanguage = selectedLanguageComboBox.SelectedItem.ToString();
            Properties.Settings.Default.Save();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData as string);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "Select location you wish to output classes to.";

            var dialogResult = folderBrowserDialog1.ShowDialog();

            var isOk = (dialogResult.ToString().ToLower() == "ok");
            if (!isOk)
            {
                textBox2.Text += "Invalid directory\r\n";
            }
            outputLocationTxt.Text = folderBrowserDialog1.SelectedPath;
            WriteLineMessageToDebugLog("Selected Directory = " + outputLocationTxt.Text);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            WriteLineMessageToDebugLog("Starting Code Generation");
            try
            {
                WriteLineMessageToDebugLog("Fetching Table Names");

                var tableNames = new List<string>();
                foreach (var item in tableListView.SelectedItems)
                {
                    tableNames.Add(item.ToString());
                }

                WriteLineMessageToDebugLog(tableNames.Count() + " Tables Found.");
                ExecutePocoGen();
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

        private void tableListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tableListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            tableListView.ListViewItemSorter = new ListViewItemComparer(e.Column);
        }

        class ListViewItemComparer : IComparer
        {
            private int col = 0;

            public ListViewItemComparer(int column)
            {
                col = column;
            }
            public int Compare(object x, object y)
            {
                return String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
            }
        }

        private void conStringTxtBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}