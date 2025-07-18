using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using CozyComfortClient.SOrderServiceReference;
using CozyComfortClient.BlanketServiceReference;
using CozyComfortClient.UserServiceReference;
using CozyComfortClient.SellerServiceReference;
using CozyComfortClient.DistributorServiceReference;

namespace CozyComfortClient.Seller
{
    public partial class SOrder : System.Web.UI.Page
    {
        private SOrderServiceSoapClient orderClient = new SOrderServiceSoapClient();
        private BlanketServiceSoapClient blanketService = new BlanketServiceSoapClient();
        private UserServiceSoapClient userService = new UserServiceSoapClient();
        private SellerServiceSoapClient sellerService = new SellerServiceSoapClient();
        private DistributorServiceSoapClient distributorService = new DistributorServiceSoapClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!ValidateUser())
                {
                    Response.Redirect("~/Account/Login.aspx");
                    return;
                }

                InitializePage();
            }
        }

        private bool ValidateUser()
        {
            if (Session["UserType"] == null || Session["UserType"].ToString() != "s")
            {
                ShowErrorMessage("Access denied. Please login as seller.");
                return false;
            }

            if (Session["UserId"] == null)
            {
                ShowErrorMessage("Session expired. Please login again.");
                return false;
            }

            return true;
        }

        private void InitializePage()
        {
            try
            {
                int appUserId = Convert.ToInt32(Session["UserId"]);
                int sellerDbId = sellerService.GetSellerIdByAppUserId(appUserId);

                if (sellerDbId <= 0)
                {
                    ShowErrorMessage("Seller account not found in database.");
                    return;
                }

                Session["SellerDbId"] = sellerDbId;

                var user = userService.GetUserDetails(appUserId, 's');
                if (user == null || !string.IsNullOrEmpty(user.Error))
                {
                    ShowErrorMessage(user?.Error ?? "Failed to load seller details");
                    return;
                }

                

                LoadBlanketDropdown();
                LoadOrdersGrid();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Initialization error: " + ex.Message);
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

        private void LoadBlanketDropdown()
        {
            try
            {
                var blankets = blanketService.GetAllBlankets().ToList();

                ddlBlanket.Items.Clear();
                ddlBlanket.Items.Add(new ListItem("Select Blanket", ""));

                ddlBlanket.DataSource = blankets;
                ddlBlanket.DataTextField = "Model";
                ddlBlanket.DataValueField = "BlanketID";
                ddlBlanket.DataBind();

                ViewState["BlanketList"] = blankets;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error loading blankets: " + ex.Message);
            }
        }

        private void LoadDistributorDropdown(int blanketId)
        {
            try
            {
                var distributors = orderClient.GetDistributorsForBlanket(blanketId);

                ddlDistributor.Items.Clear();
                ddlDistributor.Items.Add(new ListItem("Select Distributor", ""));

                if (distributors != null && distributors.Count() > 0)
                {
                    ddlDistributor.DataSource = distributors;
                    ddlDistributor.DataTextField = "DistributorName";
                    ddlDistributor.DataValueField = "DistributorId";
                    ddlDistributor.DataBind();
                    ddlDistributor.Enabled = true;
                }
                else
                {
                    ddlDistributor.Items.Add(new ListItem("No distributors available for this blanket", ""));
                    ddlDistributor.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error loading distributors: " + ex.Message);
            }
        }

        private void LoadOrdersGrid()
        {
            try
            {
                if (Session["SellerDbId"] == null)
                {
                    ShowErrorMessage("Seller ID not found in session.");
                    return;
                }

                int sellerId = Convert.ToInt32(Session["SellerDbId"]);
                string statusFilter = ddlStatusFilter.SelectedValue;

                var orders = orderClient.GetOrdersBySeller(sellerId, statusFilter);

                
                gvOrders.DataSource = orders;
                gvOrders.DataBind();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error loading orders: " + ex.Message);
            }
        }

        protected void btnNewOrder_Click(object sender, EventArgs e)
        {
            pnlOrderForm.Visible = true;
            ClearForm();
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateOrderInputs())
                    return;

                int sellerId = Convert.ToInt32(Session["SellerDbId"]);
                int distributorId = Convert.ToInt32(ddlDistributor.SelectedValue);
                int blanketId = Convert.ToInt32(ddlBlanket.SelectedValue);
                int quantity = Convert.ToInt32(txtQuantity.Text);
                int orderId = Convert.ToInt32(hdnOrderID.Value);

                if (orderId == 0)
                {
                    var result = orderClient.PlaceOrder(sellerId, distributorId, blanketId, quantity);

                    if (result.Success)
                    {
                        ShowSuccessMessage("Order placed successfully!");
                        ClearForm();
                        LoadOrdersGrid();
                        pnlOrderForm.Visible = false;
                    }
                    else
                    {
                        ShowErrorMessage("Error placing order: " + result.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error saving order: " + ex.Message);
            }
        }

        private bool ValidateOrderInputs()
        {
            if (Session["SellerDbId"] == null)
            {
                ShowErrorMessage("Seller session expired. Please login again.");
                return false;
            }

            if (string.IsNullOrEmpty(ddlBlanket.SelectedValue))
            {
                ShowErrorMessage("Please select a blanket model.");
                return false;
            }

            if (string.IsNullOrEmpty(ddlDistributor.SelectedValue))
            {
                ShowErrorMessage("Please select a distributor.");
                return false;
            }

            int quantity;
            if (!int.TryParse(txtQuantity.Text, out quantity) || quantity <= 0)
            {
                ShowErrorMessage("Please enter a valid quantity (must be greater than 0).");
                return false;
            }

            return true;
        }

        protected void ddlBlanket_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlBlanket.SelectedValue))
            {
                int blanketId = Convert.ToInt32(ddlBlanket.SelectedValue);
                LoadDistributorDropdown(blanketId);
            }
            else
            {
                ddlDistributor.Items.Clear();
                ddlDistributor.Items.Add(new ListItem("Select Distributor", ""));
                ddlDistributor.Enabled = false;
            }
        }

        protected void gvOrders_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "CancelOrder")
                {
                    int orderId = Convert.ToInt32(e.CommandArgument);
                    var result = orderClient.CancelOrder(orderId);

                    if (result.Success)
                    {
                        ShowSuccessMessage("Order cancelled successfully");
                        LoadOrdersGrid();
                    }
                    else
                    {
                        ShowErrorMessage("Error cancelling order: " + result.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error processing command: " + ex.Message);
            }
        }

        protected void gvOrders_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Label lblStatus = (Label)e.Row.FindControl("lblStatus");
                if (lblStatus != null)
                {
                    lblStatus.CssClass = "status-" + lblStatus.Text.ToLower();
                }
            }
        }

        protected void ddlStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadOrdersGrid();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
            pnlOrderForm.Visible = false;
        }

        private void ClearForm()
        {
            hdnOrderID.Value = "0";
            ddlBlanket.SelectedIndex = 0;
            ddlDistributor.Items.Clear();
            ddlDistributor.Items.Add(new ListItem("Select Distributor", ""));
            ddlDistributor.Enabled = false;
            txtQuantity.Text = "";
            formTitle.InnerText = "Place New Order";
            btnSubmit.Text = "Place Order";
        }

        public string GetModelName(object blanketIdObj)
        {
            if (blanketIdObj == null) return "";

            int blanketId = Convert.ToInt32(blanketIdObj);
            var blankets = GetBlanketList();

            var blanket = blankets.FirstOrDefault(b => b.BlanketID == blanketId);
            return blanket?.Model ?? "Unknown";
        }

        public string GetDistributorName(object distributorIdObj)
        {
            if (distributorIdObj == null) return "";

            int distributorId = Convert.ToInt32(distributorIdObj);
            var distributor = distributorService.GetDistributorDetails(distributorId);
            return distributor?.BusinessName ?? "Unknown";
        }

        private List<Blanket> GetBlanketList()
        {
            var blankets = ViewState["BlanketList"] as List<Blanket>;
            if (blankets == null)
            {
                blankets = blanketService.GetAllBlankets().ToList();
                ViewState["BlanketList"] = blankets;
            }
            return blankets;
        }

        protected void gvOrders_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvOrders.PageIndex = e.NewPageIndex;
            LoadOrdersGrid();
        }

        private void ShowErrorMessage(string message)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = "alert alert-danger";
            lblMessage.Visible = true;
        }

        private void ShowSuccessMessage(string message)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = "alert alert-success";
            lblMessage.Visible = true;
        }

        private void ShowInfoMessage(string message)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = "alert alert-info";
            lblMessage.Visible = true;
        }
    }
}