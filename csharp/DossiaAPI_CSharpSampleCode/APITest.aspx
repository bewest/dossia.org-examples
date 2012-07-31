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
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="APITest.aspx.cs" Inherits="APITest" ValidateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Dossia API Test-List of API's</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>  
        <table >
            <tr align="left">
                <td>
                <img src="flash_temp.gif" alt="" />
                </td>
            </tr>
            <tr align="left">
                <td>
                    <strong><em>
                    <span style="color:Maroon">DOSSIA API's</span> </em></strong>
                </td>
            </tr>           
            <tr align="left">
                <td>&nbsp;<asp:Label ID="lblMessage" runat="server" Font-Bold="True" Font-Italic="True" Font-Names="Book Antiqua"
                        ForeColor="#C04000">Please select the following API</asp:Label></td>
            </tr>
            <tr>
                <td>
                </td>
            </tr>
            <tr align="left">
                <td>
                       <asp:LinkButton ID="lnkCreateDocument" runat="server" Text="1.New Document" OnClick="lnkNewCreateDocument_Click"></asp:LinkButton>
                </td>
            </tr>
            <tr align="left">
                <td style="height: 21px">
                    <asp:LinkButton ID="lnkCreateBinaryDocument" runat="server" 
                        Text="2.New Binary Document" OnClick="lnkCreateBinaryDocument_Click"></asp:LinkButton></td>
            </tr>
              <tr align="left">
                <td>
                    <asp:LinkButton ID="lnkaNewApplicationDocumentWithLabel" runat="server" 
                        Text="3.New Application Document with label " Width="323px" OnClick="lnkaNewApplicationDocumentWithLabel_Click"></asp:LinkButton></td>
            </tr>
            <tr align="left">
                <td>
                    <asp:LinkButton ID="lnkCreateAppDocument" runat="server" 
                        Text="4.New Application Document" OnClick="lnkCreateAppDocument_Click"></asp:LinkButton></td>
            </tr>
              <tr align="left">
                <td>
                    <asp:LinkButton ID="lnkRelatingExistingDocumentsAsParentAndChildDocuments" runat="server" 
                        Text="5.Relating existing documents as parent and child documents " Width="323px" OnClick="lnkRelatingExistingDocumentsAsParentAndChildDocuments_Click"></asp:LinkButton></td>
            </tr>
            <tr align="left">
                <td>
                       <asp:LinkButton ID="lnkRelateADocumentToaParentDocument" runat="server" Text="6.Relating a Document to a Parent Document" OnClick="lnkRelateADocumentToaParentDocument_Click"></asp:LinkButton>
                </td>
            </tr>
            <tr align="left">
                <td>
                       <asp:LinkButton ID="lnkReplaceDocumentWithAnotherDocument" runat="server" Text="7.Replacing a Document with Another Document" OnClick="lnkReplaceDocumentWithAnotherDocument_Click"></asp:LinkButton>
                </td>
            </tr>
            
            <tr>
                <td align="left" style="height: 21px">
                    <asp:LinkButton ID="lnkReplaceAppDocWithAnotherDoc" runat="server" 
                        Text="8.Replacing an Application Document with Another Document" OnClick="lnkReplaceAppDocWithAnotherDoc_Click"></asp:LinkButton></td>
            </tr>
             <tr align="left">
                <td>
                    <asp:LinkButton ID="lnkBtnDocumentSuppression" runat="server" 
                        Text="9.Document Suppression" Width="323px" OnClick="lnkBtnDocumentSuppression_Click"></asp:LinkButton></td>
            </tr>

             <tr align="left">
                <td>
                    <asp:LinkButton ID="lnkBtndUnsuppressedADocument" runat="server" 
                        Text="10.Unsuppressed a document " Width="323px" OnClick="lnkBtndUnsuppressedADocument_Click"></asp:LinkButton></td>
            </tr>
            <tr>
                <td align="left" style="height: 21px">
                    <asp:LinkButton ID="lnkDeleteDocument" runat="server" 
                        Text="11.Application Document Deletion" OnClick="lnkDeleteDocument_Click"></asp:LinkButton></td>
            </tr>
            <tr>
                <td align="left">
                    <asp:LinkButton ID="lnkRetriveAvailableRecords" runat="server" Text="12.Retriving available Records" OnClick="lnkRetriveAvailableRecords_Click"></asp:LinkButton>
                </td>
            </tr>
             <tr>
                <td align="left">
                    <asp:LinkButton ID="lnkRetriveRecord" runat="server" Text="13.Retrieving a Record" OnClick="lnkRetriveRecord_Click"></asp:LinkButton>
                </td>
            </tr> 
            <tr align="left">
                <td>
                    <asp:LinkButton ID="lnkRetriveDocumentRecords" runat="server" Text="14.Retrieving Root Level Documents Within a Record" OnClick="lnkRetriveDocumentRecords_Click"></asp:LinkButton>&nbsp;
                </td>
            </tr>    
             <tr align="left">
                <td>
                    <asp:LinkButton ID="lnkRetrivingSummaryCount" runat="server" Text="15.Retrieving Document Summary Counts" OnClick="lnkRetrivingSummaryCount_Click"></asp:LinkButton>&nbsp;
                </td>
            </tr>    
             <tr align="left">
                <td style="height: 21px">
                    <asp:LinkButton ID="lnkRetriveDocumentFromDocId" runat="server" Text="16.Retrieving a Document From a Document ID" OnClick="lnkRetriveDocumentFromDocId_Click"></asp:LinkButton>&nbsp;
                </td>
            </tr>    
            <tr align="left">
                <td style="height: 21px">
                    <asp:LinkButton ID="lnkRetriveBinaryDocument" runat="server" 
                        Text="17.Retrieving a Binary Document" OnClick="lnkRetriveBinaryDocument_Click"></asp:LinkButton></td>
            </tr>
            <tr align="left">
                <td>
                </td>
            </tr>
           <tr align="left">
                <td>
                    <asp:LinkButton ID="lnkMetaData" runat="server" Text="18.Meta Data" OnClick="lnkMetaData_Click"></asp:LinkButton>&nbsp;
                </td>
            </tr>
             <tr align="left">
                <td>
                    <asp:LinkButton ID="lnkBtnMetadataForAllRootLevelDocuments" runat="server" 
                        Text="19.Metadata for all root level documents " Width="323px" OnClick="lnkBtnMetadataForAllRootLevelDocuments_Click"></asp:LinkButton></td>
            </tr>
            <tr align="left">
                <td>
                    <asp:LinkButton ID="lnkMetaDataForAppDoc" runat="server" Text="20.Meta Data For Application Documents" OnClick="lnkMetaDataForAppDoc_Click"></asp:LinkButton></td>
            </tr>
            <tr align="left">
                <td>
                    <asp:LinkButton ID="lnkVersions" runat="server" Text="21.Versions" OnClick="lnkVersions_Click"></asp:LinkButton>&nbsp;
                </td>
            </tr>    
            <tr align="left">
                <td>
                    <asp:LinkButton ID="lnkSpecificDocumentVersion" runat="server" Text="22.Specific Document Version" OnClick="lnkSpecificDocumentVersion_Click"></asp:LinkButton>&nbsp;
                </td>
            </tr>
              <tr align="left">
                <td>
                    <asp:LinkButton ID="lnkBtnRetrievingParentDocument" runat="server" 
                        Text="23.Retrieving parent document" Width="323px" OnClick="lnkBtnRetrievingParentDocument_Click"></asp:LinkButton></td>
            </tr>
            <tr align="left">
                <td>
                    <asp:LinkButton ID="lnkRetriveCollectionOfdocByType" runat="server" Text="24.Retrieving a Collection of Documents by Type" OnClick="lnkRetriveCollectionOfdocByType_Click"></asp:LinkButton>&nbsp;
                </td>
            </tr>
              <tr align="left">
                <td>
                    <asp:LinkButton ID="lnkRetrievingApplicationDocByKeyId" runat="server" 
                        Text="25.Retrieving a Application document by Key id " OnClick="lnkRetrievingApplicationDocByKeyId_Click"></asp:LinkButton></td>
            </tr>
          
        </table>
    
    </div>
    </form>
</body>
</html>
