using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MYSQLApp
{
    public partial class InsertPanel : Form
    {
        ControlPanel c;

        public InsertPanel(ControlPanel c)
        {
            this.c = c;
            InitializeComponent();
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string pattern = "^[+][1-9][1-9][0-9]{9,13}$";

            if (textBox1.Text.Equals("")){
                MessageBox.Show("Invalid Username");
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
            }
            else if (textBox2.Text.Equals(""))
            {
                MessageBox.Show("Invalid Password");
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
            }
            else if (!(System.Text.RegularExpressions.Regex.IsMatch(textBox4.Text, pattern)))
            {
                MessageBox.Show("Invalid Cell Phone Number");
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
            }
            else if (!(textBox3.Text.EndsWith(".jpg") || textBox3.Text.EndsWith(".gif") || textBox3.Text.EndsWith(".bmp") || textBox3.Text.EndsWith(".jpeg")))
            {
                MessageBox.Show("The selected file is not an image");
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
            }
            else if (!(System.IO.File.Exists(textBox3.Text)))
            {
                MessageBox.Show("Invalid Img path");
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
            }
            else {
                String query = @"INSERT INTO users
                               (Username, Password, Reg_time, Image_path, Cell_Phone_Number)
                               VALUES (?Username, ?Password, ?Reg_time, ?Image_path, ?Cell_Phone_Number)";
                MySqlCommand comm = c.con.CreateCommand();
                comm.CommandText = query;
                comm.Parameters.Add("?Username", MySqlDbType.VarChar).Value = textBox1.Text;
                comm.Parameters.Add("?Password", MySqlDbType.VarChar).Value = textBox2.Text;
                comm.Parameters.Add("?Reg_time", MySqlDbType.VarChar).Value = DateTime.Now.ToString("dd/MM/yyyy");
                comm.Parameters.Add("?Image_path", MySqlDbType.VarChar).Value = textBox3.Text;
                comm.Parameters.Add("?Cell_Phone_Number", MySqlDbType.VarChar).Value = textBox4.Text;
                comm.ExecuteNonQuery();
                this.Close();
                c.update();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 6;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            textBox3.Text = openFileDialog1.FileName;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
