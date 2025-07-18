using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CozyComfortClient.UserServiceReference;

namespace CozyComfortClient.Account
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
           
            lblErrorMessage.Text = "";
            lblError.Text = "";
            errorRow.Visible = false;
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                try
                {
                    UserServiceSoapClient client = new UserServiceSoapClient();
                    string result = client.Login(txtEmail.Text.Trim(), txtPassword.Text.Trim());

                    if (result == "invalid")
                    {
                        ShowErrorMessage("Invalid email or password");
                        return;
                    }

                    string[] parts = result.Split(':');
                    if (parts.Length == 2)
                    {
                        string userType = parts[0];
                        string userId = parts[1];

                        Session["UserType"] = userType;
                        Session["UserId"] = userId;

                        switch (userType)
                        {
                            case "m":
                                Response.Redirect("~/Manufacturer/AddNewProduct.aspx", false);
                                break;
                            case "d":
                                Response.Redirect("~/Distributor/DSOrder.aspx", false);
                                break;
                            case "s":
                                Response.Redirect("~/Seller/UOrder.aspx", false);
                                break;
                            default:
                                ShowErrorMessage("Unknown user type.");
                                break;
                        }
                    }
                    else
                    {
                        ShowErrorMessage("Unexpected login response: " + result);
                    }
                }
                catch (Exception ex)
                {
                    ShowErrorMessage("System error: " + ex.Message);
                }
            }
        }


        private void ShowErrorMessage(string message)
        {
            lblError.Text = message;
            errorRow.Visible = true;

            txtPassword.Text = "";

            txtEmail.Focus();
        }
    }
}