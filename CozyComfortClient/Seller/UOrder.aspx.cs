using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using CozyComfortClient.UOrderServiceReference;
using CozyComfortClient.BlanketServiceReference;
using CozyComfortClient.UserServiceReference;
using CozyComfortClient.SellerServiceReference;

namespace CozyComfortClient.Seller
{
    public partial class UOrder : System.Web.UI.Page
    {
        private UOrderServiceSoapClient orderService = new UOrderServiceSoapClient();
        private BlanketServiceSoapClient blanketService = new BlanketServiceSoapClient();
        private SellerServiceSoapClient sellerService = new SellerServiceSoapClient();
        private UserServiceSoapClient userService = new UserServiceSoapClient();

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

        private bool ValidateUser()
        {
            if (Session["UserType"] == null || Session["UserType"].ToString() != "s")
                return false;

            if (Session["UserId"] == null)
                return false;

            return true;
        }

        private void InitializePage()
        {
            try
            {
                int appUserId = Convert.ToInt32(Session["UserId"]);
                int sellerId = sellerService.GetSellerIdByAppUserId(appUserId);
                Session["SellerDbId"] = sellerId;

                LoadBlankets();
                LoadOrders();
            }
            catch (Exception ex)
            {
                ShowError("Initialization failed: " + ex.Message);
            }
        }

        private void LoadBlankets()
        {
            var blankets = blanketService.GetAllBlankets().ToList();
            ddlBlanket.DataSource = blankets;
            ddlBlanket.DataTextField = "Model";
            ddlBlanket.DataValueField = "BlanketID";
            ddlBlanket.DataBind();
            ddlBlanket.Items.Insert(0, new ListItem("Select Blanket", ""));
            ViewState["BlanketList"] = blankets;
        }

        private void LoadOrders()
        {
            int sellerId = Convert.ToInt32(Session["SellerDbId"]);
            string filter = ddlStatusFilter.SelectedValue;
            var orders = orderService.GetUOrdersBySeller(sellerId, filter);
            gvOrders.DataSource = orders;
            gvOrders.DataBind();
        }

        protected void btnPlaceOrder_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInputs()) return;

                int sellerId = Convert.ToInt32(Session["SellerDbId"]);
                int blanketId = Convert.ToInt32(ddlBlanket.SelectedValue);
                int quantity = Convert.ToInt32(txtQuantity.Text);
                string username = txtUserName.Text.Trim();
                string userContact = txtUserContact.Text.Trim();
                DateTime? expectedDate = string.IsNullOrEmpty(txtExpectedDate.Text)
                    ? (DateTime?)null
                    : Convert.ToDateTime(txtExpectedDate.Text);

                var result = orderService.PlaceUOrder(sellerId, username, userContact, blanketId, quantity, expectedDate);

                if (result.Success)
                {
                    ShowSuccess("Order placed successfully!");
                    ClearForm();
                    LoadOrders();
                }
                else
                {
                    ShowError(result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowError("Error placing order: " + ex.Message);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(txtUserName.Text.Trim()))
            {
                ShowError("Please enter user name.");
                return false;
            }

            if (string.IsNullOrEmpty(txtUserContact.Text.Trim()))
            {
                ShowError("Please enter user contact.");
                return false;
            }

            if (string.IsNullOrEmpty(ddlBlanket.SelectedValue))
            {
                ShowError("Please select a blanket.");
                return false;
            }

            if (!int.TryParse(txtQuantity.Text, out int qty) || qty <= 0)
            {
                ShowError("Enter a valid quantity greater than 0.");
                return false;
            }

            return true;
        }

        protected void gvOrders_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "CancelOrder")
            {
                int orderId = Convert.ToInt32(e.CommandArgument);
                var result = orderService.CancelUOrder(orderId);

                if (result.Success)
                {
                    ShowSuccess("Order cancelled.");
                    LoadOrders();
                }
                else
                {
                    ShowError(result.Message);
                }
            }
            else if (e.CommandName == "NextStatus")
            {
                string[] args = e.CommandArgument.ToString().Split('|');
                int orderId = Convert.ToInt32(args[0]);
                string currentStatus = args[1];
                string nextStatus = GetNextStatus(currentStatus);

                if (!string.IsNullOrEmpty(nextStatus))
                {
                    var result = orderService.UpdateUOrderStatus(orderId, nextStatus);

                    if (result.Success)
                    {
                        ShowSuccess($"Order status updated to {nextStatus}.");
                        LoadOrders();
                    }
                    else
                    {
                        ShowError(result.Message);
                    }
                }
            }
        }

        private string GetNextStatus(string currentStatus)
        {
            switch (currentStatus)
            {
                case "Pending": return "Approved";
                case "Approved": return "Shipped";
                case "Shipped": return "Completed";
                default: return null;
            }
        }


        protected void ddlStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadOrders();
        }

        protected void gvOrders_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvOrders.PageIndex = e.NewPageIndex;
            LoadOrders();
        }

        public string GetNextStatusText(string currentStatus)
        {
            switch (currentStatus)
            {
                case "Pending": return "Approve";
                case "Approved": return "Ship";
                case "Shipped": return "Complete";
                default: return "";
            }
        }

        public bool ShowNextStatusButton(string status)
        {
            return status == "Pending" || status == "Approved" || status == "Shipped";
        }


        private void ClearForm()
        {
            txtUserName.Text = "";
            txtUserContact.Text = "";
            txtQuantity.Text = "";
            txtExpectedDate.Text = "";
            ddlBlanket.SelectedIndex = 0;
        }

        private void ShowError(string msg)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = "alert alert-danger";
        }

        private void ShowSuccess(string msg)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = "alert alert-success";
        }

        public string GetModelName(object blanketIdObj)
        {
            if (blanketIdObj == null) return "";

            int blanketId = Convert.ToInt32(blanketIdObj);
            var blankets = ViewState["BlanketList"] as List<Blanket>;

            if (blankets == null)
            {
                blankets = blanketService.GetAllBlankets().ToList();
                ViewState["BlanketList"] = blankets;
            }

            var blanket = blankets.FirstOrDefault(b => b.BlanketID == blanketId);
            return blanket?.Model ?? "Unknown";
        }
    }
}
