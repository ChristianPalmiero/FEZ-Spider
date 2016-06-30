using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace AdminApp
{
    public class UsersBLL
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Image_Path { get; set; }
        public string Cellphone_Number { get; set; }
        public string Reg_Time { get; set; }

        public DataSet Select()
        {
            Users u = new Users();
            return u.SelectUsers();
        }

        public void Insert(UsersBLL bll){
            Users u = new Users();
            u.InsertUser(bll);
        }

        public void Update(UsersBLL bll)
        {
            Users u = new Users();
            u.UpdateUser(bll);
        }

        public void Delete(string username)
        {
            Users u = new Users();
            u.DeleteUser(username);
        }
    }
}