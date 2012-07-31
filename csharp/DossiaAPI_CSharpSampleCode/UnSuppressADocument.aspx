<%@ Page Language="C#" AutoEventWireup="true" CodeFile="UnSuppressADocument.aspx.cs" Inherits="UnSuppressADocument" ValidateRequest="false"  %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>UnSuppress a document</title>
</head>
<body>
    <form id="form1" runat="server" >
    <div>
    <table>
             <tr>
                <td style="color: maroon; height: 15px;" colspan="2" >
                    <span class="mw-headline">Un Supress Document</span>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:LinkButton ID="lnkGoToHome" runat="Server" Text="Go to  Home" OnClick="lnkGoToHome_Click"></asp:LinkButton>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Label ID="lblMessage" runat="server" ForeColor="Red" Visible="false"></asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    <label>Record Id</label></td><td>
                    <asp:TextBox ID="txtRecordId" runat="server" Width="222px"></asp:TextBox><span style="color:Red">*</span>
                </td>
            </tr>
            <tr>
                <td>
                    <label>Document Id</label></td><td>
                    <asp:TextBox ID="txtDocumentId" runat="server" Width="222px"></asp:TextBox><span style="color:Red">*</span>
                </td>
            </tr> 
            <tr>
                <td colspan="2">
                    <asp:Button ID="btnUnSuppress" runat="server" Text="Un Suppress Document" OnClick="btnSuppress_Click" />
                </td>
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
