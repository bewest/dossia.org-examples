<%@ Page Language="C#" AutoEventWireup="true" CodeFile="RetrieveAppDocByKeyId.aspx.cs" Inherits="RetrieveAppDocByKeyId" ValidateRequest="false"  %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Retrieving a Application document by key id</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <table>
              <tr>
                <td style="color: maroon" colspan="2">
                    Retriveing Application document by key id
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:LinkButton ID="lnkGoToHome" runat="Server" Text="Go to  Home" OnClick="lnkGoToHome_Click"></asp:LinkButton>
                </td>
            </tr>
          
            <tr>
                <td colspan="2">
                    <asp:Label id="lblMessage" runat="server" ForeColor="red" Visible="false"></asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                <label>Record Id</label></td><td>
                    <asp:TextBox ID="txtRecordId" runat="server" Width="209px"></asp:TextBox> <span style="color: red">*</span>
                </td>
            </tr>
            <tr>
                <td>
                <label>Key Id</label></td><td>
                    <asp:TextBox ID="txtKeyId" runat="server" Width="212px"></asp:TextBox> <span style="color: red">*</span>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Button ID="btnRetrive" runat="server" Text="Retrive  AppDoc by key Id" OnClick="btnRetrive_Click" /></td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:TextBox ID="txtDossiaAPIResponse" runat="server" Columns="60" Rows="25" TextMode="MultiLine"></asp:TextBox>
               </td> 
            </tr>
        </table>
    </div>
    </form>
</body>
</html>
