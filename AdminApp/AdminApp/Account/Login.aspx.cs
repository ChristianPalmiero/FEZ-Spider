using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;

namespace AdminApp.Account
{
    public partial class Login : Page
    {
        private MySqlConnection con = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Admin"] != null)
            {
                Response.Redirect("~/Database.aspx");
            }
        }

        protected void Button_Click(object sender, EventArgs e)
        {
            string username = ((TextBox)Login1.FindControl("UserName")).Text;
            string password = ((TextBox)Login1.FindControl("Password")).Text;
            con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MysqlConnection"].ConnectionString);

            if (!username.Equals("Admin"))
            {
                ScriptManager.RegisterStartupScript(this, typeof(Page), "MsgUpdate",
                                                               "alert('Wrong administrator username');", true);
                return;
            }

            try
            {
                // Open the connection
                con.Open();
            }
            catch (Exception er)
            {
                ScriptManager.RegisterStartupScript(this, typeof(Page), "MsgUpdate",
                                                               "alert('Connection NOT valid');", true);
                return;
            }
            try
            {
                string passwordDb = null;

                // Sql query that retrieves the password associated to a well defined username
                string query = @"SELECT Username, Password FROM users WHERE username = ?User";
                MySqlCommand comm = con.CreateCommand();
                comm.CommandText = query;
                comm.Parameters.Add("?User", MySqlDbType.VarChar).Value = "Admin";
                // Use a prepared statement
                comm.Prepare();
                comm.ExecuteNonQuery();
                MySqlDataReader reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    passwordDb = reader.GetString(1);
                }
                reader.Close();
                // If the retrieved password is equal to the input password, OK
                if (passwordDb.Equals(password))
                {
                    con.Close();
                    Session["Admin"] = username;
                    Response.Redirect("~/Database.aspx");
                }
                // Otherwise, there is an error
                else
                {
                    con.Close();
                    ScriptManager.RegisterStartupScript(this, typeof(Page), "MsgUpdate",
                                                                   "alert('Incorrect password');", true);
                    return;
                }
            }
            catch (Exception er)
            {
                con.Close();
                ScriptManager.RegisterStartupScript(this, typeof(Page), "MsgUpdate",
                                                               "alert('" + er.Message + "');", true);
                return;
            }
        }
    }
}