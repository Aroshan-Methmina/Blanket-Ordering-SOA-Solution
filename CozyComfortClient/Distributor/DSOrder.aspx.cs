using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using CozyComfortClient.SOrderServiceReference;
using CozyComfortClient.BlanketServiceReference;
using CozyComfortClient.UserServiceReference;
using CozyComfortClient.DistributorServiceReference;
using CozyComfortClient.SellerServiceReference;
using CozyComfortClient.DInventoryServiceReference;

namespace CozyComfortClient.Distributor
{
    public partial class DSOrder : System.Web.UI.Page
    {
       
        private readonly SOrderServiceSoapClient orderService = new SOrderServiceSoapClient();
        private readonly BlanketServiceSoapClient blanketService = new BlanketServiceSoapClient();
        private readonly UserServiceSoapClient userService = new UserServiceSoapClient();
        private readonly DistributorServiceSoapClient distributorService = new DistributorServiceSoapClient();
        private readonly SellerServiceSoapClient sellerService = new SellerServiceSoapClient();
        private readonly DInventoryServiceSoapClient inventoryService = new DInventoryServiceSoapClient();

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
            if (Session["UserType"] == null || Session["UserType"].ToString() != "d")
            {
                ShowErrorMessage("Access denied. Please login as distributor.");
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
                int distributorDbId = distributorService.GetDistributorIdByAppUserId(appUserId);

                if (distributorDbId <= 0)
                {
                    ShowErrorMessage("Distributor account not found in database.");
                    return;
                }

                Session["DistributorDbId"] = distributorDbId;

                var user = userService.GetUserDetails(appUserId, 'd');
                if (user == null || !string.IsNullOrEmpty(user.Error))
                {
                    ShowErrorMessage(user?.Error ?? "Failed to load distributor details");
                    return;
                }

                LoadSellerOrders();
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

        protected string GetAvailableQuantity(object blanketIdObj)
        {
            if (blanketIdObj == null || blanketIdObj == DBNull.Value) return "0";

            try
            {
                int blanketId = Convert.ToInt32(blanketIdObj);
                if (Session["DistributorDbId"] == null) return "0";

                int distributorId = Convert.ToInt32(Session["DistributorDbId"]);
                var inventory = inventoryService.GetDInventoriesByDistributor(distributorId)
                    .FirstOrDefault(i => i.DBlanketID == blanketId);

                return (inventory?.DQuantity ?? 0).ToString();
            }
            catch
            {
                return "0";
            }
        }

        private void LoadSellerOrders(string statusFilter = "")
        {
            try
            {
                if (Session["DistributorDbId"] == null)
                {
                    ShowErrorMessage("Distributor ID not found in session.");
                    return;
                }

                int distributorId = Convert.ToInt32(Session["DistributorDbId"]);
                var orders = orderService.GetOrdersByDistributor(distributorId, statusFilter);

                if (orders == null || !orders.Any())
                {
                    ShowInfoMessage("No orders found.");
                    gvSellerOrders.DataSource = null;
                }
                else
                {
                    gvSellerOrders.DataSource = orders;
                }

                gvSellerOrders.DataBind();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error loading orders: " + ex.Message);
            }
        }

        protected void gvSellerOrders_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (int.TryParse(e.CommandArgument.ToString(), out int orderId))
            {
                switch (e.CommandName)
                {
                    case "ApproveOrder":
                        ApproveOrder(orderId);
                        break;
                    case "ShipOrder":
                        ShipOrder(orderId);
                        break;
                    case "CompleteOrder":
                        CompleteOrder(orderId);
                        break;
                    case "CancelOrder":
                        CancelOrder(orderId);
                        break;
                }
                LoadSellerOrders(ddlStatusFilter.SelectedValue);
            }
        }

        private void ApproveOrder(int orderId)
        {
            try
            {
                var order = orderService.GetOrderDetails(orderId);
                if (order == null || !string.IsNullOrEmpty(order.Error))
                {
                    ShowErrorMessage("Order not found: " + order?.Error);
                    return;
                }

                int availableQty = GetAvailableQuantityFromService(order.BlanketId);
                if (availableQty < order.Quantity)
                {
                    ShowErrorMessage($"Cannot approve. Available: {availableQty}, Ordered: {order.Quantity}");
                    return;
                }

                var result = orderService.UpdateOrderStatus(orderId, "Approved");
                if (result.Success)
                {
                    ShowSuccessMessage("Order approved!");
                }
                else
                {
                    ShowErrorMessage("Approval failed: " + result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error approving order: " + ex.Message);
            }
        }

        private int GetAvailableQuantityFromService(int blanketId)
        {
            try
            {
                if (Session["DistributorDbId"] == null) return 0;
                int distributorId = Convert.ToInt32(Session["DistributorDbId"]);

                var inventory = inventoryService.GetDInventoriesByDistributor(distributorId)
                    .FirstOrDefault(i => i.DBlanketID == blanketId);

                return inventory?.DQuantity ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private void ShipOrder(int orderId)
        {
            try
            {
                var order = orderService.GetOrderDetails(orderId);
                if (order == null || !string.IsNullOrEmpty(order.Error))
                {
                    ShowErrorMessage("Order not found: " + order?.Error);
                    return;
                }

                var inventoryResult = inventoryService.ReduceInventory(
                    order.BlanketId,
                    order.Quantity,
                    Convert.ToInt32(Session["DistributorDbId"]));

                if (!inventoryResult.StartsWith("success"))
                {
                    ShowErrorMessage("Inventory update failed: " + inventoryResult);
                    return;
                }

                var result = orderService.UpdateOrderStatus(orderId, "Shipped");
                if (result.Success)
                {
                    ShowSuccessMessage("Order shipped!");
                }
                else
                {
                    ShowErrorMessage("Shipping failed: " + result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error shipping order: " + ex.Message);
            }
        }

        private void CompleteOrder(int orderId)
        {
            try
            {
                var result = orderService.UpdateOrderStatus(orderId, "Completed");
                if (result.Success)
                {
                    ShowSuccessMessage("Order completed!");
                }
                else
                {
                    ShowErrorMessage("Completion failed: " + result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error completing order: " + ex.Message);
            }
        }

        private void CancelOrder(int orderId)
        {
            try
            {
                var result = orderService.CancelOrder(orderId);
                if (result.Success)
                {
                    ShowSuccessMessage("Order cancelled!");
                }
                else
                {
                    ShowErrorMessage("Cancellation failed: " + result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error cancelling order: " + ex.Message);
            }
        }

        protected void gvSellerOrders_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.FindControl("lblStatus") is Label lblStatus)
                {
                    string status = lblStatus.Text.ToLower();
                    lblStatus.CssClass = $"status-{status}";
                }

                int orderId = Convert.ToInt32(gvSellerOrders.DataKeys[e.Row.RowIndex].Value);
                var order = orderService.GetOrderDetails(orderId);

                if (order != null && string.IsNullOrEmpty(order.Error) &&
                    e.Row.FindControl("litSellerContact") is Literal litContact)
                {
                    var seller = sellerService.GetSellerDetails(order.SellerId);
                    litContact.Text = seller != null && string.IsNullOrEmpty(seller.Error)
                        ? seller.ContactNumber
                        : "N/A";
                }
            }
        }

        protected void ddlStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSellerOrders(ddlStatusFilter.SelectedValue);
        }

        protected void gvSellerOrders_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvSellerOrders.PageIndex = e.NewPageIndex;
            LoadSellerOrders(ddlStatusFilter.SelectedValue);
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