<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DSOrder.aspx.cs" Inherits="CozyComfortClient.Distributor.DSOrder" %>

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
        
        .btn-approve { background-color: #4CAF50; color: white; }
        .btn-ship { background-color: #2196F3; color: white; }
        .btn-complete { background-color: #673AB7; color: white; }
        .btn-cancel { background-color: #f44336; color: white; }
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

        function confirmAction(action) {
            return confirm(`Are you sure you want to ${action} this order?`);
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
                <li class="active">
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
                <li>
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
                        <h1>Seller Order Management</h1>
                        <ul class="breadcrumb">
                           
                        </ul>
                    </div>
                </div>

                <div class="table-data" style="margin-top: 30px;">
                    <div class="order">
                        <div class="head">
                            <h3>Seller Orders</h3>
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
                        <asp:Label ID="lblMessage" runat="server" Visible="false" style="margin-top: 20px; display: block;"></asp:Label>
                        <asp:GridView ID="gvSellerOrders" runat="server" AutoGenerateColumns="False" 
                            CssClass="grid-view" DataKeyNames="OrderId"
                            OnRowCommand="gvSellerOrders_RowCommand" OnRowDataBound="gvSellerOrders_RowDataBound"
                            AllowPaging="True" OnPageIndexChanging="gvSellerOrders_PageIndexChanging" PageSize="5">
                            <Columns>
                                <asp:BoundField DataField="OrderId" HeaderText="Order ID" />
                                
                                <asp:TemplateField HeaderText="Blanket Model">
                                    <ItemTemplate>
                                        <%# Eval("BlanketModel") %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="Store Name">
                                    <ItemTemplate>
                                        <%# Eval("StoreName") %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                              
                                
                                <asp:BoundField DataField="Quantity" HeaderText="Quantity" />
                                
                                <asp:TemplateField HeaderText="Available Qty">
                                    <ItemTemplate>
                                        <%# GetAvailableQuantity(Eval("BlanketId")) %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Status") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:BoundField DataField="OrderDate" HeaderText="Order Date" DataFormatString="{0:g}" />
                                
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:Button ID="btnApprove" runat="server" Text="Approve" 
                                            CssClass="btn btn-approve" CommandName="ApproveOrder" 
                                            CommandArgument='<%# Eval("OrderId") %>' 
                                            Visible='<%# Eval("Status").ToString() == "Pending" %>'
                                            OnClientClick="return confirmAction('approve');" />
                                            
                                        <asp:Button ID="btnMarkShipped" runat="server" Text="Ship" 
                                            CssClass="btn btn-ship" CommandName="ShipOrder" 
                                            CommandArgument='<%# Eval("OrderId") %>' 
                                            Visible='<%# Eval("Status").ToString() == "Approved" %>'
                                            OnClientClick="return confirmAction('ship');" />
                                            
                                        <asp:Button ID="btnComplete" runat="server" Text="Complete" 
                                            CssClass="btn btn-complete" CommandName="CompleteOrder" 
                                            CommandArgument='<%# Eval("OrderId") %>' 
                                            Visible='<%# Eval("Status").ToString() == "Shipped" %>'
                                            OnClientClick="return confirmAction('complete');" />
                                            
                                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" 
                                            CssClass="btn btn-cancel" CommandName="CancelOrder" 
                                            CommandArgument='<%# Eval("OrderId") %>' 
                                            Visible='<%# Eval("Status").ToString() == "Pending" || Eval("Status").ToString() == "Approved" %>'
                                            OnClientClick="return confirmAction('cancel');" />
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