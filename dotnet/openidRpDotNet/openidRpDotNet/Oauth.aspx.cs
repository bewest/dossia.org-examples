using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using ExtremeSwank.OAuth;
using openidrp.Code;
using System.Net;
using System.IO;
using ExtremeSwank.OpenId;
using ExtremeSwank.OpenId.PlugIns.Extensions;

namespace openidrp
{
    public partial class Oauth : System.Web.UI.Page
    {
        public String openID = null;
        public AccessToken accessToken = null;
        public String restUrl;
        public String restResponse;

        protected void Page_Load(object sender, EventArgs e)
        {
            LoginInfo loginInfo = (LoginInfo)Session["loginInfo"];
            if (loginInfo != null)
            {
                openID = loginInfo.openID;
                accessToken = loginInfo.accessToken;
            }
            else
            {
                openID = null;
                accessToken = null;
            }
            restUrl = Request["restUrl"];
            if (restUrl == null || "".Equals(restUrl.Trim()))
            {
                restUrl = ConfigurationManager.AppSettings["rest.api.url"];
            }
            String restCallSubmit = Request["OauthSubmit"];
            if (restCallSubmit != null && !"".Equals(restCallSubmit.Trim()))
            {
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(new Uri(restUrl));
                wr.Method = "GET";
                OAuthClient oac = DossiaOauth.createDossiaOAuthClient();
                wr.Credentials = oac.GetCredentials(accessToken, null);
                wr.AllowAutoRedirect = false;
                WebResponse response = wr.GetResponse();
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    restResponse = sr.ReadToEnd();
                    restResponse = Server.HtmlEncode(restResponse);
                }
            }
            String ssoSubmit = Request["SsoSubmit"];
            if (ssoSubmit != null && !"".Equals(ssoSubmit.Trim()))
            {
                String ssoRequestTokenUrl = ConfigurationManager.AppSettings["authserver.sso.token.url"];
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(new Uri(ssoRequestTokenUrl));
                wr.Method = "GET";
                OAuthClient oac = DossiaOauth.createDossiaOAuthClient();
                wr.Credentials = oac.GetCredentials(accessToken, null);
                wr.AllowAutoRedirect = false;
                WebResponse response = wr.GetResponse();
                String token;
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    token = sr.ReadToEnd();
                }


                // ok now we have a token, let's create a checkid_setup request. 
                // The token is valid for two minutes, and can only be used once.
                // Further, Dossia requires that the openid you setup in the
                // checkid_setup request matches the user who has requested the token.
                // Further, there must be a session active for this user on the authserver.
                // Sessions can be kept alive using the url
                // ${AUTHSERVER_BASE_URL}/authserver/

                DossiaOpenID dossiaOpenID = new DossiaOpenID(Application, Session);
                OpenIdClient openIdClient = dossiaOpenID.CreateOpenIdClient();

                openIdClient.Identity = loginInfo.openID;
                AttributeExchange attributeExchange = new AttributeExchange(openIdClient);
                attributeExchange.Mode = AttributeExchangeMode.Store;
                attributeExchange.AddStoreItem("http://openid.dossia.org/sso/ssoToken", "ssoToken", token);

                // Read the arguments in the current request and
                // automatically validate any OpenID responses,
                // firing events when actions occur.
                openIdClient.ValidationSucceeded += new EventHandler(openid_ValidationSucceeded);
                openIdClient.ValidationFailed += new EventHandler(openid_ValidationFailed);
                openIdClient.ReceivedCancel += new EventHandler(openid_ReceivedCancel);
                // Subscribing to SetupNeeded is only needed if using immediate authentication
                openIdClient.ReceivedSetupNeeded += new EventHandler(openid_SetupNeeded);
                openIdClient.DetectAndHandleResponse();

                // DetectAndHandleResponse does not handle errors, so we'll do it.
                if (RequestedMode.Error.Equals(openIdClient.RequestedMode))
                {
                    String error = openIdClient.StateContainer.RequestArguments["openid.error"];
                    Trace.Write("Got openid error: '" + error + "'");
                    Response.Redirect("Default.aspx?error=" + Server.UrlEncode(error));
                }
                else
                {
                    // if there were no events to handle, just call getLoginInfo
                    createAndSendLoginRequest(dossiaOpenID, openIdClient);
                }

            }
        }

        // The following events were registered in the GetConsumer() method above.
        protected void openid_ReceivedCancel(object sender, EventArgs e)
        {
            // Request has been cancelled. Respond appropriately.
            Response.Redirect("Default.aspx?error=" + Server.UrlEncode("The openID request was cancelled by the user"));
        }

        protected void openid_ValidationSucceeded(object sender, EventArgs e)
        {
            // User has been validated!  Respond appropriately.        
            OpenIdClient openIdClient = (OpenIdClient)sender;
            OpenIdUser openIdUser = openIdClient.RetrieveUser();
            LoginInfo loginInfo = new LoginInfo();
            loginInfo.openID = openIdUser.BaseIdentity;
            loginInfo.extensionData = openIdUser.ExtensionData;
            OAuthClient oauthClient = DossiaOauth.createDossiaOAuthClient();
            OAuthExtension oauthExtension = new OAuthExtension(openIdClient, oauthClient);
            loginInfo.accessToken = oauthExtension.GetAccessToken();
            Session["loginInfo"] = loginInfo;
            Response.Redirect("Default.aspx", true);
        }

        protected void openid_ValidationFailed(object sender, EventArgs e)
        {
            // Validating the user has failed.  Respond appropriately.    
            OpenIdClient openIdClient = (OpenIdClient)sender;
            // ValidationResponse validationResponse = openIdClient.ValidateResponse;
            ErrorCondition errorCondition = openIdClient.ErrorState;
            String openIdString = openIdClient.ToString();
            Response.Redirect("Default.aspx", true);
        }

        protected void openid_SetupNeeded(object sender, EventArgs e)
        {
            // Immediate authentication response showed that the user isn't logged in.
            // You should redirect the user to OpenID Provider to continue setup.
            ((OpenIdClient)sender).CreateRequest(false, true);
        }

        protected void createAndSendLoginRequest(DossiaOpenID dossiaOpenID,
            OpenIdClient openIdClient)
        {
            dossiaOpenID.InitializeForSending(openIdClient);
            openIdClient.CreateRequest(false, true);
        }
    }
}
