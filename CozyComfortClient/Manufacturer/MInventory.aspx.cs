using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Linq;
using CozyComfortClient.MInventoryServiceReference; 
using CozyComfortClient.BlanketServiceReference; 


namespace CozyComfortClient.Manufacturer
{
    public partial class MInventory : System.Web.UI.Page
    {
        MInventoryServiceSoapClient inventoryClient = new MInventoryServiceSoapClient();
        BlanketServiceSoapClient blanketService = new BlanketServiceSoapClient();
        List<Blanket> blanketList = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
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
                        lblCompanyName.Text = user.CompanyName;
                }

                LoadBlanketDropdown();
                LoadInventoryGrid();
            }
        }

        private void LoadBlanketDropdown()
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

        private void LoadInventoryGrid()
        {
            int manufacturerId = Convert.ToInt32(Session["UserId"]);
            var inventoryList = inventoryClient.GetMInventoriesByManufacturer(manufacturerId);
            gvInventory.DataSource = inventoryList;
            gvInventory.DataBind();
        }

  
        protected string GetModelName(object blanketIdObj)
        {
            if (blanketIdObj == null)
                return "";

            int blanketId = Convert.ToInt32(blanketIdObj);

            var blankets = ViewState["BlanketList"] as List<Blanket>;
            if (blankets == null)
            {
                int manufacturerId = Convert.ToInt32(Session["UserId"]);
                blankets = blanketService.GetBlanketsByManufacturer(manufacturerId).ToList();
                ViewState["BlanketList"] = blankets;
            }

            var blanket = blankets.FirstOrDefault(b => b.BlanketID == blanketId);

            return blanket != null ? blanket.Model : "Unknown";
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int manufacturerId = Convert.ToInt32(Session["UserId"]);
            int blanketId = Convert.ToInt32(ddlBlanket.SelectedValue);
            int quantity = Convert.ToInt32(txtQuantity.Text);
            int inventoryId = Convert.ToInt32(hdnInventoryID.Value);

           
            if (inventoryId == 0) 
            {
                var existingInventories = inventoryClient.GetMInventoriesByManufacturer(manufacturerId);
                bool alreadyExists = existingInventories.Any(inv => inv.MBlanketID == blanketId);

                if (alreadyExists)
                {
                    lblMessage.Text = "This blanket already has an inventory. Please update instead of adding a new one.";
                    lblMessage.Visible = true;
                    return;
                }
            }

            var inventory = new MInventoryServiceReference.MInventory
            {
                MInventoryID = inventoryId,
                ManufacturerID = manufacturerId,
                MBlanketID = blanketId,
                MQuantity = quantity
            };

            string result = inventoryId == 0
                ? inventoryClient.AddMInventory(inventory)
                : inventoryClient.UpdateMInventory(inventory);

            lblMessage.Text = result == "success"
                ? (inventoryId == 0 ? "Inventory added." : "Inventory updated.")
                : result;

            lblMessage.Visible = true;

            ClearForm();
            LoadInventoryGrid();
            LoadBlanketDropdown();
        }


        protected void gvInventory_RowCommand(object sender, GridViewCommandEventArgs e)
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

        protected void gvInventory_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int inventoryId = Convert.ToInt32(gvInventory.DataKeys[e.RowIndex].Value);
            string result = inventoryClient.DeleteMInventory(inventoryId);

            lblMessage.Text = result == "success" ? "Inventory deleted successfully." : result;
            lblMessage.Visible = true;

            LoadInventoryGrid();
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
            btnSubmit.Text = "Add Inventory";
            btnCancel.Visible = false;
            formTitle.InnerText = "Add Inventory";
        }

        protected string GetMaterialName(object blanketIdObj)
        {
            if (blanketIdObj == null)
                return "";

            int blanketId = Convert.ToInt32(blanketIdObj);

            var blankets = ViewState["BlanketList"] as List<Blanket>;
            if (blankets == null)
            {
                int manufacturerId = Convert.ToInt32(Session["UserId"]);
                blankets = blanketService.GetBlanketsByManufacturer(manufacturerId).ToList();
                ViewState["BlanketList"] = blankets;
            }

            var blanket = blankets.FirstOrDefault(b => b.BlanketID == blanketId);
            return blanket != null ? blanket.Material : "Unknown";
        }


        protected void gvInventory_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvInventory.PageIndex = e.NewPageIndex;
            LoadInventoryGrid();
        }
    }
}
