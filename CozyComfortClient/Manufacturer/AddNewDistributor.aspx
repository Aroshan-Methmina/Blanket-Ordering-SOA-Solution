<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AddNewDistributor.aspx.cs" Inherits="CozyComfortClient.Manufacturer.AddNewDistributor" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" href="../css/Style.css">
    <link href='https://unpkg.com/boxicons@2.0.9/css/boxicons.min.css' rel='stylesheet'>
    <title>Distributor Management</title>

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
            return confirm('Are you sure you want to delete this distributor?');
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
                <li class="active">
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
                        <h1>Distributor Management</h1>
                        <ul class="breadcrumb">
                          
                        </ul>
                    </div>
                </div>

                <div class="user-form">
                    <h2 runat="server" id="formTitle">Add New Distributor</h2>
                    
                    <div class="form-group">
                        <label for="txtEmail">Email</label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" 
                            placeholder="Enter distributor email" TextMode="Email" required></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="txtPassword">Password</label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" 
                            placeholder="Enter password" TextMode="Password" required></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="txtBusinessName">Business Name</label>
                        <asp:TextBox ID="txtBusinessName" runat="server" CssClass="form-control" 
                            placeholder="Enter business name" required></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="txtContact">Contact Number</label>
                        <asp:TextBox ID="txtContact" runat="server" CssClass="form-control" 
                            placeholder="Enter contact number"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="txtWarehouse">Warehouse Location</label>
                        <asp:TextBox ID="txtWarehouse" runat="server" CssClass="form-control" 
                            placeholder="Enter warehouse location"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="txtLicense">License Number</label>
                        <asp:TextBox ID="txtLicense" runat="server" CssClass="form-control" 
                            placeholder="Enter license number"></asp:TextBox>
                    </div>

                    <div class="form-actions">
                        <asp:Button ID="btnSubmit" runat="server" Text="Add Distributor" CssClass="btn btn-primary" OnClick="btnSubmit_Click" />
                        <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" />
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-warning" Visible="false" OnClick="btnCancel_Click" />
                    </div>

                    <asp:HiddenField ID="hdnDistributorID" runat="server" Value="0" />
                    <asp:Label ID="lblMessage" runat="server" Visible="false" style="margin-top: 20px; display: block;"></asp:Label>
                </div>

                <div class="table-data" style="margin-top: 30px;">
                    <div class="order">
                        <div class="head">
                            <h3>Existing Distributors</h3>
                        </div>
                        <asp:GridView ID="gvDistributors" runat="server" AutoGenerateColumns="False" 
                            CssClass="grid-view" DataKeyNames="did"
                            OnRowEditing="gvDistributors_RowEditing" OnRowCancelingEdit="gvDistributors_RowCancelingEdit"
                            OnRowUpdating="gvDistributors_RowUpdating" OnRowDeleting="gvDistributors_RowDeleting"
                            OnSelectedIndexChanging="gvDistributors_SelectedIndexChanging"
                            AllowPaging="True" OnPageIndexChanging="gvDistributors_PageIndexChanging" PageSize="5">
                            <Columns>
                                <asp:BoundField DataField="did" HeaderText="ID" ReadOnly="true" />
                                
                                <asp:TemplateField HeaderText="Business Name">
                                    <ItemTemplate>
                                        <asp:Label ID="lblBusinessName" runat="server" Text='<%# Eval("business_name") %>'></asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtBusinessNameEdit" runat="server" Text='<%# Bind("business_name") %>' CssClass="form-control"></asp:TextBox>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                             
                                
                                <asp:TemplateField HeaderText="Contact">
                                    <ItemTemplate>
                                        <asp:Label ID="lblContact" runat="server" Text='<%# Eval("distributor_contact") %>'></asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtContactEdit" runat="server" Text='<%# Bind("distributor_contact") %>' CssClass="form-control"></asp:TextBox>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="Warehouse">
                                    <ItemTemplate>
                                        <asp:Label ID="lblWarehouse" runat="server" Text='<%# Eval("warehouse_location") %>'></asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtWarehouseEdit" runat="server" Text='<%# Bind("warehouse_location") %>' CssClass="form-control"></asp:TextBox>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="License">
                                    <ItemTemplate>
                                        <asp:Label ID="lblLicense" runat="server" Text='<%# Eval("license_number") %>'></asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtLicenseEdit" runat="server" Text='<%# Bind("license_number") %>' CssClass="form-control"></asp:TextBox>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:CommandField ShowSelectButton="True" SelectText="Edit in Form" ButtonType="Button" ControlStyle-CssClass="btn btn-edit" />
                                
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Button ID="btnDelete" runat="server" CommandName="Delete" Text="Delete" 
                                            CssClass="btn btn-delete" OnClientClick="return confirm('Are you sure you want to delete this distributor?');" />
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