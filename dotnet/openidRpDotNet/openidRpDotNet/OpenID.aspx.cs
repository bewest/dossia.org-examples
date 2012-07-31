using System;
using System.Web;
using ExtremeSwank.OpenId;
using openidrp.Code;
using ExtremeSwank.OpenId.PlugIns.Extensions;
using ExtremeSwank.OAuth;
using System.Configuration;

namespace openidrp
{
    public partial class OpenID : System.Web.UI.Page
    {
        

        protected void Page_Load(object sender, EventArgs e)
        {
            Boolean reqAttributes = Request["reqAttributes"] == "1";
            Boolean reqOauthToken = Request["reqOauthToken"] == "1";
            Trace.Write("reqAttributes = '" + reqAttributes + "'");
            Trace.Write("reqOauthToken = '" + reqOauthToken + "'");
            
            // If this is not a postback, start up the Consumer
            // and handle any OpenID response messages, if present
            if (!IsPostBack)
            {
                DossiaOpenID dossiaOpenID = new DossiaOpenID(Application, Session);
                OpenIdClient openIdClient = dossiaOpenID.CreateOpenIdClient();
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
                    createAndSendLoginRequest(dossiaOpenID, openIdClient, reqAttributes, reqOauthToken);
                }
            }
        }

        protected void createAndSendLoginRequest(DossiaOpenID dossiaOpenID, 
            OpenIdClient openIdClient, 
            Boolean reqAttributes,
            Boolean reqOauthToken)
        {
            dossiaOpenID.InitializeForSending(openIdClient);
            
            // see http://docs.dossia.org/index.php/Dossia_OpenID_Consumer_Developer_Guide#Attributes_Available_via_OpenID
            // for list of available attributes
            if (reqAttributes)
            {
                AttributeExchange attributeExchange = new AttributeExchange(openIdClient);
                attributeExchange.AddFetchItem("http://openid.dossia.org/participant/address1", "address1", 1, true);
                attributeExchange.AddFetchItem("http://openid.dossia.org/participant/address2", "address2", 1, true);
                attributeExchange.AddFetchItem("http://openid.dossia.org/participant/city", "city", 1, true);
                attributeExchange.AddFetchItem("http://openid.dossia.org/participant/dateOfBirth", "dateOfBirth", 1, true);
                attributeExchange.AddFetchItem("http://openid.dossia.org/participant/email", "email", 1, true);
                attributeExchange.AddFetchItem("http://openid.dossia.org/participant/employeeId", "employeeId", 1, true);
                attributeExchange.AddFetchItem("http://openid.dossia.org/participant/employer", "employer", 1, true);
                attributeExchange.AddFetchItem("http://openid.dossia.org/participant/firstName", "firstName", 1, true);
                attributeExchange.AddFetchItem("http://openid.dossia.org/participant/gender", "gender", 1, true);
                attributeExchange.AddFetchItem("http://openid.dossia.org/participant/lastName", "lastName", 1, true);
                attributeExchange.AddFetchItem("http://openid.dossia.org/participant/middleName", "middleName", 1, true);
                attributeExchange.AddFetchItem("http://openid.dossia.org/participant/postalCode", "postalCode", 1, true);
                attributeExchange.AddFetchItem("http://openid.dossia.org/participant/state", "state", 1, true);
            }

            if (reqOauthToken)
            {
                OAuthClient oauthClient = DossiaOauth.createDossiaOAuthClient();
                OAuthExtension oauthext = new OAuthExtension(openIdClient, oauthClient);
            }

            openIdClient.CreateRequest(false, true);
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

        
    }
}
