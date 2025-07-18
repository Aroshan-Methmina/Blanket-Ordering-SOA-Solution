using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using CozyComfortClient.DistributorServiceReference;
using CozyComfortClient.UserServiceReference;

namespace CozyComfortClient.Manufacturer
{
    public partial class AddNewDistributor : System.Web.UI.Page
    {
        private readonly UserServiceSoapClient userService = new UserServiceSoapClient();
        private readonly DistributorServiceSoapClient distributorService = new DistributorServiceSoapClient();

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
            
            BindDistributors();
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

        private void BindDistributors()
        {
            try
            {
                var distributors = distributorService.GetAllDistributors();
                DataTable dt = ConvertToDataTable(distributors);
                gvDistributors.DataSource = dt;
                gvDistributors.DataBind();
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading distributors: " + ex.Message, false);
            }
        }

        private DataTable ConvertToDataTable(DistributorDetails[] distributors)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("did", typeof(int));
            dt.Columns.Add("business_name", typeof(string));
            dt.Columns.Add("distributor_contact", typeof(string));
            dt.Columns.Add("warehouse_location", typeof(string));
            dt.Columns.Add("license_number", typeof(string));
            dt.Columns.Add("appuser_id", typeof(int)); 

            foreach (var distributor in distributors)
            {
                dt.Rows.Add(
                    distributor.DistributorId,
                    distributor.BusinessName,
                    distributor.ContactNumber,
                    distributor.WarehouseLocation,
                    distributor.LicenseNumber,
                    GetAppUserId(distributor.DistributorId)
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
                    if (hdnDistributorID.Value == "0") 
                    {
                      
                        var userDetails = new UserDetails
                        {
                            CompanyName = txtBusinessName.Text.Trim(),
                            ContactNumber = txtContact.Text.Trim(),
                            Address = txtWarehouse.Text.Trim(),
                            LicenseNumber = txtLicense.Text.Trim()
                        };

                        
                        string result = userService.Register(
                            txtEmail.Text.Trim(),
                            txtPassword.Text.Trim(),
                            'd', 
                            userDetails
                        );

                        if (result == "success")
                        {
                            ShowMessage("Distributor added successfully!", true);
                            BindDistributors();
                            ResetForm();
                        }
                        else
                        {
                            ShowMessage("Error: " + result, false);
                        }
                    }
                    else 
                    {
                        int distributorId = Convert.ToInt32(hdnDistributorID.Value);

                       
                        var userDetails = new UserDetails
                        {
                            UserId = GetAppUserId(distributorId),
                            Email = txtEmail.Text.Trim()
                        };

                  
                        string updateResult = distributorService.UpdateDistributorProfile(
                            distributorId,
                            txtBusinessName.Text.Trim(),
                            txtContact.Text.Trim(),
                            txtWarehouse.Text.Trim(),
                            txtLicense.Text.Trim()
                        );

                        if (updateResult == "success")
                        {
                            ShowMessage("Distributor updated successfully!", true);
                            BindDistributors();
                            ResetForm();
                            hdnDistributorID.Value = "0";
                            btnCancel.Visible = false;
                            formTitle.InnerText = "Add New Distributor";
                            btnSubmit.Text = "Add Distributor";
                        }
                        else
                        {
                            ShowMessage("Error: " + updateResult, false);
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
            var userDetails = userService.GetUserDetails(appuserId, 'd');
            return userDetails?.Email ?? string.Empty;
        }



        private int GetAppUserId(int distributorId)
        {
            try
            {
                var distributor = distributorService.GetDistributorDetails(distributorId);
                var userDetails = userService.GetUserDetails(distributorId, 'd');
                return userDetails.UserId;
            }
            catch
            {
                return -1;
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ResetForm();
            hdnDistributorID.Value = "0";
            btnCancel.Visible = false;
            formTitle.InnerText = "Add New Distributor";
            btnSubmit.Text = "Add Distributor";
        }

        private void ResetForm()
        {
            txtEmail.Text = "";
            txtPassword.Text = "";
            txtBusinessName.Text = "";
            txtContact.Text = "";
            txtWarehouse.Text = "";
            txtLicense.Text = "";
        }

        protected void gvDistributors_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvDistributors.EditIndex = e.NewEditIndex;
            BindDistributors();
        }

        protected void gvDistributors_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvDistributors.EditIndex = -1;
            BindDistributors();
        }

        protected void gvDistributors_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int distributorId = Convert.ToInt32(gvDistributors.DataKeys[e.RowIndex].Value);

                GridViewRow row = gvDistributors.Rows[e.RowIndex];

                string businessName = ((TextBox)row.FindControl("txtBusinessNameEdit")).Text;
                string contactNumber = ((TextBox)row.FindControl("txtContactEdit")).Text;
                string warehouseLocation = ((TextBox)row.FindControl("txtWarehouseEdit")).Text;
                string licenseNumber = ((TextBox)row.FindControl("txtLicenseEdit")).Text;

                string result = distributorService.UpdateDistributorProfile(
                    distributorId,
                    businessName,
                    contactNumber,
                    warehouseLocation,
                    licenseNumber
                );

                if (result == "success")
                {
                    ShowMessage("Distributor updated successfully!", true);
                    gvDistributors.EditIndex = -1;
                    BindDistributors();
                }
                else
                {
                    ShowMessage("Error: " + result, false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error updating distributor: " + ex.Message, false);
            }
        }

        protected void gvDistributors_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int distributorId = Convert.ToInt32(gvDistributors.DataKeys[e.RowIndex].Value);
                int appUserId = GetAppUserId(distributorId);

                var distributor = distributorService.GetDistributorDetails(distributorId);

                ShowMessage("Distributor deleted successfully!", true);
                BindDistributors();
            }
            catch (Exception ex)
            {
                ShowMessage("Error deleting distributor: " + ex.Message, false);
            }
        }

        protected void gvDistributors_SelectedIndexChanging(object sender, GridViewSelectEventArgs e)
        {
            int distributorId = Convert.ToInt32(gvDistributors.DataKeys[e.NewSelectedIndex].Value);
            hdnDistributorID.Value = distributorId.ToString();

            var distributor = distributorService.GetDistributorDetails(distributorId);
            if (distributor != null)
            {
                txtBusinessName.Text = distributor.BusinessName;
                txtContact.Text = distributor.ContactNumber;
                txtWarehouse.Text = distributor.WarehouseLocation;
                txtLicense.Text = distributor.LicenseNumber;
                txtEmail.Text = distributor.Email;


                txtPassword.Attributes["value"] = "";
                txtPassword.Text = ""; 

                formTitle.InnerText = "Update Distributor";
                btnSubmit.Text = "Update Distributor";
                btnCancel.Visible = true;
            }
        }

        protected void gvDistributors_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvDistributors.PageIndex = e.NewPageIndex;
            BindDistributors();
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            lblMessage.Text = message;
            lblMessage.Visible = true;
            lblMessage.CssClass = isSuccess ? "text-success" : "text-danger";
        }
    }
}