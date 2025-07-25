﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using CozyComfortClient.SInventoryServiceReference;
using CozyComfortClient.BlanketServiceReference;
using CozyComfortClient.UserServiceReference;
using CozyComfortClient.SellerServiceReference;

namespace CozyComfortClient.Seller
{
    public partial class SInventory : System.Web.UI.Page
    {
        private SInventoryServiceSoapClient inventoryClient = new SInventoryServiceSoapClient();
        private BlanketServiceSoapClient blanketService = new BlanketServiceSoapClient();
        private UserServiceSoapClient userService = new UserServiceSoapClient();
        private SellerServiceSoapClient sellerService = new SellerServiceSoapClient();

        private List<Blanket> blanketList = null;

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
                LoadInventoryGrid();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Initialization error: " + ex.Message);
            }
        }

        private void LoadBlanketDropdown()
        {
            try
            {
                blanketList = blanketService.GetAllBlankets().ToList();

                ddlBlanket.Items.Clear();
                ddlBlanket.Items.Add(new ListItem("Select Blanket", ""));

                ddlBlanket.DataSource = blanketList;
                ddlBlanket.DataTextField = "Model";
                ddlBlanket.DataValueField = "BlanketID";
                ddlBlanket.DataBind();

                ViewState["BlanketList"] = blanketList;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error loading blankets: " + ex.Message);
            }
        }

        private void LoadInventoryGrid()
        {
            try
            {
                if (Session["SellerDbId"] == null)
                {
                    ShowErrorMessage("Seller ID not found in session.");
                    return;
                }

                int sellerId = Convert.ToInt32(Session["SellerDbId"]);
                var inventoryList = inventoryClient.GetSInventoriesBySeller(sellerId);

                if (inventoryList == null)
                {
                    ShowErrorMessage("No inventory data received from service.");
                    return;
                }

                gvInventory.DataSource = inventoryList;
                gvInventory.DataBind();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error loading inventory: " + ex.Message);
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInventoryInputs())
                    return;

                int sellerId = Convert.ToInt32(Session["SellerDbId"]);
                int blanketId = Convert.ToInt32(ddlBlanket.SelectedValue);
                int quantity = Convert.ToInt32(txtQuantity.Text);
                int inventoryId = Convert.ToInt32(hdnInventoryID.Value);

                if (inventoryId == 0 && InventoryExists(sellerId, blanketId))
                {
                    ShowErrorMessage("This blanket already exists in inventory.");
                    return;
                }

                var inventory = new SInventoryServiceReference.SInventory
                {
                    SInventoryID = inventoryId,
                    SellerID = sellerId,
                    SBlanketID = blanketId,
                    SQuantity = quantity,
                    SLastUpdated = DateTime.Now
                };

                var result = inventoryId == 0
                    ? inventoryClient.AddSInventory(inventory)
                    : inventoryClient.UpdateSInventory(inventory);

                if (result.Success)
                {
                    ShowSuccessMessage(inventoryId == 0 ? "Inventory added successfully!" : "Inventory updated successfully!");
                    ClearForm();
                    LoadInventoryGrid();
                }
                else
                {
                    ShowErrorMessage("Error: " + result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error saving inventory: " + ex.Message);
            }
        }

        private bool ValidateInventoryInputs()
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

            int quantity;
            if (!int.TryParse(txtQuantity.Text, out quantity) || quantity <= 0)
            {
                ShowErrorMessage("Please enter a valid quantity (must be greater than 0).");
                return false;
            }

            return true;
        }

        private bool InventoryExists(int sellerId, int blanketId)
        {
            var existingInventories = inventoryClient.GetSInventoriesBySeller(sellerId);
            return existingInventories != null && existingInventories.Any(inv => inv.SBlanketID == blanketId);
        }

        protected void gvInventory_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "Select")
                {
                    int index = Convert.ToInt32(e.CommandArgument);
                    GridViewRow row = gvInventory.Rows[index];

                    hdnInventoryID.Value = gvInventory.DataKeys[index].Value.ToString();
                    ddlBlanket.SelectedValue = ((HiddenField)row.FindControl("hdnBlanketID")).Value;
                    txtQuantity.Text = ((Label)row.FindControl("lblQuantity")).Text;

                    formTitle.InnerText = "Update Inventory";
                    btnSubmit.Text = "Update";
                    btnCancel.Visible = true;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error selecting record: " + ex.Message);
            }
        }

        protected void gvInventory_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int inventoryId = Convert.ToInt32(gvInventory.DataKeys[e.RowIndex].Value);
                var result = inventoryClient.DeleteSInventory(inventoryId);

                if (result.Success)
                {
                    ShowSuccessMessage("Inventory record deleted successfully.");
                    LoadInventoryGrid();
                }
                else
                {
                    ShowErrorMessage("Delete failed: " + result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error deleting record: " + ex.Message);
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (Session["SellerDbId"] == null)
                {
                    ShowErrorMessage("Seller session expired.");
                    return;
                }

                int sellerId = Convert.ToInt32(Session["SellerDbId"]);
                var inventoryList = inventoryClient.GetSInventoriesBySeller(sellerId);

                if (!string.IsNullOrEmpty(txtSearch.Text))
                {
                    string searchTerm = txtSearch.Text.ToLower();
                    var filteredList = inventoryList.Where(inv =>
                        GetModelName(inv.SBlanketID).ToLower().Contains(searchTerm) ||
                        GetMaterialName(inv.SBlanketID).ToLower().Contains(searchTerm)
                    ).ToList();

                    gvInventory.DataSource = filteredList;
                }
                else
                {
                    gvInventory.DataSource = inventoryList;
                }

                gvInventory.DataBind();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Search error: " + ex.Message);
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hdnInventoryID.Value = "0";
            ddlBlanket.SelectedIndex = 0;
            txtQuantity.Text = "";
            formTitle.InnerText = "Add Inventory";
            btnSubmit.Text = "Add Inventory";
            btnCancel.Visible = false;
        }

        public string GetModelName(object blanketIdObj)
        {
            if (blanketIdObj == null) return "";

            int blanketId = Convert.ToInt32(blanketIdObj);
            var blankets = GetBlanketList();

            var blanket = blankets.FirstOrDefault(b => b.BlanketID == blanketId);
            return blanket?.Model ?? "Unknown";
        }

        public string GetMaterialName(object blanketIdObj)
        {
            if (blanketIdObj == null) return "";

            int blanketId = Convert.ToInt32(blanketIdObj);
            var blankets = GetBlanketList();

            var blanket = blankets.FirstOrDefault(b => b.BlanketID == blanketId);
            return blanket?.Material ?? "Unknown";
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

        protected void gvInventory_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvInventory.PageIndex = e.NewPageIndex;
            LoadInventoryGrid();
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
    }
}
