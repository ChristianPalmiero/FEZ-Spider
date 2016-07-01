using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AdminApp
{
    public partial class Database : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Fill();
            }
        }

        // Fill the GridView
        private void Fill()
        {
            UsersBLL u = new UsersBLL();
            GridView1.DataSource = u.Select();
            GridView1.DataBind();
        }

        // Insert a new user
        protected void Button1_Click(object sender, EventArgs e)
        {
            UsersBLL u = new UsersBLL();
            u.Username = ((TextBox)GridView1.FooterRow.FindControl("TextBox1")).Text;
            u.Password = ((TextBox)GridView1.FooterRow.FindControl("TextBox2")).Text;
            u.Image_Path = ((TextBox)GridView1.FooterRow.FindControl("TextBox3")).Text;
            u.Cellphone_Number = ((TextBox)GridView1.FooterRow.FindControl("TextBox4")).Text;
            u.Reg_Time = DateTime.Now.ToString("dd/MM/yyyy");

            // Do some check
            if (check(((TextBox)GridView1.FooterRow.FindControl("TextBox1")).Text,
                ((TextBox)GridView1.FooterRow.FindControl("TextBox2")).Text,
                ((TextBox)GridView1.FooterRow.FindControl("TextBox3")).Text,
                ((TextBox)GridView1.FooterRow.FindControl("TextBox4")).Text))
            {
                u.Insert(u);
                Fill();
            }
        }

        // Edit an existing user
        protected void GridView1_RowEditing(object sender, GridViewEditEventArgs e)
        {
            GridView1.EditIndex = e.NewEditIndex;
            Fill();
        }

        // Update the GridView
        protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            UsersBLL u = new UsersBLL();
            u.Username = ((Label)GridView1.Rows[e.RowIndex].FindControl("Label1")).Text;
            u.Password = ((TextBox)GridView1.Rows[e.RowIndex].FindControl("TextBox6")).Text;
            u.Image_Path = ((TextBox)GridView1.Rows[e.RowIndex].FindControl("TextBox7")).Text;
            u.Cellphone_Number = ((TextBox)GridView1.Rows[e.RowIndex].FindControl("TextBox8")).Text;
            if (check(((Label)GridView1.Rows[e.RowIndex].FindControl("Label1")).Text,
                    ((TextBox)GridView1.Rows[e.RowIndex].FindControl("TextBox6")).Text,
                    ((TextBox)GridView1.Rows[e.RowIndex].FindControl("TextBox7")).Text,
                    ((TextBox)GridView1.Rows[e.RowIndex].FindControl("TextBox8")).Text))
            {
                u.Update(u);
                u = null;
                GridView1.EditIndex = -1;
                Fill();
            }
        }

        protected bool check(string one, string two, string three, string four)
        {
            try
            {
                // one digit between 1-9, one digit between 1-9, one digit between 0-9, 9 to 13 digits
                string pattern = "^[+][1-9][1-9][0-9]{9,13}$";

                if (one.Equals(""))
                {
                    ScriptManager.RegisterStartupScript(this, typeof(Page), "MsgUpdate",
                                                                      "alert('Empty username');", true);
                    return false;
                }
                else if (!Regex.IsMatch(one, @"^[a-zA-Z0-9]+$"))
                {
                    ScriptManager.RegisterStartupScript(this, typeof(Page), "MsgUpdate",
                                                      "alert('Only letters and numbers are allowed');", true);
                    return false;
                }
                else if (two.Equals(""))
                {
                    ScriptManager.RegisterStartupScript(this, typeof(Page), "MsgUpdate",
                                                                      "alert('Invalid password');", true);
                    return false;
                }
                else if (Path.GetFileNameWithoutExtension(three).Equals(""))
                {
                    ScriptManager.RegisterStartupScript(this, typeof(Page), "MsgUpdate",
                                                                   "alert('Not an image');", true);
                    return false;
                }
                else if (!(three.EndsWith(".jpg") ||
                    three.EndsWith(".gif") ||
                    three.EndsWith(".bmp") ||
                    three.EndsWith(".jpeg")))
                {
                    ScriptManager.RegisterStartupScript(this, typeof(Page), "MsgUpdate",
                                                                   "alert('Not an image');", true);
                    return false;
                }
                else if (!(System.Text.RegularExpressions.Regex.IsMatch(
                    four, pattern)))
                {
                    ScriptManager.RegisterStartupScript(this, typeof(Page), "MsgUpdate",
                                                                      "alert('Invalid cellphone number');", true);
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Cancel button handler
        protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridView1.EditIndex = -1;
            Fill();
        }

        // Delete a user
        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            UsersBLL u = new UsersBLL();
            u.Username = ((Label)GridView1.Rows[e.RowIndex].FindControl("Label1")).Text;
            u.Delete(u.Username);
            u = null;
            GridView1.EditIndex = -1;
            Fill();
        }

        // Show a new page
        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            Fill();
        }
    }
}