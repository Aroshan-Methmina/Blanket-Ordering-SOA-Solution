<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UOrder.aspx.cs" Inherits="CozyComfortClient.Seller.UOrder" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" href="../css/Style.css">
    <link href='https://unpkg.com/boxicons@2.0.9/css/boxicons.min.css' rel='stylesheet'>
    <title>User Order Management</title>
    <style>
        .status-pending { color: orange; }
        .status-approved { color: blue; }
        .status-cancelled { color: red; }
        
        .form-area {
            background: var(--light);
            padding: 24px;
            border-radius: 20px;
            margin-bottom: 24px;
        }
        
        .order-history {
            background: var(--light);
            padding: 24px;
            border-radius: 20px;
        }
        
        .order-history .head {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
        }
        
        .search-filter {
            width: 200px;
        }
        
        .btn-delete {
            background-color: var(--red);
            color: white;
        }
        
        .btn-delete:hover {
            background-color: #c82333;
        }
        
        .grid-view {
            width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
        }
        
        .grid-view th, .grid-view td {
            padding: 12px 15px;
            text-align: left;
            border-bottom: 1px solid #ddd;
        }
        
        .grid-header {
            background-color: var(--blue);
            color: white;
        }
        
        .grid-row:hover {
            background-color: #f5f5f5;
        }
        
        .grid-pager a, .grid-pager span {
            display: inline-block;
            padding: 5px 10px;
            margin: 0 2px;
            border: 1px solid #ddd;
        }
        
        .grid-pager a {
            color: var(--blue);
            text-decoration: none;
        }
        
        .grid-pager a:hover {
            background-color: #eee;
        }
        
        .grid-pager span {
            background-color: var(--blue);
            color: white;
            border-color: var(--blue);
        }
    </style>
    <script>
        function confirmCancel() {
            return confirm('Are you sure you want to cancel this order?');
        }

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
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <section id="sidebar">
            <a href="UOrder.aspx">
                <img src="../icon/Logo.png" style="height: 90px; margin-left: 80px;">
            </a>
            <a href="UOrder.aspx" class="brand">
                <span class="text" style="margin-left:10px;">Cozy Comfort Pvt Ltd</span>
            </a>
            <ul class="side-menu top">
                <li class="active">
                    <a href="UOrder.aspx">
                        <img src="../icon/uorder.svg" style="width: 20px; height: 20px; vertical-align: middle; margin-left: 5px; margin-right: 5px;" />
                        <span class="text">Get Orders</span>
                    </a>
                </li>
               
                <li>
                    <a href="SInventory.aspx">
                        <img src="../icon/inventory.svg" style="width: 20px; height: 20px; vertical-align: middle; margin-left: 5px; margin-right: 5px;" />
                        <span class="text">Inventory</span>
                    </a>
                </li>
                <li>
                    <a href="SOrder.aspx">
                        <img src="../icon/order.svg" style="width: 20px; height: 20px; vertical-align: middle; margin-left: 5px; margin-right: 5px;" />
                        <span class="text">Order</span>
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
                <i class='bx bx-menu'></i>
                <a href="UOrder.aspx" class="nav-link">Sellers</a>
            </nav>

            <main>
                <div class="head-title">
                    <div class="left">
                        <h1>User Order Management</h1>
                    </div>
                </div>

                <div class="form-area">
                    <h2>Place New Order</h2>
                    <div class="form-group">
                        <label>User Name</label>
                        <asp:TextBox ID="txtUserName" CssClass="form-control" runat="server" />
                    </div>

                    <div class="form-group">
                        <label>User Contact</label>
                        <asp:TextBox ID="txtUserContact" CssClass="form-control" runat="server" />
                    </div>

                    <div class="form-group">
                        <label>Blanket</label>
                        <asp:DropDownList ID="ddlBlanket" CssClass="form-control" runat="server" />
                    </div>

                    <div class="form-group">
                        <label>Quantity</label>
                        <asp:TextBox ID="txtQuantity" CssClass="form-control" runat="server" TextMode="Number" />
                    </div>

                    <div class="form-group">
                        <label>Expected Delivery Date</label>
                        <asp:TextBox ID="txtExpectedDate" CssClass="form-control" runat="server" TextMode="Date" />
                    </div>

                    <div class="form-actions">
                        <asp:Button ID="btnPlaceOrder" CssClass="btn btn-primary" runat="server" Text="Place Order" OnClick="btnPlaceOrder_Click" />
                    </div>
                    
                    <asp:Label ID="lblMessage" runat="server" Visible="false" CssClass="message-label" style="margin-top: 10px; display: block;"></asp:Label>
                </div>

                <div class="order-history">
                    <div class="head">
                        <h3>Order History</h3>
                        <div class="search-filter">
                            <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlStatusFilter_SelectedIndexChanged">
                                <asp:ListItem Value="">All</asp:ListItem>
                                <asp:ListItem Value="Pending">Pending</asp:ListItem>
                                <asp:ListItem Value="Approved">Approved</asp:ListItem>
                                <asp:ListItem Value="Cancelled">Cancelled</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>

                    <asp:GridView ID="gvOrders" runat="server" AutoGenerateColumns="False" CssClass="grid-view" 
                        AllowPaging="true" PageSize="5" OnRowCommand="gvOrders_RowCommand" 
                        OnPageIndexChanging="gvOrders_PageIndexChanging">
                        <Columns>
                            <asp:BoundField DataField="OrderId" HeaderText="Order ID" />
                            <asp:BoundField DataField="UserName" HeaderText="User Name" />
                            <asp:BoundField DataField="UserContact" HeaderText="Contact" />
                            <asp:TemplateField HeaderText="Blanket Model">
                                <ItemTemplate>
                                    <%# GetModelName(Eval("BlanketId")) %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Quantity" HeaderText="Qty" />
                            <asp:BoundField DataField="Status" HeaderText="Status" />
                            <asp:BoundField DataField="OrderDate" HeaderText="Order Date" DataFormatString="{0:yyyy-MM-dd}" />
                            <asp:BoundField DataField="ExpectedDeliveryDate" HeaderText="Expected Date" DataFormatString="{0:yyyy-MM-dd}" />
                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <asp:Button ID="btnCancelOrder" runat="server" Text="Cancel" CssClass="btn btn-delete"
                                        CommandName="CancelOrder" CommandArgument='<%# Eval("OrderId") %>'
                                        Visible='<%# Eval("Status").ToString() == "Pending" %>'
                                        OnClientClick="return confirmCancel();" />

                                    <asp:Button ID="btnNextStatus" runat="server" CssClass="btn btn-primary"
                                        CommandName="NextStatus" CommandArgument='<%# Eval("OrderId") + "|" + Eval("Status") %>'
                                        Text='<%# GetNextStatusText(Eval("Status").ToString()) %>'
                                        Visible='<%# ShowNextStatusButton(Eval("Status").ToString()) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <PagerStyle CssClass="grid-pager" />
                        <HeaderStyle CssClass="grid-header" />
                        <RowStyle CssClass="grid-row" />
                        <AlternatingRowStyle CssClass="grid-alt-row" />
                    </asp:GridView>
                </div>
            </main>
        </section>
    </form>
</body>
</html>