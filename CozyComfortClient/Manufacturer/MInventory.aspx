<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MInventory.aspx.cs" Inherits="CozyComfortClient.Manufacturer.MInventory" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" href="../css/Style.css">
    <link href='https://unpkg.com/boxicons@2.0.9/css/boxicons.min.css' rel='stylesheet'>
    <title>Inventory Management</title>

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
            return confirm('Are you sure you want to delete this inventory record?');
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
                
                <li>
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
                <li class="active">
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
                        <h1>Inventory Management</h1>
                        <ul class="breadcrumb">
                          
                        </ul>
                    </div>
                </div>

                <div class="user-form">
                    <h2 runat="server" id="formTitle">Add Inventory</h2>
                    <div class="form-group">
                        <label for="ddlBlanket">Blanket</label>
                        <asp:DropDownList ID="ddlBlanket" runat="server" CssClass="form-control" DataTextField="Model" 
                            DataValueField="BlanketID" AppendDataBoundItems="true" required>
                            <asp:ListItem Value="">Select Blanket</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="form-group">
                        <label for="txtQuantity">Quantity</label>
                        <asp:TextBox ID="txtQuantity" runat="server" CssClass="form-control" 
                            placeholder="Enter quantity" TextMode="Number" min="1" required></asp:TextBox>
                    </div>

                    <div class="form-actions">
                        <asp:Button ID="btnSubmit" runat="server" Text="Add Inventory" CssClass="btn btn-primary" OnClick="btnSubmit_Click" />
                        <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" />
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-warning" Visible="false" OnClick="btnCancel_Click" />
                    </div>

                    <asp:HiddenField ID="hdnInventoryID" runat="server" Value="0" />
                    <asp:Label ID="lblMessage" runat="server" Visible="false" style="margin-top: 20px; display: block;"></asp:Label>
                </div>

                <div class="table-data" style="margin-top: 30px;">
                    <div class="order">
                        <div class="head">
                            <h3>Inventory Records</h3>
                        </div>
                        <asp:GridView ID="gvInventory" runat="server" AutoGenerateColumns="False" 
                            CssClass="grid-view" DataKeyNames="MInventoryID"
                            OnRowCommand="gvInventory_RowCommand" OnRowDeleting="gvInventory_RowDeleting"
                            AllowPaging="True" OnPageIndexChanging="gvInventory_PageIndexChanging" PageSize="5">
                            <Columns>
                            <asp:BoundField DataField="MInventoryID" HeaderText="ID" />

                            <asp:TemplateField HeaderText="Blanket Model">
                                <ItemTemplate>
                                    <asp:Label ID="lblModel" runat="server" Text='<%# GetModelName(Eval("MBlanketID")) %>'></asp:Label>
                                    <asp:HiddenField ID="hdnBlanketID" runat="server" Value='<%# Eval("MBlanketID") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Material">
                                <ItemTemplate>
                                    <%# GetMaterialName(Eval("MBlanketID")) %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Quantity">
                                <ItemTemplate>
                                    <asp:Label ID="lblQuantity" runat="server" Text='<%# Eval("MQuantity") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="MLastUpdated" HeaderText="Last Updated" DataFormatString="{0:g}" />

                            <asp:ButtonField CommandName="Select" Text="Edit" ButtonType="Button" ControlStyle-CssClass="btn btn-edit" />
                            <asp:CommandField ShowDeleteButton="True" ButtonType="Button" ControlStyle-CssClass="btn btn-delete" DeleteText="Delete" />
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