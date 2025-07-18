<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DOrder.aspx.cs" Inherits="CozyComfortClient.Distributor.DOrder" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" href="../css/Style.css">
    <link href='https://unpkg.com/boxicons@2.0.9/css/boxicons.min.css' rel='stylesheet'>
    <title>Distributor Order Management</title>
    <style>
        .status-pending { color: orange; font-weight: bold; }
        .status-approved { color: blue; font-weight: bold; }
        .status-shipped { color: purple; font-weight: bold; }
        .status-completed { color: green; font-weight: bold; }
        .status-cancelled { color: red; font-weight: bold; }
    </style>
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

        function confirmCancel() {
            return confirm('Are you sure you want to cancel this order?');
        }

        function updateManufacturers() {
            const blanketDropdown = document.getElementById('<%= ddlBlanket.ClientID %>');
            const manufacturerDropdown = document.getElementById('<%= ddlManufacturer.ClientID %>');
            const selectedBlanketId = blanketDropdown.value;
            
            __doPostBack('<%= ddlBlanket.UniqueID %>', '');
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
       
        <section id="sidebar">
            <a href="DSOrder.aspx">
                 <img src="../icon/Logo.png" style="height: 90px; margin-left: 80px;">
            </a>
            <a href="DSOrder.aspx" class="brand">
                <asp:Label runat="server" Text="Cozy Comfort Pvt Ltd" class="text" style="margin-left:10px;"></asp:Label>
            </a>
            <ul class="side-menu top">
                <li>
                    <a href="DSOrder.aspx">
                        <img src="../icon/order.svg" style="width: 20px; height: 20px; vertical-align: middle; margin-left: 5px; margin-right: 5px;" />
                        <span class="text">Seller Orders</span>
                    </a>
                </li>
                <li>
                    <a href="DInventory.aspx">
                        <img src="../icon/inventory.svg" style="width: 20px; height: 20px; vertical-align: middle; margin-left: 5px; margin-right: 5px;" />
                        <span class="text">Inventory</span>
                    </a>
                </li>
                <li class="active">
                    <a href="DOrder.aspx">
                        <img src="../icon/addoder.svg" style="width: 20px; height: 20px; vertical-align: middle; margin-left: 5px; margin-right: 5px;" />
                        <span class="text">Add Orders</span>
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
                <a href="DSOrder.aspx" class="nav-link">Distributor</a>
               
            </nav>
            <main>
                <div class="head-title">
                    <div class="left">
                        <h1>Order Management</h1>
                        <ul class="breadcrumb">
                           
                        </ul>
                    </div>
                    <asp:Button ID="btnNewOrder" runat="server" Text="+ New Order" CssClass="btn btn-primary" OnClick="btnNewOrder_Click" />
                </div>

                <asp:Panel ID="pnlOrderForm" runat="server" Visible="false" CssClass="user-form">
                    <h2 runat="server" id="formTitle">Place New Order</h2>
                    <div class="form-group">
                        <label for="ddlBlanket">Blanket</label>
                        <asp:DropDownList ID="ddlBlanket" runat="server" CssClass="form-control" 
                            DataTextField="Model" DataValueField="BlanketID" 
                            AppendDataBoundItems="true" required AutoPostBack="true" OnSelectedIndexChanged="ddlBlanket_SelectedIndexChanged">
                            <asp:ListItem Value="">Select Blanket</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="form-group">
                        <label for="ddlManufacturer">Manufacturer</label>
                        <asp:DropDownList ID="ddlManufacturer" runat="server" CssClass="form-control" 
                            DataTextField="ManufacturerName" DataValueField="ManufacturerId" 
                            AppendDataBoundItems="true" required Enabled="false">
                            <asp:ListItem Value="">Select Manufacturer</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="form-group">
                        <label for="txtQuantity">Quantity</label>
                        <asp:TextBox ID="txtQuantity" runat="server" CssClass="form-control" 
                            placeholder="Enter quantity" TextMode="Number" min="1" required></asp:TextBox>
                    </div>

                    <div class="form-actions">
                        <asp:Button ID="btnSubmit" runat="server" Text="Place Order" CssClass="btn btn-primary" OnClick="btnSubmit_Click" />
                        <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" />
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-warning" OnClick="btnCancel_Click" />
                    </div>

                    <asp:HiddenField ID="hdnOrderID" runat="server" Value="0" />
                    <asp:Label ID="lblMessage" runat="server" Visible="false" style="margin-top: 20px; display: block;"></asp:Label>
                </asp:Panel>

                <div class="table-data" style="margin-top: 30px;">
                    <div class="order">
                        <div class="head">
                            <h3>Order History</h3>
                            <div class="search-filter">
                                <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlStatusFilter_SelectedIndexChanged">
                                    <asp:ListItem Value="">All Statuses</asp:ListItem>
                                    <asp:ListItem Value="Pending">Pending</asp:ListItem>
                                    <asp:ListItem Value="Approved">Approved</asp:ListItem>
                                    <asp:ListItem Value="Shipped">Shipped</asp:ListItem>
                                    <asp:ListItem Value="Completed">Completed</asp:ListItem>
                                    <asp:ListItem Value="Cancelled">Cancelled</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                        <asp:GridView ID="gvOrders" runat="server" AutoGenerateColumns="False" 
                            CssClass="grid-view" DataKeyNames="OrderId"
                            OnRowCommand="gvOrders_RowCommand" OnRowDataBound="gvOrders_RowDataBound"
                            AllowPaging="True" OnPageIndexChanging="gvOrders_PageIndexChanging" PageSize="5">
                            <Columns>
                                <asp:BoundField DataField="OrderId" HeaderText="Order ID" />
                                
                                <asp:TemplateField HeaderText="Blanket Model">
                                    <ItemTemplate>
                                        <%# GetModelName(Eval("BlanketId")) %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="Manufacturer">
                                    <ItemTemplate>
                                        <%# GetManufacturerName(Eval("ManufacturerId")) %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:BoundField DataField="Quantity" HeaderText="Quantity" />
                                
                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Status") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:BoundField DataField="OrderDate" HeaderText="Order Date" DataFormatString="{0:g}" />
                                
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:Button ID="btnCancelOrder" runat="server" Text="Cancel" 
                                            CssClass="btn btn-delete" CommandName="CancelOrder" 
                                            CommandArgument='<%# Eval("OrderId") %>' 
                                            Visible='<%# Eval("Status").ToString() == "Pending" || Eval("Status").ToString() == "Approved" %>'
                                            OnClientClick="return confirmCancel();" />
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