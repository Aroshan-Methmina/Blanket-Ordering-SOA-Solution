<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="CozyComfortClient.Account.Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login</title>
    <style>
        body {
            margin: 7%;
            background-color: #1a73a0;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }

        .container {
            width: 45%;
            min-width: 350px;
            max-width: 500px;
            background-color: white;
            border: 1px solid rgb(235, 235, 235);
            border-radius: 8px;
            margin: 0 auto;
            padding: 30px;
            box-shadow: 0 7px 9px 0 rgba(44, 43, 43, 0.3);
            animation: transitionIn-Y-over 0.5s;
        }

        @keyframes transitionIn-Y-over {
            0% {
                transform: translateY(-20px);
                opacity: 0;
            }
            100% {
                transform: translateY(0);
                opacity: 1;
            }
        }

        td {
            text-align: center;
        }

        .header-text {
            font-weight: 600;
            font-size: 30px;
            letter-spacing: 1px;
            margin-bottom: 10px;
            color: black;
        }

        .sub-text {
            font-size: 15px;
            color: rgb(138, 138, 138);
            margin-bottom: 25px;
        }

        .form-label {
            color: rgb(44, 44, 44);
            text-align: left;
            font-size: 14px;
            font-weight: 500;
            display: block;
            margin-bottom: 5px;
        }

        .label-td {
            text-align: left;
            padding-top: 15px;
        }

        .input-text {
            width: 100%;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 4px;
            font-size: 14px;
            box-sizing: border-box;
            transition: border 0.3s;
        }

        .input-text:focus {
            border-color: #0A76D8;
            outline: none;
            box-shadow: 0 0 0 2px rgba(10, 118, 216, 0.2);
        }

        .hover-link1 {
            font-weight: bold;
            text-decoration: none;
            color: #0A76D8;
            font-size: 14px;
        }

        .hover-link1:hover {
            opacity: 0.8;
            transition: 0.3s;
            text-decoration: underline;
        }

        .login-btn {
            margin-top: 20px;
            margin-bottom: 15px;
            width: 100%;
            padding: 12px;
            background-color: #0A76D8;
            color: white;
            border: none;
            border-radius: 4px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: background-color 0.3s;
        }

        .login-btn:hover {
            background-color: #095cb2;
        }

        .error-message {
            display: block;
            color: #d9534f;
            background-color: #f8d7da;
            border: 1px solid #f5c6cb;
            padding: 10px;
            border-radius: 4px;
            margin: 10px 0;
            font-size: 14px;
            text-align: center;
        }

        .footer-links {
            margin-top: 20px;
            text-align: center;
            font-size: 14px;
        }

        .footer-links a {
            margin: 0 5px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <table style="width: 100%;">
                <tr>
                    <td colspan="2">
                        <p class="header-text">Welcome Back!</p>
                        <p class="sub-text">Login with your details to continue</p>
                    </td>
                </tr>
                
               
                <tr id="errorRow" runat="server" visible="false">
                    <td colspan="2"><asp:Label ID="lblErrorMessage" runat="server" CssClass="error-message" ForeColor="Red"></asp:Label> <asp:Label ID="lblError" runat="server" CssClass="error-message"></asp:Label>
                    </td>
                </tr>
                
                <tr>
                    <td class="label-td" colspan="2">
                        <asp:Label ID="Label1" runat="server" Text="Email Address" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="input-text" TextMode="Email" 
                            Placeholder="Enter your email address" required></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvEmail" runat="server" 
                            ControlToValidate="txtEmail" ErrorMessage="Email is required" 
                            Display="Dynamic" ForeColor="Red" CssClass="error-message"></asp:RequiredFieldValidator>
                    </td>
                </tr>
                <tr>
                    <td class="label-td" colspan="2">
                        <asp:Label ID="Label2" runat="server" Text="Password" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="input-text" TextMode="Password" 
                            Placeholder="Enter your password" required></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvPassword" runat="server" 
                            ControlToValidate="txtPassword" ErrorMessage="Password is required" 
                            Display="Dynamic" ForeColor="Red" CssClass="error-message"></asp:RequiredFieldValidator>
                    </td>
                </tr>
               
                <tr>
                    <td colspan="2">
                        <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="login-btn" OnClick="btnLogin_Click" />
                    </td>
                </tr>
               
            </table>
        </div>
    </form>
</body>
</html>
