<%@ Page Language="C#" AutoEventWireup="true" CodeFile="NewApplicationDocumentwithlabel.aspx.cs" Inherits="NewApplicationDocumentwithlabel_" ValidateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
     <form id="form1" runat="server">
    <div> 
    <table>
     <tr>
         <td style="color: maroon; height: 21px;" colspan="2">
           Create a new Application document with Label</td>
    </tr>
    <tr>
        <td colspan="2">
              <asp:LinkButton ID="lnkGoToHome" runat="Server" Text="Go to  Home" OnClick="lnkGoToHome_Click"></asp:LinkButton>
       </td>
    </tr>
     <tr>
        <td colspan="2">
                <asp:Label ID="lblMessage" runat="server" ForeColor="red" Visible="false"></asp:Label>
        </td>
    </tr>
    <tr>
        <td>
            <label>Record Id</label></TD><td>
            <asp:TextBox ID="txtRecordId" runat="server" Width="202px"></asp:TextBox><span style="color:Red">*</span>
        </td>
    </tr>
    <tr>
        <td>
            <label>Key Id</label></tD><td>
            <asp:TextBox ID="txtKeyId" runat="server" Width="204px"></asp:TextBox><span style="color:Red">*</span>
        </td>
    </tr>
        <tr>
        <td>
            <label>Label Id</label></tD><td>
            <asp:TextBox ID="txtLabelId" runat="server" Width="204px"></asp:TextBox><span style="color:Red">*</span>
        </td>
    </tr>

    <tr>
        <td>Upload file</td>
        <td>
            &nbsp;<asp:FileUpload ID="fileUploader" runat="server" /><span style="color:Red">*</span></td>
    </tr>
    <tr>
        <td colspan="2">
            <asp:Button ID="btnPost" runat="server" Text="POST" OnClick="btnPost_Click" />            
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
