<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MDOrder.aspx.cs" Inherits="CozyComfortClient.Manufacturer.MDOrder" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Distributor Order Management</title>
    <link rel="stylesheet" href="../css/Style.css" />
    <link href="https://unpkg.com/boxicons@2.0.9/css/boxicons.min.css" rel="stylesheet" />
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
                 <li class="active">
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
                        <h1>Distributor Order Management</h1>
                        <ul class="breadcrumb">
                            
                        </ul>
                    </div>
                </div>

                <div class="table-data" style="margin-top: 30px;">
                    <div class="order">
                        <div class="head">
                            <h3>Distributor Orders</h3>
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

                        <asp:Label ID="lblMessage" runat="server" Visible="false" CssClass="message-label"></asp:Label>

                            <asp:GridView ID="gvDistributorOrders" runat="server" AutoGenerateColumns="False"
                                CssClass="grid-view" DataKeyNames="OrderId"
                                OnRowCommand="gvDistributorOrders_RowCommand"
                                OnRowDataBound="gvDistributorOrders_RowDataBound"
                                AllowPaging="True" PageSize="5"
                                OnPageIndexChanging="gvDistributorOrders_PageIndexChanging">
    
                                <Columns>
                                    <asp:BoundField DataField="OrderId" HeaderText="Order ID" />
                                    <asp:BoundField DataField="BlanketModel" HeaderText="Blanket Model" />
                                    <asp:BoundField DataField="DistributorName" HeaderText="Distributor Name" />
                                    <asp:BoundField DataField="Quantity" HeaderText="Order Qty" />
                                   
                                    <asp:TemplateField HeaderText="Available Qty">
                                        <ItemTemplate>
                                            <asp:Label ID="lblAvailableQty" runat="server" Text='<%# GetAvailableQuantity(Convert.ToInt32(Eval("ManufacturerId")), Convert.ToInt32(Eval("BlanketId"))) %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Status">
                                        <ItemTemplate>
                                            <asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Status") %>' CssClass='<%# "status-" + Eval("Status").ToString().ToLower() %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="OrderDate" HeaderText="Order Date" DataFormatString="{0:g}" />
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:Button ID="btnApprove" runat="server" Text="Approve" CommandName="ApproveOrder"
                                            CommandArgument='<%# Eval("OrderId") %>' CssClass="btn btn-approve"
                                            Visible='<%# Eval("Status").ToString() == "Pending" %>' />
                                        <asp:Button ID="btnMarkShipped" runat="server" Text="Ship" CommandName="ShipOrder"
                                            CommandArgument='<%# Eval("OrderId") %>' CssClass="btn btn-ship"
                                            Visible='<%# Eval("Status").ToString() == "Approved" %>' />
                                        <asp:Button ID="btnComplete" runat="server" Text="Complete" CommandName="CompleteOrder"
                                            CommandArgument='<%# Eval("OrderId") %>' CssClass="btn btn-complete"
                                            Visible='<%# Eval("Status").ToString() == "Shipped" %>' />
                                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CommandName="CancelOrder"
                                            CommandArgument='<%# Eval("OrderId") %>' CssClass="btn btn-cancel"
                                            Visible='<%# Eval("Status").ToString() == "Pending" || Eval("Status").ToString() == "Approved" %>' />
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