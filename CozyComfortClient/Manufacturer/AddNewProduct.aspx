<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AddNewProduct.aspx.cs" Inherits="CozyComfortClient.Manufacturer.AddNewProduct" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" href="../css/Style.css">
    <link href='https://unpkg.com/boxicons@2.0.9/css/boxicons.min.css' rel='stylesheet'>
    <title>Blanket Management</title>

    <script>
        const allSideMenu = document.querySelectorAll('#sidebar .side-menu.top li a');

        allSideMenu.forEach(item => {
            const li = item.parentElement;

            item.addEventListener('click', function () {
                allSideMenu.forEach(i => {
                    i.parentElement.classList.remove('active');
                })
                li.classList.add('active');
            })
        });
        const menuBar = document.querySelector('#content nav .bx.bx-menu');
        const sidebar = document.getElementById('sidebar');

        menuBar.addEventListener('click', function () {
            sidebar.classList.toggle('hide');
        });

        function confirmDelete() {
            return confirm('Are you sure you want to delete this blanket?');
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
         <section id="sidebar">
            <a href="AddNewProduct.aspx" >
                 <img src="../icon/Logo.png"  style="height: 90px; margin-left: 80px;  ">
            </a>
            <a href="AddNewProduct.aspx" class="brand">
                <asp:Label ID="lblCompanyName" runat="server" Text="Manufacturer" class="text" style="margin-left:10px;"></asp:Label>
            </a>
            <ul class="side-menu top">
                
                <li class="active">
                    <a href="AddNewProduct.aspx">
                        <img src="../icon/addproduct.svg" style="width: 20px; height: 20px; vertical-align: middle; margin-left: 5px; margin-right: 5px;" />
                        <span class="text">Add New Product</span>
                    </a>
                </li>
                 <li>
                    <a href="MDOrder.aspx">
                        <img src="../icon/order.svg" style="width: 20px; height: 20px; vertical-align: middle; margin-left: 5px; margin-right: 5px;" />
                        <span class="text">Order</span>
                    </a>
                </li>
                <li>
                    <a href="MInventory.aspx">
                        <img src="../icon/inventory.svg" style="width: 20px; height: 20px; vertical-align: middle; margin-left: 5px; margin-right: 5px;" />
                        <span class="text">Inventory</span>
                    </a>
                </li>
                <li>
                    <a href="AddNewDistributor.aspx">
                       <img src="../icon/addd.svg" style="width: 20px; height: 20px; vertical-align: middle; margin-left: 5px; margin-right: 5px;" />
                        <span class="text">Add New Distributor</span>
                    </a>
                </li>
                <li>
                    <a href="AddNewSeller.aspx">
                        <img src="../icon/addseller.svg" style="width: 20px; height: 20px; vertical-align: middle; margin-left: 5px; margin-right: 5px;" />
                        <span class="text">Add New Seller</span>
                    </a>
                </li>
            </ul>
            <ul class="side-menu">
            
                <li>
                    <asp:LinkButton ID="lnkLogout" runat="server" OnClick="lnkLogout_Click" CssClass="logout">
                        <img src="../icon/ulogout.svg" style="width: 20px; height: 20px; vertical-align: middle; margin-left: 5px; margin-right: 5px;" />
                        <span class="text">Logout</span>
                    </asp:LinkButton>
                </li>
            </ul>
        </section>
        
       
        <section id="content">
            <nav>
                <img src="../icon/menu.svg" style="width: 20px; height: 20px; vertical-align: middle; margin-left: 5px; margin-right: 5px;" />
                <a href="AddNewProduct.aspx" class="nav-link">Manufacturer</a>
              
            </nav>

            <main>
                <div class="head-title">
                    <div class="left">
                        <h1>Blanket Management</h1>
                        <ul class="breadcrumb">
                           
                        </ul>
                    </div>
                </div>

                <div class="user-form">
                    <h2 runat="server" id="formTitle">Add New Blanket</h2>
                    <div class="form-group">
                        <label for="txtModel">Model</label>
                        <asp:TextBox ID="txtModel" runat="server" CssClass="form-control" placeholder="Enter blanket model" required></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="ddlMaterial">Material</label>
                        <asp:DropDownList ID="ddlMaterial" runat="server" CssClass="form-control" required>
                            <asp:ListItem Value="">Select Material</asp:ListItem>
                            <asp:ListItem Value="Wool">Wool</asp:ListItem>
                            <asp:ListItem Value="Cotton">Cotton</asp:ListItem>
                            <asp:ListItem Value="Fleece">Fleece</asp:ListItem>
                            <asp:ListItem Value="Acrylic">Acrylic</asp:ListItem>
                            <asp:ListItem Value="Polyester">Polyester</asp:ListItem>
                            <asp:ListItem Value="Other">Other</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="form-group">
                        <label for="txtProductionCapacity">Production Capacity (per day)</label>
                        <asp:TextBox ID="txtProductionCapacity" runat="server" CssClass="form-control" 
                            placeholder="Enter daily production capacity" TextMode="Number" min="0"></asp:TextBox>
                    </div>

                    <div class="form-actions">
                        <asp:Button ID="btnSubmit" runat="server" Text="Add Blanket" CssClass="btn btn-primary" OnClick="btnSubmit_Click" />
                        <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" />
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-warning" Visible="false" OnClick="btnCancel_Click" />
                    </div>

                    <asp:HiddenField ID="hdnBlanketID" runat="server" Value="0" />
                    <asp:Label ID="lblMessage" runat="server" Visible="false" style="margin-top: 20px; display: block;"></asp:Label>
                </div>

                <div class="table-data" style="margin-top: 30px;">
                    <div class="order">
                        <div class="head">
                            <h3>Existing Blankets</h3>
                        </div>
                        <asp:GridView ID="gvBlankets" runat="server" AutoGenerateColumns="False" 
                            CssClass="grid-view" DataKeyNames="BlanketID"
                            OnRowEditing="gvBlankets_RowEditing" OnRowCancelingEdit="gvBlankets_RowCancelingEdit"
                            OnRowUpdating="gvBlankets_RowUpdating" OnRowDeleting="gvBlankets_RowDeleting"
                            OnSelectedIndexChanging="gvBlankets_SelectedIndexChanging"
                            AllowPaging="True" OnPageIndexChanging="gvBlankets_PageIndexChanging" PageSize="5">
                            <Columns>
                                <asp:BoundField DataField="BlanketID" HeaderText="ID" ReadOnly="true" />
                                <asp:TemplateField HeaderText="Model">
                                    <ItemTemplate>
                                        <asp:Label ID="lblModel" runat="server" Text='<%# Eval("Model") %>'></asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtModelEdit" runat="server" Text='<%# Bind("Model") %>' CssClass="form-control"></asp:TextBox>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Material">
                                    <ItemTemplate>
                                        <asp:Label ID="lblMaterial" runat="server" Text='<%# Eval("Material") %>'></asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:DropDownList ID="ddlMaterialEdit" runat="server" CssClass="form-control" SelectedValue='<%# Bind("Material") %>'>
                                            <asp:ListItem Value="Wool">Wool</asp:ListItem>
                                            <asp:ListItem Value="Cotton">Cotton</asp:ListItem>
                                            <asp:ListItem Value="Fleece">Fleece</asp:ListItem>
                                            <asp:ListItem Value="Acrylic">Acrylic</asp:ListItem>
                                            <asp:ListItem Value="Polyester">Polyester</asp:ListItem>
                                            <asp:ListItem Value="Other">Other</asp:ListItem>
                                        </asp:DropDownList>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Production Capacity">
                                    <ItemTemplate>
                                        <asp:Label ID="lblCapacity" runat="server" Text='<%# Eval("ProductionCapacity") %>'></asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtCapacityEdit" runat="server" Text='<%# Bind("ProductionCapacity") %>' 
                                            CssClass="form-control" TextMode="Number" min="0"></asp:TextBox>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                                <asp:CommandField ShowSelectButton="True" SelectText="Edit in Form" ButtonType="Button" ControlStyle-CssClass="btn btn-edit" />
                                
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Button ID="btnDelete" runat="server" CommandName="Delete" Text="Delete" 
                                            CssClass="btn btn-delete" OnClientClick="return confirm('Are you sure you want to delete this blanket?');" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <PagerStyle CssClass="grid-pager" />
                            <HeaderStyle CssClass="grid-header" />
                            <RowStyle CssClass="grid-row" />
                            <AlternatingRowStyle CssClass="grid-alt-row" />
                        </asp:GridView>
                    </div>
                </div>
            </main>
        </section>
    </form>
</body>
</html>