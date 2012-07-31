<%@ Page Language="C#" AutoEventWireup="true" CodeFile="RetrievingParentDocument.aspx.cs" Inherits="RetrievingParentDocument" ValidateRequest="false"  %>

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
                <td style="color: maroon" colspan="2"> 
                  Retrieving parent document
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:LinkButton ID="lnkGoToHome" runat="Server" Text="Go to  Home" OnClick="lnkGoToHome_Click"></asp:LinkButton>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Label ID="lblMessage" runat="server" ForeColor="red"></asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    <label>Record Id</label></td><td>
                    <asp:TextBox ID="txtRecordId" runat="server" Width="190px"></asp:TextBox><span style="color:Red">*</span>
                </td>
            </tr>    
             <tr>
                <td >
                    <label>Document Id</label></td><td>
                    <asp:TextBox ID="txtDocumentId" runat="server" Width="188px" ></asp:TextBox><span style="color:Red">*</span>
                </td>
            </tr>   
            <tr>
                    <td colspan="2">
                        <asp:Button ID="btnGetResponse" runat="server" Text="Get Response" OnClick="btnGetResponse_Click" />
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
