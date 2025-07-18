using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using CozyComfortClient.DOrderServiceReference;
using CozyComfortClient.BlanketServiceReference;
using CozyComfortClient.UserServiceReference;
using CozyComfortClient.DistributorServiceReference;
using System.Web.UI;

namespace CozyComfortClient.Distributor
{
    public partial class DOrder : System.Web.UI.Page
    {
       
        private DOrderServiceSoapClient orderClient = new DOrderServiceSoapClient();
        private BlanketServiceSoapClient blanketService = new BlanketServiceSoapClient();
        private UserServiceSoapClient userService = new UserServiceSoapClient();
        private DistributorServiceSoapClient distributorService = new DistributorServiceSoapClient();

        private int DistributorDbId
        {
            get
            {
                if (Session["DistributorDbId"] != null)
                {
                    return Convert.ToInt32(Session["DistributorDbId"]);
                }
                return 0;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!ValidateUser())
                {
                    Response.Redirect("../Login.aspx");
                    return;
                }

                InitializePage();
            }
        }

        private bool ValidateUser()
        {
            if (Session["UserType"] == null || Session["UserType"].ToString() != "d")
            {
                ShowMessage("Access denied. Please login as distributor.", "error");
                return false;
            }

            if (Session["UserId"] == null)
            {
                ShowMessage("Session expired. Please login again.", "error");
                return false;
            }

            return true;
        }

        private void InitializePage()
        {
            try
            {
                int appUserId = Convert.ToInt32(Session["UserId"]);

           
                int distributorId = distributorService.GetDistributorIdByAppUserId(appUserId);

                if (distributorId <= 0)
                {
                    ShowMessage("Distributor account not found in database.", "error");
                    return;
                }

                Session["DistributorDbId"] = distributorId;

               

                LoadBlankets();
                LoadOrders();
            }
            catch (Exception ex)
            {
                ShowMessage("Initialization error: " + ex.Message, "error");
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

        private void LoadBlankets()
        {
            try
            {
                ddlBlanket.Items.Clear();
                var blankets = blanketService.GetAllBlankets();
                ddlBlanket.DataSource = blankets;
                ddlBlanket.DataBind();
                ddlBlanket.Items.Insert(0, new ListItem("Select Blanket", ""));
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading blankets: " + ex.Message, "error");
            }
        }

        private void LoadManufacturers(int blanketId)
        {
            try
            {
                ddlManufacturer.Items.Clear(); 

                var manufacturers = orderClient.GetManufacturersForBlanket(blanketId);
                ddlManufacturer.DataSource = manufacturers;
                ddlManufacturer.DataBind();
                ddlManufacturer.Enabled = manufacturers.Count() > 0;

                ddlManufacturer.Items.Insert(0, new ListItem("Select Manufacturer", ""));
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading manufacturers: " + ex.Message, "error");
            }
        }


        private void LoadOrders()
        {
            try
            {
                var orders = orderClient.GetDistributorOrdersByDistributor(DistributorDbId, ddlStatusFilter.SelectedValue);
                gvOrders.DataSource = orders;
                gvOrders.DataBind();
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading orders: " + ex.Message, "error");
            }
        }

        protected void ddlBlanket_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlBlanket.SelectedIndex > 0)
            {
                int blanketId = Convert.ToInt32(ddlBlanket.SelectedValue);
                LoadManufacturers(blanketId);
            }
            else
            {
                ddlManufacturer.SelectedIndex = 0;
                ddlManufacturer.Enabled = false;
            }
        }

        protected void btnNewOrder_Click(object sender, EventArgs e)
        {
            pnlOrderForm.Visible = true;
            formTitle.InnerText = "Place New Order";
            hdnOrderID.Value = "0";
            ResetForm();
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                try
                {
                    int blanketId = Convert.ToInt32(ddlBlanket.SelectedValue);
                    int manufacturerId = Convert.ToInt32(ddlManufacturer.SelectedValue);
                    int quantity = Convert.ToInt32(txtQuantity.Text);

                    var result = orderClient.PlaceDistributorOrder(DistributorDbId, manufacturerId, blanketId, quantity);

                    if (result.Success)
                    {
                        ShowMessage("Order placed successfully!", "success");
                        pnlOrderForm.Visible = false;
                        LoadOrders();
                    }
                    else
                    {
                        ShowMessage(result.Message, "error");
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage("Error placing order: " + ex.Message, "error");
                }
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlOrderForm.Visible = false;
            ResetForm();
        }

        private void ResetForm()
        {
            ddlBlanket.SelectedIndex = 0;
            ddlManufacturer.SelectedIndex = 0;
            ddlManufacturer.Enabled = false;
            txtQuantity.Text = "";
            lblMessage.Visible = false;
        }

        protected void gvOrders_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "CancelOrder")
            {
                try
                {
                    int orderId = Convert.ToInt32(e.CommandArgument);
                    var result = orderClient.CancelDistributorOrder(orderId);

                    if (result.Success)
                    {
                        ShowMessage("Order cancelled successfully!", "success");
                        LoadOrders();
                    }
                    else
                    {
                        ShowMessage(result.Message, "error");
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage("Error cancelling order: " + ex.Message, "error");
                }
            }
        }

        protected void gvOrders_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Label lblStatus = (Label)e.Row.FindControl("lblStatus");
                if (lblStatus != null)
                {
                    switch (lblStatus.Text.ToLower())
                    {
                        case "pending":
                            lblStatus.CssClass = "status-pending";
                            break;
                        case "approved":
                            lblStatus.CssClass = "status-approved";
                            break;
                        case "shipped":
                            lblStatus.CssClass = "status-shipped";
                            break;
                        case "completed":
                            lblStatus.CssClass = "status-completed";
                            break;
                        case "cancelled":
                            lblStatus.CssClass = "status-cancelled";
                            break;
                    }
                }
            }
        }

        protected void gvOrders_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvOrders.PageIndex = e.NewPageIndex;
            LoadOrders();
        }

        protected void ddlStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadOrders();
        }

        public string GetModelName(object blanketId)
        {
            if (blanketId == null) return "N/A";

            try
            {
                return blanketService.GetBlanketModelName(Convert.ToInt32(blanketId));
            }
            catch
            {
                return "N/A";
            }
        }

        public string GetManufacturerName(object manufacturerId)
        {
            if (manufacturerId == null) return "N/A";

            try
            {
                var manufacturer = userService.GetUserDetails(Convert.ToInt32(manufacturerId), 'm');
                return manufacturer?.CompanyName ?? "N/A";
            }
            catch
            {
                return "N/A";
            }
        }

        private void ShowMessage(string message, string type)
        {
            lblMessage.Text = message;
            lblMessage.Visible = true;
            lblMessage.CssClass = type == "success" ? "alert alert-success" : "alert alert-danger";
        }
    }
}