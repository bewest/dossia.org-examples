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
using ExtremeSwank.OpenId.PlugIns.Extensions;
using System.IO;
using ExtremeSwank.OpenId.Persistence;

namespace openidrp
{
    public partial class EsLogin : System.Web.UI.Page
    {
        // private static StringWriter LOGGER = new StringWriter(Global.LogMessages); 
        // todo: This should be an application global
        private static SingularAssociationManager associationManager = new SingularAssociationManager();
        private static SingularSessionManager sessionManager = new SingularSessionManager();
        
        protected void Page_Load(object sender, EventArgs e)
        {
            // If this is not a postback, start up the Consumer
            // and handle any OpenID response messages, if present
            if (!IsPostBack)
            {
                OpenIdClient openid = GetConsumer();
                // Read the arguments in the current request and
                // automatically validate any OpenID responses,
                // firing events when actions occur.
                openid.DetectAndHandleResponse();
            }
        }

        protected OpenIdClient GetConsumer()
        {
            // Initialize a new OpenIdClient, reading arguments
            // from the current request, and using Session and
            // Application objects to store data.  For more
            // flexibility, see "Disabling Stateful Mode" and
            // "Persisting Stateful Data" below for more information.
            OpenIdClient openid = new OpenIdClient();
            openid.EnableStatefulMode(associationManager, sessionManager);
            // Subscribe to all the events that could occur
            openid.ValidationSucceeded += new EventHandler(openid_ValidationSucceeded);
            openid.ValidationFailed += new EventHandler(openid_ValidationFailed);
            openid.ReceivedCancel += new EventHandler(openid_ReceivedCancel);
            // Subscribing to SetupNeeded is only needed if using immediate authentication
            openid.ReceivedSetupNeeded += new EventHandler(openid_SetupNeeded);
            return openid;
        }

        // This is the OnClick event for the submit button next to the OpenID text box.    
        protected void submitButton_OnClick(object sender, EventArgs e)
        {
            // LOGGER.WriteLine("openidrp: entering submitButton_OnClick");
            // Create an OpenIdClient using default settings
            // (see Page_Load() above)
            OpenIdClient openid = GetConsumer();
            // Set the Identity to what the user entered on the form.
            openid.ProviderUrl = new Uri("http://webui2.dossia.org:8080/authserver/openID.action");
            openid.TrustRoot = "http://localhost:8080/openidrp";
            openid.UseDirectedIdentity = true;
            // openid.Identity = "http://specs.openid.net/auth/2.0/identifier_select";
            // openid.Identity = "https://webui2.dosssia.org:8080/authserver";
            Uri uri = openid.CreateRequest(false, false);
            Response.Redirect(uri.ToString());
            // LOGGER.WriteLine("openidrp: Uri = " + uri.ToString());
        }

        // The following events were registered in the GetConsumer() method above.
        protected void openid_ReceivedCancel(object sender, EventArgs e)
        {
            // Request has been cancelled. Respond appropriately.
            Response.Redirect("Default.aspx");
        }

        protected void openid_ValidationSucceeded(object sender, EventArgs e)
        {
            // User has been validated!  Respond appropriately.        
            OpenIdUser thisuser = ((OpenIdClient)sender).RetrieveUser();

            Response.Redirect("Default.aspx");
        }

        protected void openid_ValidationFailed(object sender, EventArgs e)
        {
            // Validating the user has failed.  Respond appropriately.
            Response.Redirect("Default.aspx");
        }

        protected void openid_SetupNeeded(object sender, EventArgs e)
        {
            // Immediate authentication response showed that the user isn't logged in.
            // You should redirect the user to OpenID Provider to continue setup.
            ((OpenIdClient)sender).CreateRequest(false, true);
        }
    }
}

