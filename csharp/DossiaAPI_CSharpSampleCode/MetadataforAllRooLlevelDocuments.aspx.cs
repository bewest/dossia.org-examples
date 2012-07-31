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


public partial class MetadataforAllRooLlevelDocuments : System.Web.UI.Page
{
    oAuthDossia oAuth = null;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.QueryString["oauth_token"] == null)
        {
            oAuth = new oAuthDossia();
            Response.Redirect(oAuth.GetTokens());
        }
    }

    /// <summary>
    /// Method to clear the session
    /// </summary>
    private void clearSession()
    {
        Session[oAuthDossia.SESSION_TOKEN] = "";
        Session[oAuthDossia.SESSION_TOKEN_SECRET] = "";
    }

    protected void lnkGoToHome_Click(object sender, EventArgs e)
    {
        Response.Redirect("APITest.aspx", false);
        clearSession();
    }
    protected void btnGetResponse_Click(object sender, EventArgs e)
    {
        if (txtRecordId.Text.Length == 0)
        {
            lblMessage.Text = "Please enter record id.";
            return;
        }
        string accessTokenRedirectUrl = string.Empty;
        string apiGetUrl = string.Empty;
        string getRecordUrl = string.Empty;
        string responseText = string.Empty;
        string recordId = txtRecordId.Text;
        oAuth = new oAuthDossia();
        //Authorize and use the set the token and secret in session and get the new access token
        oAuth.AccessTokenGet();
        if (oAuth.Token.Length > 0 && oAuth.TokenSecret.Length > 0)
        {
            try
            {
                //generate the API url
                apiGetUrl = oAuthDossia.DOSSIA_API_URL + "records/" + recordId + "/documents/meta";
                //get the access to API url
                accessTokenRedirectUrl = oAuth.AccessToAPILink(apiGetUrl, oAuth.Token, oAuth.TokenSecret, oAuthDossia.Method.GET.ToString());
                //do the request and set the response to text
                responseText = oAuth.MakeWebRequest(accessTokenRedirectUrl);
                this.txtDossiaAPIResponse.Text = responseText;
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
}
