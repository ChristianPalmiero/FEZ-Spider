using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;

namespace AdminApp
{
    public class Users
    {
        MySqlConnection con;
        MySqlCommand cmd;
        MySqlDataAdapter sda;
        DataSet ds;

        public Users()
        {
            con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MysqlConnection"].ConnectionString);
            //con = new MySqlConnection("server=localhost; user id=Chris; password=christian8; database=sys");
        }

        public DataSet SelectUsers()
        {
            sda = new MySqlDataAdapter(@"SELECT Username, Password, Reg_time, Image_path, Cell_Phone_Number
                                    FROM users", con);
            ds = new DataSet();
            sda.Fill(ds);
            return ds;
        }

        public void InsertUser(UsersBLL bll)
        {
            String query = @"INSERT INTO users
                               (Username, Password, Reg_time, Image_path, Cell_Phone_Number)
                               VALUES (?Username, ?Password, ?Reg_time, ?Image_path, ?Cell_Phone_Number)";
            MySqlCommand comm = con.CreateCommand();
            comm.CommandText = query;
            comm.Parameters.Add("?Username", MySqlDbType.VarChar).Value = bll.Username;
            comm.Parameters.Add("?Password", MySqlDbType.VarChar).Value = bll.Password;
            comm.Parameters.Add("?Reg_time", MySqlDbType.VarChar).Value = DateTime.Now.ToString("dd/MM/yyyy");
            comm.Parameters.Add("?Image_path", MySqlDbType.VarChar).Value = bll.Image_Path;
            comm.Parameters.Add("?Cell_Phone_Number", MySqlDbType.VarChar).Value = bll.Cellphone_Number;
            try
            {
                con.Open();
                comm.ExecuteNonQuery();
                con.Close();
            }
            catch
            { }
        }

        public void UpdateUser(UsersBLL bll)
        {
            String query = "UPDATE users SET Password = @p, Reg_time = @r, Image_path = @ip, Cell_Phone_number = @cp WHERE Username = @u;";
            MySqlCommand comm = con.CreateCommand();
            comm.CommandText = query;
            comm.Parameters.Add("@u", MySqlDbType.VarChar).Value = bll.Username;
            comm.Parameters.Add("@p", MySqlDbType.VarChar).Value = bll.Password;
            comm.Parameters.Add("@r", MySqlDbType.VarChar).Value = DateTime.Now.ToString("dd/MM/yyyy");
            comm.Parameters.Add("@ip", MySqlDbType.VarChar).Value = bll.Image_Path;
            comm.Parameters.Add("@cp", MySqlDbType.VarChar).Value = bll.Cellphone_Number;
            try
            {
                con.Open();
                comm.ExecuteNonQuery();
                con.Close();
            }
            catch { }
        }

        public void DeleteUser(string Username)
        {
            String query = @"DELETE FROM users
                           WHERE Username = ?val";
            MySqlCommand comm = con.CreateCommand();
            comm.CommandText = query;
            comm.Parameters.Add("?Val", MySqlDbType.VarChar).Value = Username;
            try
            {
                con.Open();
                comm.ExecuteNonQuery();
                con.Close();
            }
            catch { }
        }
    }
}