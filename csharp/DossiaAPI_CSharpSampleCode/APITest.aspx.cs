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

using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using System.Net;
using System.Xml;
using System.IO;
using DossiaAPITest;

public partial class APITest : System.Web.UI.Page
{
    oAuthDossia oAuth = null;
    protected void Page_Load(object sender, EventArgs e)
    {
    }

    #region Post Events
    protected void lnkNewCreateDocument_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/CreateNewDocument.aspx", false);
    }

    protected void lnkCreateBinaryDocument_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/CreateNewBinaryDocument.aspx", false);
    }

    protected void lnkCreateAppDocument_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/CreateNewAppDocument.aspx", false);
    }

    protected void lnkRelateADocumentToaParentDocument_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/RelateDocToParentDoc.aspx", false);
    }

    protected void lnkReplaceDocumentWithAnotherDocument_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/ReplaceDocWithAnotherDoc.aspx", false);
    }

    protected void lnkReplaceAppDocWithAnotherDoc_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/ReplaceAppDocWithAnotherDoc.aspx", false);
    }

    protected void lnkaNewApplicationDocumentWithLabel_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/NewApplicationDocumentwithlabel.aspx", false);
    }

    protected void lnkRelatingExistingDocumentsAsParentAndChildDocuments_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/RelatingExistingDocumentsAsParenAndChildDocuments.aspx", false);
    }

    protected void lnkBtndUnsuppressedADocument_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/UnSuppressADocument.aspx", false);
    }
    #endregion

    #region GET Events
    protected void lnkRetriveDocumentRecords_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
       Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/RetriveDocumentWithinRecord.aspx", false);
    }

    protected void lnkRetriveAvailableRecords_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
       Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/RetriveAvailableRecords.aspx", false);
    }

    protected void lnkRetriveRecord_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/RetriveRecord.aspx", false);
    }

    protected void lnkRetrivingSummaryCount_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/RetrivingDocSummaryCount.aspx", false);
    }

    protected void lnkRetriveDocumentFromDocId_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/RetriveDocFromDocID.aspx", false);
    }

    protected void lnkRetriveBinaryDocument_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/RetrieveBinaryDocument.aspx", false);
    }

    protected void lnkMetaData_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/MetaData.aspx", false);
    }

    protected void lnkMetaDataForAppDoc_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/MetaDataAppDocument.aspx", false);
    }

    protected void lnkVersions_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/Versions.aspx", false);
    }

    protected void lnkSpecificDocumentVersion_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/SpecificDocumentVersion.aspx", false);
    }

    protected void lnkRetriveCollectionOfdocByType_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/RetriveCollectionDocumentsByType.aspx", false);
    }


    protected void lnkRetrievingApplicationDocByLabelId_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/RetrieveAppDocByLabelId.aspx", false);
    }

    protected void lnkRetrievingApplicationDocByKeyId_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/RetrieveAppDocByKeyId.aspx", false);
    }

    protected void lnkRetrievingApplicationDocByDocId_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/RetrieveAppDocByDocId.aspx", false);
    }

    protected void lnkBtnMetadataForAllRootLevelDocuments_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/MetadataforAllRooLlevelDocuments.aspx", false);
    }

    protected void lnkBtnRetrievingParentDocument_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/RetrievingParentDocument.aspx", false);
    }

    #endregion

    #region DELETE EVENTS
    protected void lnkBtnDocumentSuppression_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/DocumentSuppression.aspx", false);
    }

    protected void lnkDeleteDocument_Click(object sender, EventArgs e)
    {
        oAuth = new oAuthDossia();
        Response.Redirect(oAuth.ExtractBaseURLFromURL().ToString() + "/ApplicationDocumentDeletion.aspx", false);
    }
    #endregion
  
   

}
