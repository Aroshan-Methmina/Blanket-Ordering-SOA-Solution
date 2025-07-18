using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CozyComfortClient.DOrderServiceReference;
using CozyComfortClient.ManufacturerServiceReference;
using CozyComfortClient.MInventoryServiceReference;

namespace CozyComfortClient.Manufacturer
{
    public partial class MDOrder : Page
    {
        private readonly DOrderServiceSoapClient _orderService = new DOrderServiceSoapClient();
        private readonly ManufacturerServiceSoapClient _manufacturerService = new ManufacturerServiceSoapClient();
        private readonly MInventoryServiceSoapClient inventoryClient = new MInventoryServiceSoapClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["UserId"] == null || Session["UserType"] == null || Session["UserType"].ToString() != "m")
                {
                    Response.Redirect("~/Login.aspx");
                }

                LoadManufacturerDetails();
                LoadDistributorOrders();
            }
        }

        private void LoadManufacturerDetails()
        {
            int manufacturerId = GetManufacturerId();
            if (manufacturerId == -1) return;

            var manufacturer = _manufacturerService.GetManufacturerDetails(manufacturerId);
            if (manufacturer != null)
            {
                lblCompanyName.Text = manufacturer.CompanyName;
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

        private int GetManufacturerId()
        {
            if (Session["UserId"] == null) return -1;

            int userId;
            if (!int.TryParse(Session["UserId"].ToString(), out userId)) return -1;

            var manufacturer = _manufacturerService.GetManufacturerDetails(userId);
            return manufacturer != null ? manufacturer.ManufacturerId : -1;
        }

        protected void ddlStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadDistributorOrders();
        }

        private void LoadDistributorOrders()
        {
            int manufacturerId = GetManufacturerId();
            if (manufacturerId == -1)
            {
                lblMessage.Text = "Error: Unable to identify manufacturer";
                lblMessage.Visible = true;
                return;
            }

            var orders = _orderService.GetDistributorOrdersByManufacturer(manufacturerId, ddlStatusFilter.SelectedValue);

            if (orders == null || (orders.Count() > 0 && !string.IsNullOrEmpty(orders[0].Error)))
            {
                lblMessage.Text = "Error loading orders: " +
                    (orders != null && orders.Count() > 0 ? orders[0].Error : "Unknown error");
                lblMessage.Visible = true;
                gvDistributorOrders.Visible = false;
            }
            else
            {
                gvDistributorOrders.DataSource = orders;
                gvDistributorOrders.DataBind();
                gvDistributorOrders.Visible = true;
                lblMessage.Visible = false;
            }
        }


        protected int GetAvailableQuantity(int manufacturerId, int blanketId)
        {
            try
            {
                MInventoryServiceSoapClient inventoryService = new MInventoryServiceSoapClient();
                var inventoryList = inventoryService.GetMInventoriesByManufacturer(manufacturerId);

                var inventoryItem = inventoryList.FirstOrDefault(i => i.MBlanketID == blanketId);
                return inventoryItem != null ? inventoryItem.MQuantity : 0;
            }
            catch
            {
                return 0; 
            }
        }


        protected void gvDistributorOrders_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var lblStatus = e.Row.FindControl("lblStatus") as Label;
                if (lblStatus != null)
                {
                    lblStatus.CssClass = "status-" + lblStatus.Text.ToLower();
                }

                int orderQty = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Quantity"));
                int availableQty = Convert.ToInt32(((Label)e.Row.FindControl("lblAvailableQty")).Text);

                if (orderQty > availableQty)
                {
                    e.Row.CssClass = "low-stock-row";
                    ((Label)e.Row.FindControl("lblAvailableQty")).CssClass = "low-stock";
                }
            }
        }

        protected void gvDistributorOrders_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ApproveOrder" || e.CommandName == "ShipOrder" ||
                e.CommandName == "CompleteOrder" || e.CommandName == "CancelOrder")
            {
                if (!int.TryParse(e.CommandArgument.ToString(), out int orderId))
                {
                    lblMessage.Text = "Invalid order ID";
                    lblMessage.Visible = true;
                    return;
                }

                string newStatus;
                switch (e.CommandName)
                {
                    case "ApproveOrder":
                        newStatus = "Approved";
                        break;
                    case "ShipOrder":
                        newStatus = "Shipped";
                        break;
                    case "CompleteOrder":
                        newStatus = "Completed";
                        break;
                    case "CancelOrder":
                        newStatus = "Cancelled";
                        break;
                    default:
                        newStatus = string.Empty;
                        break;
                }

                var result = _orderService.UpdateDistributorOrderStatus(orderId, newStatus);
                if (result != null && result.Success)
                {
                    lblMessage.Text = "Order status updated successfully!";
                    lblMessage.CssClass = "success-message";
                    LoadDistributorOrders();
                }
                else
                {
                    lblMessage.Text = "Error updating order: " + (result != null ? result.Message : "Unknown error");
                    lblMessage.CssClass = "error-message";
                }
                lblMessage.Visible = true;
            }
        }

        protected void gvDistributorOrders_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvDistributorOrders.PageIndex = e.NewPageIndex;
            LoadDistributorOrders();
        }
    }
}