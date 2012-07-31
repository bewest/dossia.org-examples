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
using DossiaAPITest;

public partial class ReplaceDocWithAnotherDoc : System.Web.UI.Page
{

    oAuthDossia oAuth = null;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.QueryString["oauth_token"] == null)
        {
            if (!Page.IsPostBack)
            {
                oAuth = new oAuthDossia();
                Response.Redirect(oAuth.GetTokens());
            }
        }
    }

    protected void btnPost_Click(object sender, EventArgs e)
    {
        if (txtDossiaAPIInputRequest.Text.Length == 0)
        {
            lblMessage.Text = "Please enter the Input";
            lblMessage.Visible = true;
            return;
        }
        if (txtDocumentId.Text.Length == 0)
        {
            lblMessage.Text = "please enter documentId.";
            lblMessage.Visible = true;
            return;
        }
        if (txtRecordId.Text.Length == 0)
        {
            lblMessage.Text = "please enter recordId.";
            lblMessage.Visible = true;
            return;
        }
        string accessTokenRedirectUrl = string.Empty;        
        string apiPostUrl = string.Empty;
        string responseText = string.Empty;
        string recordId = txtRecordId.Text;
        string documentId = txtDocumentId.Text;
        oAuth = new oAuthDossia();
        //Authorize and use the token and secret that is set above and create new token
        oAuth.AccessTokenGet();
        //check the lenght is greather then 0
        if (oAuth.Token.Length > 0 && oAuth.TokenSecret.Length > 0)
        {
            try
            {               
                //set the request API url
                apiPostUrl = oAuthDossia.DOSSIA_API_URL + "records/" + recordId + "/documents/" + documentId + "/replace";
                //get the access token url
                accessTokenRedirectUrl = oAuth.AccessToAPILink(apiPostUrl, oAuth.Token, oAuth.TokenSecret, oAuthDossia.Method.POST.ToString());
                //do the request and get the response,If any error is there it will throw the error
                responseText = oAuth.MakePostRequest(accessTokenRedirectUrl, txtDossiaAPIInputRequest.Text);
                //set the response value
                txtDossiaAPIResponse.Text = responseText;
                //clear session
                clearSession();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        else
        {
            Response.Redirect("APITest.aspx", false);
        }
    }

    protected void lnkGoToHome_Click(object sender, EventArgs e)
    {
        Response.Redirect("APITest.aspx", false);
        clearSession();
    }

    /// <summary>
    /// Method to clear the session
    /// </summary>
    private void clearSession()
    {
        Session[oAuthDossia.SESSION_TOKEN] = "";
        Session[oAuthDossia.SESSION_TOKEN_SECRET] = "";
    }
}
