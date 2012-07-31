<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EsLogin.aspx.cs" Inherits="openidrp.EsLogin" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <br />
        <asp:Label ID="HeaderLabel" runat="server" 
            Text="Enter an OpenID (for example https://webui1.dossia.org/authserver)"></asp:Label>
        <br />
        <asp:TextBox ID="openIDTextBox" runat="server" Width="389px">https://webui1.dossia.org/authserver</asp:TextBox>
        <br />
        <br />
        <asp:Label ID="securityRealmLabel" runat="server" 
            Text="The Security Realm must be a parent url of the return URL, and must be registered with Dossia"></asp:Label>
        <br />
        <asp:TextBox ID="securityRealmTextBox" runat="server" Width="392px">http://localhost:8080/openidrp</asp:TextBox>
        <br />
    
    </div>
    <asp:Button ID="submitButton" runat="server" OnClick="submitButton_OnClick" Text="Submit" />
    </form>
</body>
</html>
