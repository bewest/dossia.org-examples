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
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="RetriveCollectionDocumentsByType.aspx.cs" Inherits="RetriveCollectionDocumentsByType" ValidateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title> Retrieving a Collection of Documents by Type</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <table>
             <tr>
                <td style="color: maroon"  colspan="2">
                    Retrieving a Collection of Documents by Type
                </td>
            </tr>
            <tr>
                <td  colspan="2">
                    <asp:LinkButton ID="lnkGoToHome" runat="Server" Text="Go to  Home" OnClick="lnkGoToHome_Click"></asp:LinkButton>
                </td>
            </tr>
            <tr>
                <td  colspan="2">
                    <asp:Label id="lblMessage" runat="server" ForeColor="red" Visible="false"></asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                <label>Record Id</label></td><td>
                    <asp:TextBox ID="txtRecordId" runat="server" Width="221px"></asp:TextBox> <span style="color: red">*</span>
                </td>
            </tr>
            <tr>
                <td><label>document type</label></td><td>
                    <asp:DropDownList ID="ddlDocumentType" runat="server" Width="225px">
                    <asp:ListItem Text="Encounter" Value="Encounter"></asp:ListItem>
                     <asp:ListItem Text="Allergy" Value="Allergy"></asp:ListItem>
                     <asp:ListItem Text="Annotation" Value="Annotation"></asp:ListItem>
                     <asp:ListItem Text="Appointment" Value="Appointment"></asp:ListItem>
                     <asp:ListItem Text="Association" Value="Association"></asp:ListItem>
                     <asp:ListItem Text="BinaryData" Value="BinaryData"></asp:ListItem>
                     <asp:ListItem Text="Dental" Value="Dental"></asp:ListItem>
                     <asp:ListItem Text="FamilyHistory" Value="FamilyHistory"></asp:ListItem>
                     <asp:ListItem Text="Equipment" Value="Equipment"></asp:ListItem>
                     <asp:ListItem Text="Immunization" Value="Immunization"></asp:ListItem>
                     <asp:ListItem Text="Insurance" Value="Insurance"></asp:ListItem>
                     <asp:ListItem Text="LabTest" Value="LabTest"></asp:ListItem>
                     <asp:ListItem Text="Measurement" Value="Measurement"></asp:ListItem>
                     <asp:ListItem Text="Medication" Value="Medication"></asp:ListItem>
                     <asp:ListItem Text="Micro" Value="Micro"></asp:ListItem>
                     <asp:ListItem Text="Problem" Value="Problem"></asp:ListItem>
                     <asp:ListItem Text="Procedure" Value="Procedure"></asp:ListItem>
                  </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td  colspan="2">
                    <asp:Button ID="btnRetrive" runat="server" Text="Retrive" OnClick="btnRetrive_Click" />
                </td>
            </tr>
            <tr>
                <td  colspan="2">
                    <asp:TextBox ID="txtDossiaAPIResponse" runat="server" Columns="60" Rows="25" TextMode="MultiLine"></asp:TextBox>
               </td> 
            </tr>
        </table>
    </div>
    </form>
</body>
</html>
