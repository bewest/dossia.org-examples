   <%--
   
/*
 * Copyright 2009 Dossia
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
    
   
   --%>
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="RelateDocToParentDoc.aspx.cs" Inherits="RelateDocToParentDoc" ValidateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Relating a Document to a Parent Document</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <table>
     <tr>
         <td style="color: maroon"  colspan="2">
          Relating a Document to a Parent Document
         </td>
    </tr>
    <tr>
        <td  colspan="2">
              <asp:LinkButton ID="lnkGoToHome" runat="Server" Text="Go to  Home" OnClick="lnkGoToHome_Click"></asp:LinkButton>
       </td>
    </tr>
    <tr>
        <td  colspan="2">
                <asp:Label ID="lblMessage" runat="server" ForeColor="Red" Visible="false"></asp:Label>
        </td>
    </tr>
    <tr>
        <td>
            <label>Record Id</label></td><td>
            <asp:TextBox ID="txtRecordId" runat="server" Width="184px"></asp:TextBox>
            <span style="color: red">*</span></td>
    </tr>
    <tr>
        <td>
            <label>Document Id</label></tD><td>
            <asp:TextBox ID="txtDocumentId" runat="server" Width="183px"></asp:TextBox>
            <span style="color: red">*</span></td>
    </tr>
   
    <tr>
        <td valign="top">XML Request</td>
        <td>
             <asp:TextBox ID="txtDossiaAPIInputRequest" runat="server" Columns="60" Rows="25" TextMode="MultiLine"></asp:TextBox>
        </td>
    </tr>
    <tr>
        <td  colspan="2">
            <asp:Button ID="btnPost" runat="server" Text="POST" OnClick="btnPost_Click" />            
        </td>
    </tr>
    <tr>
        <td  colspan="2">                
            <asp:TextBox ID="txtDossiaAPIResponse" runat="server" Columns="60" Rows="25" TextMode="MultiLine" Width="582px"></asp:TextBox>
       </td> 
    </tr>
    </table>
    </div>
    </form>
</body>
</html>
