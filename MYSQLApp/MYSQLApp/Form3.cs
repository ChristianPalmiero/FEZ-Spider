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
    public partial class RemovePanel : Form
    {
        ControlPanel c;

        public RemovePanel(ControlPanel c)
        {
            this.c = c;
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            String query = @"DELETE FROM users
                           WHERE Username = ?val";
            MySqlCommand comm = c.con.CreateCommand();
            comm.CommandText = query;
            comm.Parameters.Add("?Val", MySqlDbType.VarChar).Value = comboBox1.Text;
            comm.ExecuteNonQuery();
            this.Close();
            c.update();
        }

        private void comboBox1_AdjustWidthComboBox(object sender, EventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            int width = senderComboBox.DropDownWidth;
            Graphics g = senderComboBox.CreateGraphics();
            Font font = senderComboBox.Font;
            int vertScrollBarWidth =
                (senderComboBox.Items.Count > senderComboBox.MaxDropDownItems)
                ? SystemInformation.VerticalScrollBarWidth : 0;

            int newWidth;
            foreach (var item in ((ComboBox)sender).Items)
            {
                string s = ((DataRowView)item)["Username"].ToString();
                newWidth = (int)g.MeasureString(s, font).Width
                    + vertScrollBarWidth;
                if (width < newWidth)
                {
                    width = newWidth;
                }
            }
            senderComboBox.DropDownWidth = width;
            string query = "SELECT Username FROM users";
            MySqlCommand command = new MySqlCommand(query, c.con);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                senderComboBox.Items.Add(reader.GetString(0));
            }
            reader.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
