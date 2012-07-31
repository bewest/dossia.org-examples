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
using ExtremeSwank.OpenId;
using System.Web.SessionState;
using openidrp.Code;
using System.Collections.Specialized;
using ExtremeSwank.OpenId.PlugIns.Extensions;

namespace openidrp
{
    public partial class OpenIDSSO : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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

                // if there were no events to handle, just call getLoginInfo
                createAndSendLoginRequest(dossiaOpenID, openIdClient);
            }
        }

        protected void createAndSendLoginRequest(DossiaOpenID dossiaOpenID, OpenIdClient openIdClient)
        {
            dossiaOpenID.InitializeForSending(openIdClient);
            AttributeExchange attributeExchange = new AttributeExchange(openIdClient);
            // see http://docs.dossia.org/index.php/Dossia_OpenID_Consumer_Developer_Guide#Attributes_Available_via_OpenID
            // for list of available attributes
            attributeExchange.AddFetchItem("http://openid.dossia.org/participant/firstName", "firstName", 1, true);
            attributeExchange.AddFetchItem("http://openid.dossia.org/participant/lastName", "lastName", 1, true);
            
            openIdClient.CreateRequest(false, true);
        }

        // The following events were registered in the GetConsumer() method above.
        protected void openid_ReceivedCancel(object sender, EventArgs e)
        {
            // Request has been cancelled. Respond appropriately.    
            Response.Redirect("Default.aspx", true);
        }

        protected void openid_ValidationSucceeded(object sender, EventArgs e)
        {
            // User has been validated!  Respond appropriately.        
            OpenIdClient openIdClient = (OpenIdClient)sender;
            OpenIdUser openIdUser = openIdClient.RetrieveUser();
            LoginInfo loginInfo = new LoginInfo();
            loginInfo.openID = openIdUser.BaseIdentity;
            String firstName = openIdUser.GetValue("http://openid.dossia.org/participant/firstName");
            String lastName = openIdUser.GetValue("http://openid.dossia.org/participant/lastName");
            loginInfo.firstName = firstName;
            loginInfo.lastName = lastName;
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
