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
using System.IO;

public partial class RetrieveAppDocByLabelId : System.Web.UI.Page
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
    protected void btnRetrive_Click(object sender, EventArgs e)
    {
        string accessTokenRedirectUrl = string.Empty;
        string apiGetUrl;
        string getRecordUrl = string.Empty;
        string responseText = string.Empty;

        if (txtRecordId.Text.Length < 0)
        {
            lblMessage.Text = "Please enter record id.";
            return;
        }
        if (txtLabelId.Text.Length < 0)
        {
            lblMessage.Text = "Please enter label id.";
            return;
        }
        oAuth = new oAuthDossia();

        //Authorize and use the token and secret to get the new access token
        oAuth.AccessTokenGet();
        string recordId = txtRecordId.Text;
        string labelId = txtLabelId.Text;
        if (oAuth.Token.Length > 0 && oAuth.TokenSecret.Length > 0)
        {
            try
            {
                string contentType;
                byte[] outputData;

                //Get the App path
                string physicalPath = HttpContext.Current.Request.MapPath(HttpContext.Current.Request.ApplicationPath);

                //generate the API url
                apiGetUrl = oAuthDossia.DOSSIA_API_URL + "records/" + recordId + "/apps/documents/label/" + labelId;
                //get the access to API url
                accessTokenRedirectUrl = oAuth.AccessToAPILink(apiGetUrl, oAuth.Token, oAuth.TokenSecret, oAuthDossia.Method.GET.ToString());
                //do the request and set the response to text
                responseText = oAuth.MakeWebRequest(accessTokenRedirectUrl, out contentType, out outputData);

                //Save the reponse data in file stream
                FileStream fs = new FileStream(physicalPath + @"\output." + contentType.Split('/')[1], FileMode.Create, FileAccess.ReadWrite);
                fs.Write(outputData, 0, outputData.Length);
                fs.Close();

                this.txtDossiaAPIResponse.Text = "Binary file is saved at: " + fs.Name;
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
