using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CozyComfortClient.BlanketServiceReference;

namespace CozyComfortClient.Manufacturer
{
    public partial class AddNewProduct : System.Web.UI.Page
    {
        BlanketServiceSoapClient blanketService = new BlanketServiceSoapClient();

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
                    {
                        lblCompanyName.Text = user.CompanyName;
                    }
                }

                BindGrid();
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
        private void BindGrid()
        {
            gvBlankets.DataSource = blanketService.GetAllBlankets();
            gvBlankets.DataBind();
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string model = txtModel.Text.Trim();
            string material = ddlMaterial.SelectedValue;
            int.TryParse(txtProductionCapacity.Text.Trim(), out int capacity);

            if (hdnBlanketID.Value == "0")
            {
                string result = blanketService.AddBlanket(model, material, capacity);
                lblMessage.Text = result;
            }
            else
            {
                int blanketId = int.Parse(hdnBlanketID.Value);
                string result = blanketService.UpdateBlanket(blanketId, model, material, capacity);
                lblMessage.Text = result;
                btnSubmit.Text = "Add Blanket";
                formTitle.InnerText = "Add New Blanket";
                btnCancel.Visible = false;
                hdnBlanketID.Value = "0";
            }

            lblMessage.Visible = true;
            ClearForm();
            BindGrid();
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
            txtModel.Text = string.Empty;
            ddlMaterial.SelectedIndex = 0;
            txtProductionCapacity.Text = string.Empty;
            hdnBlanketID.Value = "0";
            btnSubmit.Text = "Add Blanket";
            formTitle.InnerText = "Add New Blanket";
            btnCancel.Visible = false;
        }

        protected void gvBlankets_SelectedIndexChanging(object sender, GridViewSelectEventArgs e)
        {
            int blanketId = Convert.ToInt32(gvBlankets.DataKeys[e.NewSelectedIndex].Value);
            var blanket = blanketService.GetBlanket(blanketId);

            if (blanket != null)
            {
                hdnBlanketID.Value = blanket.BlanketID.ToString();
                txtModel.Text = blanket.Model;
                ddlMaterial.SelectedValue = blanket.Material;
                txtProductionCapacity.Text = blanket.ProductionCapacity.ToString();

                btnSubmit.Text = "Update Blanket";
                formTitle.InnerText = "Edit Blanket";
                btnCancel.Visible = true;
            }
        }

        protected void gvBlankets_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int blanketId = Convert.ToInt32(gvBlankets.DataKeys[e.RowIndex].Value);
            string result = blanketService.DeleteBlanket(blanketId);
            lblMessage.Text = result;
            lblMessage.Visible = true;
            BindGrid();
        }

        protected void gvBlankets_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvBlankets.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        protected void gvBlankets_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvBlankets.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        protected void gvBlankets_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvBlankets.EditIndex = -1;
            BindGrid();
        }

        protected void gvBlankets_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = gvBlankets.Rows[e.RowIndex];
            int blanketId = Convert.ToInt32(gvBlankets.DataKeys[e.RowIndex].Value);

            string model = ((TextBox)row.FindControl("txtModelEdit")).Text;
            string material = ((DropDownList)row.FindControl("ddlMaterialEdit")).SelectedValue;
            int capacity = int.Parse(((TextBox)row.FindControl("txtCapacityEdit")).Text);

            string result = blanketService.UpdateBlanket(blanketId, model, material, capacity);
            lblMessage.Text = result;
            lblMessage.Visible = true;

            gvBlankets.EditIndex = -1;
            BindGrid();
        }
    }
}
