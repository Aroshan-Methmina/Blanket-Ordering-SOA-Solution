<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AddNewSeller.aspx.cs" Inherits="CozyComfortClient.Manufacturer.AddNewSeller" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" href="../css/Style.css">
    <link href='https://unpkg.com/boxicons@2.0.9/css/boxicons.min.css' rel='stylesheet'>
    <title>Seller Management</title>

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
            return confirm('Are you sure you want to delete this seller?');
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
                <li class="active">
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
                        <h1>Seller Management</h1>
                        <ul class="breadcrumb">
                           
                        </ul>
                    </div>
                </div>

                <div class="user-form">
                    <h2 runat="server" id="formTitle">Add New Seller</h2>
                    
                    <div class="form-group">
                        <label for="txtEmail">Email</label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" 
                            placeholder="Enter seller email" TextMode="Email" required></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="txtPassword">Password</label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" 
                            placeholder="Enter password" TextMode="Password" required></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="txtStoreName">Store Name</label>
                        <asp:TextBox ID="txtStoreName" runat="server" CssClass="form-control" 
                            placeholder="Enter store name" required></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="txtContact">Contact Number</label>
                        <asp:TextBox ID="txtContact" runat="server" CssClass="form-control" 
                            placeholder="Enter contact number"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="txtStoreLocation">Store Location</label>
                        <asp:TextBox ID="txtStoreLocation" runat="server" CssClass="form-control" 
                            placeholder="Enter store location"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="txtWebsite">Website (optional)</label>
                        <asp:TextBox ID="txtWebsite" runat="server" CssClass="form-control" 
                            placeholder="Enter website URL"></asp:TextBox>
                    </div>

                    <div class="form-actions">
                        <asp:Button ID="btnSubmit" runat="server" Text="Add Seller" CssClass="btn btn-primary" OnClick="btnSubmit_Click" />
                        <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" />
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-warning" Visible="false" OnClick="btnCancel_Click" />
                    </div>

                    <asp:HiddenField ID="hdnSellerID" runat="server" Value="0" />
                    <asp:Label ID="lblMessage" runat="server" Visible="false" style="margin-top: 20px; display: block;"></asp:Label>
                </div>

                <div class="table-data" style="margin-top: 30px;">
                    <div class="order">
                        <div class="head">
                            <h3>Existing Sellers</h3>
                        </div>
                        <asp:GridView ID="gvSellers" runat="server" AutoGenerateColumns="False" 
                            CssClass="grid-view" DataKeyNames="sid"
                            OnRowEditing="gvSellers_RowEditing" OnRowCancelingEdit="gvSellers_RowCancelingEdit"
                            OnRowUpdating="gvSellers_RowUpdating" OnRowDeleting="gvSellers_RowDeleting"
                            OnSelectedIndexChanging="gvSellers_SelectedIndexChanging"
                            AllowPaging="True" OnPageIndexChanging="gvSellers_PageIndexChanging" PageSize="5">
                            <Columns>
                                <asp:BoundField DataField="sid" HeaderText="ID" ReadOnly="true" />
                                
                                <asp:TemplateField HeaderText="Store Name">
                                    <ItemTemplate>
                                        <asp:Label ID="lblStoreName" runat="server" Text='<%# Eval("store_name") %>'></asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtStoreNameEdit" runat="server" Text='<%# Bind("store_name") %>' CssClass="form-control"></asp:TextBox>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="Contact">
                                    <ItemTemplate>
                                        <asp:Label ID="lblContact" runat="server" Text='<%# Eval("seller_contact") %>'></asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtContactEdit" runat="server" Text='<%# Bind("seller_contact") %>' CssClass="form-control"></asp:TextBox>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="Location">
                                    <ItemTemplate>
                                        <asp:Label ID="lblStoreLocation" runat="server" Text='<%# Eval("store_location") %>'></asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtStoreLocationEdit" runat="server" Text='<%# Bind("store_location") %>' CssClass="form-control"></asp:TextBox>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="Website">
                                    <ItemTemplate>
                                        <asp:Label ID="lblWebsite" runat="server" Text='<%# Eval("website") %>'></asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtWebsiteEdit" runat="server" Text='<%# Bind("website") %>' CssClass="form-control"></asp:TextBox>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:CommandField ShowSelectButton="True" SelectText="Edit in Form" ButtonType="Button" ControlStyle-CssClass="btn btn-edit" />
                                
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Button ID="btnDelete" runat="server" CommandName="Delete" Text="Delete" 
                                            CssClass="btn btn-delete" OnClientClick="return confirm('Are you sure you want to delete this seller?');" />
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