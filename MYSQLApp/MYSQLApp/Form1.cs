using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MYSQLApp
{
    public partial class ControlPanel : Form
    {
        public MySqlConnection con { get; set; }
        private MySqlCommand cmd;
        private MySqlDataAdapter sda;
        private MySqlCommandBuilder scb;
        private DataTable dt;

        public ControlPanel()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            con = new MySqlConnection("server=localhost; user id=Chris; password=christian8; database=sys");
            // Define the connection
            try
            {
                // Open the connection
                con.Open();
                MessageBox.Show("You are connected!");
            }
            catch (Exception er)
            {   // Show an error message
                MessageBox.Show("Connection error: " + er.Message);
            }
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
 
        }

        // Remove an existing user
        private void button3_Click(object sender, EventArgs e)
        {
            if (con != null)
            {
                try
                {
                    RemovePanel rmp = new RemovePanel(this);
                    rmp.Show();
                }
                catch (Exception er)
                {
                    MessageBox.Show("Error: " + er.Message);
                }
            }
            else
                MessageBox.Show("You are not connected yet");
        }

        // Insert a new user
        private void button2_Click(object sender, EventArgs e)
        {
            if (con != null)
            {
                try
                {
                    InsertPanel frm = new InsertPanel(this);
                    frm.Show();
                }
                catch (Exception er)
                {
                    MessageBox.Show("Error: " + er.Message);
                }
            }
            else
                MessageBox.Show("You are not connected yet");
        }

        // Load the users table with hidden passwords
        private void button4_Click(object sender, EventArgs e)
        {
            update();
        }

        public void update()
        {
            if (con != null)
            {
                try
                {
                    sda = new MySqlDataAdapter(@"SELECT Username, Password, Reg_time, Image_path, Cell_Phone_Number
                                    FROM users", con);
                    dt = new DataTable();
                    sda.Fill(dt);
                    //Show passwords with a passwordchar = '*'
                    int columnNumber = 1;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i][columnNumber] = new String('*', dt.Rows[i][columnNumber].ToString().Length);
                    }
                    Table.DataSource = dt;
                }
                catch (Exception er)
                {
                    MessageBox.Show("Error: " + er.Message);
                }
            }
            else
                MessageBox.Show("You are not connected yet");
        }

        // Load the users table with visible passwords
        private void button5_Click(object sender, EventArgs e)
        {
            if (con != null)
            {
                try
                {
                    sda = new MySqlDataAdapter(@"SELECT Username, Password, Reg_time, Image_path, Cell_Phone_Number
                                    FROM users", con);
                    dt = new DataTable();
                    sda.Fill(dt);
                    Table.DataSource = dt;
                    button4.Text = "Unshow Passwords";
                }
                catch (Exception er)
                {
                    MessageBox.Show("Error: " + er.Message);
                }
            }
            else
                MessageBox.Show("You are not connected yet");
        }

        // See the log file in the browser
        private void button6_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://localhost:8080/test/");
            //System.Diagnostics.Process.Start("www.google.com");
        }
    }
}