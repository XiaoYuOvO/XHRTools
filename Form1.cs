using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using CsvHelper.Configuration;

namespace XHRTools
{
    public partial class Form1 : Form
    {
        private readonly List<User> UserList = new List<User>();
        public Form1()
        {
            InitializeComponent();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown1.Visible = radioButton2.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.CheckFileExists = true;
            openFileDialog.DefaultExt = ".csv";
            switch (openFileDialog.ShowDialog())
            {
                case DialogResult.None:
                    break;
                case DialogResult.Cancel:
                    break;
                case DialogResult.Abort:
                    break;
                case DialogResult.Retry:
                    break;
                case DialogResult.Ignore:
                    break;
                case DialogResult.OK:
                case DialogResult.Yes:
                    LoadCsvUserList(openFileDialog.OpenFile());
                    break;
                case DialogResult.No:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LoadCsvUserList(Stream stream)
        {
            var readerConfiguration = new CsvConfiguration(CultureInfo.CurrentUICulture)
            {
                HasHeaderRecord = false
            };
            var csvReader = new CsvHelper.CsvReader(new StreamReader(stream),readerConfiguration);
            UserList.AddRange(csvReader.GetRecords<User>());
            this.label2.Text = (UserList.Count).ToString() + "个账号";
        }
    }
}