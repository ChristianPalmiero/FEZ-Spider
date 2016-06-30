using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ServerPack
{
    public class Database
    {
        private MySqlConnection con = null;

        // Constructor
        public Database(string connectionString)
        {
            // "server=localhost; user id=Chris; password=christian8; database=sys;"
            con = new MySqlConnection(connectionString);
            //con = new MySqlConnection("server=" + server + "; user id=" + user + "; password=" + password + "; database=" + db + ";");
        }

        // Check if the credentials (username + password) are valid
        public bool CheckPassword(string nameAndPassword){
            try
            {
                // Open the connection
                con.Open();
                Console.WriteLine("You are connected to the db!");
            }
            catch (Exception er)
            {
                Console.WriteLine("Connection error: " + er.Message);
                return false;
            }
            try
            {
                // username@password splitted into username and password
                string[] tokens = nameAndPassword.Split('@');
                string name = null;
                string password = null;

                // Sql query that retrieves the password associated to a well defined username
                string query = @"SELECT Username, Password FROM users WHERE username = ?User";
                MySqlCommand comm = con.CreateCommand();
                comm.CommandText = query;
                comm.Parameters.Add("?User", MySqlDbType.VarChar).Value = tokens[0];
                // Use a prepared statement
                comm.Prepare();
                comm.ExecuteNonQuery();
                MySqlDataReader reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    name = reader.GetString(0);
                    password = reader.GetString(1);
                    if (password == null || name == null)
                    {
                        Console.WriteLine("Entry not found in db");
                        con.Close();
                        reader.Close();
                        return false;
                    }
                }
                reader.Close();
                // If the retrieved password is equal to the input password, OK
                if (password.Equals(tokens[1]))
                {
                    Console.WriteLine("Match");
                    con.Close();
                    return true;
                }
                // Otherwise, there is an error
                else
                {
                    Console.WriteLine("Non match");
                    con.Close();
                    return false;
                }
            }
            catch (Exception er)
            {
                Console.WriteLine("Error: " + er.Message);
                con.Close();
                return false;
            }
        }

        public string RetrieveImage(string username)
        {
            try
            {
                // Open the connection
                con.Open();
                Console.WriteLine("You are connected to the db!");
            }
            catch (Exception er)
            {
                Console.WriteLine("Connection error: " + er.Message);
                return null;
            }
            try
            {
                string image = null;
                // Sql query that retrieves the image path associated to a well defined username
                string query = @"SELECT Image_path FROM users WHERE username = ?User";
                MySqlCommand comm = con.CreateCommand();
                comm.CommandText = query;
                comm.Parameters.Add("?User", MySqlDbType.VarChar).Value = username;
                // Use a prepare statement
                comm.Prepare();
                comm.ExecuteNonQuery();
                MySqlDataReader reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    image = reader.GetString(0);
                }
                reader.Close();
                con.Close();
                return image;
            }
            catch (Exception er)
            {
                Console.WriteLine("Error: " + er.Message);
                con.Close();
                return null;
            }
        }

        public string RetrieveCellPhone(string username)
        {
            try
            {
                // Open the connection
                con.Open();
                Console.WriteLine("You are connected to the db!");
            }
            catch (Exception er)
            {
                Console.WriteLine("Connection error: " + er.Message);
                return null;
            }
            try
            {
                string image = null;
                // Sql query that retrieves the cell phone number associated to a well defined username
                string query = @"SELECT Cell_Phone_number FROM users WHERE username = ?User";
                MySqlCommand comm = con.CreateCommand();
                comm.CommandText = query;
                comm.Parameters.Add("?User", MySqlDbType.VarChar).Value = username;
                // Use a prepare statement
                comm.Prepare();
                comm.ExecuteNonQuery();
                MySqlDataReader reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    image = reader.GetString(0);
                }
                reader.Close();
                con.Close();
                return image;
            }
            catch (Exception er)
            {
                Console.WriteLine("Error: " + er.Message);
                con.Close();
                return null;
            }
        }
    }
}