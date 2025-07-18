using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using CozyComfortClient.SellerServiceReference;
using CozyComfortClient.UserServiceReference;

namespace CozyComfortClient.Manufacturer
{
    public partial class AddNewSeller : System.Web.UI.Page
    {
        private readonly UserServiceSoapClient userService = new UserServiceSoapClient();
        private readonly SellerServiceSoapClient sellerService = new SellerServiceSoapClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserType"] == null || Session["UserType"].ToString() != "m")
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            string userId = Session["UserId"]?.ToString();
            if (!string.IsNullOrEmpty(userId))
            {
                var client = new UserServiceReference.UserServiceSoapClient();
                var user = client.GetUserDetails(Convert.ToInt32(userId), 'm');

                if (user != null)
                {
                    lblCompanyName.Text = user.CompanyName;
                }

                if (!IsPostBack)
                {
                    BindSellers();
                }
            }
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {

            Session.Clear();
            Session.Abandon();


            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
            }

            Response.Redirect("~/Account/Login.aspx");
        }
        private void BindSellers()
        {
            try
            {
                var sellers = sellerService.GetAllSellers();
                DataTable dt = ConvertToDataTable(sellers);
                gvSellers.DataSource = dt;
                gvSellers.DataBind();
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading sellers: " + ex.Message, false);
            }
        }

        private DataTable ConvertToDataTable(SellerServiceReference.Seller[] sellers)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("sid", typeof(int));
            dt.Columns.Add("store_name", typeof(string));
            dt.Columns.Add("seller_contact", typeof(string));
            dt.Columns.Add("store_location", typeof(string));
            dt.Columns.Add("website", typeof(string));
            dt.Columns.Add("appuser_id", typeof(int));

            foreach (var seller in sellers)
            {
                dt.Rows.Add(
                    seller.SellerId,
                    seller.StoreName,
                    seller.ContactNumber,
                    seller.StoreLocation,
                    seller.Website,
                    seller.SellerId
                );
            }

            return dt;
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                try
                {
                    if (hdnSellerID.Value == "0") 
                    {
                        var userDetails = new UserDetails
                        {
                            CompanyName = txtStoreName.Text.Trim(),
                            ContactNumber = txtContact.Text.Trim(),
                            Address = txtStoreLocation.Text.Trim(),
                            Website = txtWebsite.Text.Trim()
                        };

                        string result = userService.Register(
                            txtEmail.Text.Trim(),
                            txtPassword.Text.Trim(),
                            's',
                            userDetails
                        );

                        if (result == "success")
                        {
                            ShowMessage("Seller added successfully!", true);
                            BindSellers();
                            ResetForm();
                        }
                        else
                        {
                            ShowMessage("Error: " + result, false);
                        }
                    }
                    else 
                    {
                        int sellerId = Convert.ToInt32(hdnSellerID.Value);

                        var seller = new SellerServiceReference.Seller
                        {
                            SellerId = sellerId,
                            StoreName = txtStoreName.Text.Trim(),
                            ContactNumber = txtContact.Text.Trim(),
                            StoreLocation = txtStoreLocation.Text.Trim(),
                            Website = txtWebsite.Text.Trim()
                        };

                        var updateResult = sellerService.UpdateSellerProfile(seller);

                        if (updateResult.Success)
                        {
                            ShowMessage("Seller updated successfully!", true);
                            BindSellers();
                            ResetForm();
                            hdnSellerID.Value = "0";
                            btnCancel.Visible = false;
                            formTitle.InnerText = "Add New Seller";
                            btnSubmit.Text = "Add Seller";
                        }
                        else
                        {
                            ShowMessage("Error: " + updateResult.Message, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage("Error: " + ex.Message, false);
                }
            }
        }

        public string GetUserEmail(object appuserIdObj)
        {
            if (appuserIdObj == null || appuserIdObj == DBNull.Value)
                return string.Empty;

            int appuserId = Convert.ToInt32(appuserIdObj);
            var userDetails = userService.GetUserDetails(appuserId, 's');
            return userDetails?.Email ?? string.Empty;
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ResetForm();
            hdnSellerID.Value = "0";
            btnCancel.Visible = false;
            formTitle.InnerText = "Add New Seller";
            btnSubmit.Text = "Add Seller";
        }

        private void ResetForm()
        {
            txtEmail.Text = "";
            txtPassword.Text = "";
            txtStoreName.Text = "";
            txtContact.Text = "";
            txtStoreLocation.Text = "";
            txtWebsite.Text = "";
        }

        protected void gvSellers_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvSellers.EditIndex = e.NewEditIndex;
            BindSellers();
        }

        protected void gvSellers_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvSellers.EditIndex = -1;
            BindSellers();
        }

        protected void gvSellers_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int sellerId = Convert.ToInt32(gvSellers.DataKeys[e.RowIndex].Value);

                GridViewRow row = gvSellers.Rows[e.RowIndex];

                var seller = new SellerServiceReference.Seller
                {
                    SellerId = sellerId,
                    StoreName = ((TextBox)row.FindControl("txtStoreNameEdit")).Text,
                    ContactNumber = ((TextBox)row.FindControl("txtContactEdit")).Text,
                    StoreLocation = ((TextBox)row.FindControl("txtStoreLocationEdit")).Text,
                    Website = ((TextBox)row.FindControl("txtWebsiteEdit")).Text
                };

                var result = sellerService.UpdateSellerProfile(seller);

                if (result.Success)
                {
                    ShowMessage("Seller updated successfully!", true);
                    gvSellers.EditIndex = -1;
                    BindSellers();
                }
                else
                {
                    ShowMessage("Error: " + result.Message, false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error updating seller: " + ex.Message, false);
            }
        }

        protected void gvSellers_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int sellerId = Convert.ToInt32(gvSellers.DataKeys[e.RowIndex].Value);

                var result = sellerService.DeleteSeller(sellerId);

                if (result.Success)
                {
                    ShowMessage("Seller deleted successfully!", true);
                    BindSellers();
                }
                else
                {
                    ShowMessage("Error: " + result.Message, false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error deleting seller: " + ex.Message, false);
            }
        }

        protected void gvSellers_SelectedIndexChanging(object sender, GridViewSelectEventArgs e)
        {
            int sellerId = Convert.ToInt32(gvSellers.DataKeys[e.NewSelectedIndex].Value);
            hdnSellerID.Value = sellerId.ToString();

            var seller = sellerService.GetSellerDetails(sellerId);
            if (seller != null && string.IsNullOrEmpty(seller.Error))
            {
                txtStoreName.Text = seller.StoreName;
                txtContact.Text = seller.ContactNumber;
                txtStoreLocation.Text = seller.StoreLocation;
                txtWebsite.Text = seller.Website;
                txtEmail.Text = seller.Email;

                txtPassword.Attributes["value"] = "";
                txtPassword.Text = "";

                formTitle.InnerText = "Update Seller";
                btnSubmit.Text = "Update Seller";
                btnCancel.Visible = true;
            }
        }

        protected void gvSellers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvSellers.PageIndex = e.NewPageIndex;
            BindSellers();
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            lblMessage.Text = message;
            lblMessage.Visible = true;
            lblMessage.CssClass = isSuccess ? "text-success" : "text-danger";
        }
    }
}