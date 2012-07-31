using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using ExtremeSwank.OpenId.PlugIns.Discovery;

namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Provides an OpenID Relying Party (Consumer) compatible with OpenID 1.1 and 2.0 specifications.
    /// </summary>
    /// <example>
    /// Here is a simple procedural example of using OpenIdClient in an ASP.NET application:
    /// <code>
    /// using ExtremeSwank.OpenID;
    /// 
    /// public partial class _Default
    /// {
    ///    protected void LoginButton_Click(object sender, EventArgs e)
    ///    {
    ///        OpenIdClient openid = new OpenIdClient();
    ///        openid.Identity = LoginBox1.Text;
    ///        openid.CreateRequest();
    ///    }
    ///    protected void LogOutButton_Click(object sender, EventArgs e)
    ///    {
    ///        Session["OpenID_UserObject"] = null;
    ///        // Handle user logout here
    ///    }
    ///    protected void Page_Load(object sender, EventArgs e)
    ///    {
    ///        if (!IsPostBack)
    ///        {
    ///            OpenIdClient openid = new OpenIdClient();
    ///            switch (openid.RequestedMode)
    ///            {
    ///               case RequestedMode.IdResolution:
    ///                    if (openid.ValidateResponse())
    ///                    {
    ///                        OpenIdUser thisuser = openid.RetrieveUser();
    ///                        Session["OpenID_UserObject"] = thisuser;
    ///                        // Authentication successful - Perform login here
    ///                    }
    ///                    else
    ///                    {
    ///                        // Authentication failure handled here
    ///                    }
    ///                    break;
    ///                case RequestedMode.CanceledByUser:
    ///                    // User has cancelled authentication - handle here
    ///                    break;
    ///            }
    ///        }
    ///    }
    /// }
    /// </code>
    /// A more advanced method, based on .NET events, is below:
    /// <code>
    /// using ExtremeSwank.OpenID;
    /// using ExtremeSwank.OpenID.Plugins.Extensions;
    ///
    /// public partial class _Default
    /// {
    ///
    ///    protected void Page_Load(object sender, EventArgs e)
    ///    {
    ///        // If this is not a postback, start up the Consumer
    ///        // and handle any OpenID request, if present
    ///        if (!IsPostBack)
    ///        {
    ///            OpenIdClient openid = GetConsumer();
    ///            openid.DetectAndHandleResponse();
    ///        }
    ///    }
    ///
    ///    protected OpenIdClient GetConsumer()
    ///    {
    ///        // Initialize the OpenID Consumer
    ///        OpenIdClient openid = new OpenIdClient();
    ///
    ///        // Subscribe to all the events that could occur
    ///        openid.ValidationSucceeded += new EventHandler(openid_ValidationSucceeded);
    ///        openid.ValidationFailed += new EventHandler(openid_ValidationFailed);
    ///        openid.ReceivedCancel += new EventHandler(openid_ReceivedCancel);
    ///
    ///        return openid;
    ///    }
    ///
    ///    protected void Button_Click(object sender, EventArgs e)
    ///    {
    ///        OpenIdClient openid = GetConsumer();
    ///
    ///        // Set Identity to the text of a field
    ///        openid.Identity = openid_url.Text;
    ///
    ///        openid.CreateRequest();
    ///    }
    ///
    ///    protected void openid_ReceivedCancel(object sender, EventArgs e)
    ///    {
    ///        // Request has been cancelled. Respond appropriately.
    ///    }
    ///
    ///    protected void openid_ValidationSucceeded(object sender, EventArgs e)
    ///    {
    ///        // User has been validated!  Respond appropriately.
    ///        OpenIdUser UserObject = ((OpenIdClient)sender).RetrieveUser();
    ///    }
    ///
    ///    protected void openid_ValidationFailed(object sender, EventArgs e)
    ///    {
    ///        // Validating the user has failed.  Respond appropriately.
    ///    }
    /// }
    /// </code>
    /// For non-ASP.NET environments:
    /// <code>
    /// IAssociationPersistence associationManager;
    /// ISessionPersistence sessionManager;
    ///  
    /// public MyObject() 
    /// {
    ///     // Create the association manager.
    ///     // Note that IAssociationPeristence object is only needed if OpenIdClient.EnableStatefulMode()
    ///     // is used.
    ///     string dsnstr = "Driver={SQL Server};Server=SERVER\\INSTANCE;Database=OpenIDDatabase;Uid=sa;Pwd=password;"
    ///     associationManager = new OdbcAssociationManager(dsnstr, "Prefix_");
    ///     // Use the SingularSessionManager for situations where the session does not actually
    ///     // need to be persisted, assuming that this instance will survive the entire OpenID
    ///     // authentication lifecycle.  DbSessionManager can also be used if you want to
    ///     // save session state into a database.
    ///     // Note that ISessionPersistence object is only needed if OpenIdClient.EnableStatefulMode()
    ///     // is used.
    ///     sessionManager = new SingularSessionManager();
    /// }
    /// 
    /// public OpenIdClient SetupConsumer(NameValueCollection arguments) 
    /// {
    ///     // Create a new OpenIdClient object
    ///     OpenIdClient openid = new OpenIdClient(arguments);
    ///     
    ///     // Enable Stateful mode.  If Stateful mode is not desired, just omit
    ///     // this step.
    ///     openid.EnableStatefulMode(associationManager, sessionManager);    
    ///     
    ///     openid.TrustRoot = "http://myserver.com/";
    ///     openid.ReturnUrl = "http://myserver.com/myPage";
    ///     return openid;
    /// }
    /// 
    /// // Discover the supplied OpenID identity's Provider URL, build and
    /// // return a redirect URL for the web browser.
    /// public string RetrieveAuthenticationUrl(string identity) 
    /// {
    ///     OpenIdClient openid = SetupConsumer(new NameValueCollection());
    ///     openid.Identity = identity;
    ///     
    ///     // Discover the supplied OpenID and return the redirect URL
    ///     return openid.CreateRequest(false, false);
    /// }
    /// 
    /// // An authentication response has been received from the OpenID Provider
    /// // by way of the user's web browser.  Process the data in the response.
    /// // If the response is valid, return an OpenIdUser object containing
    /// // the authentication details.
    /// public OpenIdUser ProcessAuthenticationResponse(NameValueCollection arguments) 
    /// {
    ///     OpenIdClient openid = SetupConsumer(arguments);
    ///     switch (openid.RequestedMode) 
    ///     {
    ///         case RequestedMode.IdResolution:
    ///             if (openid.ValidateResponse()) {
    ///                 OpenIdUser thisuser = openid.RetrieveUser();
    ///                 return thisuser;
    ///             }
    ///             break;
    ///     }
    ///     return null;
    /// }
    /// </code>
    /// </example>
    [Serializable]
    public sealed class OpenIdClient : ClientCore
    {
        string identity;
        bool useDirectedIdentity;

        #region Public Members

        /// <summary>
        /// Gets or sets the OpenID idenitifer and normalizes the value
        /// </summary>
        public string Identity
        {
            get { return identity; }
            set
            {
                identity = value;
            }
        }

        /// <summary>
        /// Set to true to enforce use of Directed Identity.
        /// </summary>
        /// <remarks>
        /// If set to true, ProviderUrl must be explicitly set.
        /// </remarks>
        public bool UseDirectedIdentity
        {
            get { return useDirectedIdentity; }
            set { useDirectedIdentity = value; }
        }

        /// <summary>
        /// Creates the redirect URL for the OpenID authentication request,
        /// and, if in an ASP.NET context, will automatically redirects the 
        /// user's web browser to the OpenID Provider.
        /// </summary>
        /// <returns>The redirect URL string that should be launched by the user's web browser.</returns>
        /// <remarks>
        /// Uses standard "checkid_setup" mode for authentication.  If user
        /// is not logged in to the OpenID Provider, the Provider is able
        /// to interact as needed.
        /// </remarks>
        public Uri CreateRequest()
        {
            return CreateRequest(false, true);
        }

        /// <summary>
        /// Creates the redirect URL for the OpenID authentication request.
        /// </summary>
        /// <param name="immediate">Create an immediate-mode request URL.</param>
        /// <param name="autoRedirect">Automatically redirect the current HTTP session to the OpenID Provider.</param>
        /// <returns>The redirect URL string that should be launched by the user's web browser.</returns>
        /// <remarks>
        /// <para>
        /// Use Immediate mode to get an immediate response back from the OpenID Provider.
        /// This means the OpenID Provider will not prompt the user in any way, but will
        /// redirect the user's browser back with an authentication response.  If the user
        /// is not currently logged in to the Provider, a "SetupNeeded" response will be
        /// received here at the client.
        /// </para>
        /// <para>
        /// Set autoRedirect to true to automatically perform an HTTP redirect on the user's
        /// browser.  This option is only valid in an ASP.NET environment.  For all other
        /// environments, setting this value will have no effect.
        /// </para>
        /// </remarks>
        public override Uri CreateRequest(bool immediate, bool autoRedirect)
        {
            // If Directed Identity is enabled, perform needed setup
            if (ProviderUrl != null && UseDirectedIdentity == true)
            {
                SetupDirectedIdentity();
            }

            // Check to ensure Identity is set
            if (String.IsNullOrEmpty(Identity))
            {
                StateContainer.ErrorState = ErrorCondition.NoIdSpecified;
                Tracer.Write("No OpenID specified, ending check");
                return null;
            }

            // If Directed Identity is disabled, and discovery has not already
            // occurred, perform discovery.
            if (ProviderUrl == null && UseDirectedIdentity == false)
            {
                LastDiscoveryResult = Utility.GetProviderUrl(Identity, StateContainer.DiscoveryPlugIns);
                if (LastDiscoveryResult != null)
                {
                    ProviderUrl = LastDiscoveryResult.ServerUrl;
                }
                else
                {
                    Tracer.Write("Discovery plug-ins could not locate OpenID Provider.");
                    StateContainer.ErrorState = ErrorCondition.NoServersFound;
                    return null;
                }
            }

            Tracer.Write("OpenID Version Discovered: " + LastDiscoveryResult.AuthVersion.ToString());

            return base.CreateRequest(immediate, autoRedirect);
        }

        /// <summary>
        /// Validates an OpenID authentication response.
        /// </summary>
        /// <remarks>
        /// <para>
        /// To determine if this method should be used, look at the value
        /// of the RequestedMode property, which detects the operational mode
        /// requested by the current HTTP request.  
        /// </para>
        /// <para>
        /// If RequestedMode is set to RequestedMode.IdResolution, the request
        /// is an authentication response from an OpenID Provider.
        /// </para>
        /// <para>
        /// Therefore, ValidateResponse() should be used to verify
        /// the validity of the response.
        /// </para>
        /// </remarks>
        /// <returns>True if successfully authenticated, false if not.</returns>
        public override bool ValidateResponse()
        {
            LastDiscoveryResult = Utility.GetProviderUrl(Identity, StateContainer.DiscoveryPlugIns);

            if (LastDiscoveryResult != null)
            {
                // Check to ensure the local identifier for the claimed identity
                // matches the identifier received from the Provider
                if (LastDiscoveryResult.LocalId != LastDiscoveryResult.ClaimedId)
                {
                    if (LastDiscoveryResult.LocalId != StateContainer.RequestArguments["openid.identity"])
                    {
                        Tracer.Write("Received identity does not match the discovered local identifier.");
                        return false;
                    }
                }
            }
            return base.ValidateResponse();
        }

        /// <summary>
        /// Independently performs discovery on the supplied OpenID and determines whether
        /// or not it is valid.
        /// </summary>
        /// <remarks>This is a free operation.  If you invoke IsValidIdentity(), the discovered server URL
        /// is cached.  Running CreateRequest() will skip server discovery and move directly on to 
        /// redirect URL generation.</remarks>
        /// <returns>True if discovery was successful, false if not.</returns>
        public bool IsValidIdentity()
        {
            LastDiscoveryResult = Utility.GetProviderUrl(Identity, StateContainer.DiscoveryPlugIns);
            if (LastDiscoveryResult != null)
            {
                ProviderUrl = LastDiscoveryResult.ServerUrl;
            }
            if (ProviderUrl == null) { return false; }
            return true;
        }

        #endregion

        /// <summary>
        /// Initialize the OpenIdClient object.
        /// </summary>
        protected override void Init()
        {
            // Use ClientCore's Init method.
            base.Init();

            // Set Identity to the claimed identifier present in the
            // current arguments, if present
            string esoid_claimedid = StateContainer.RequestArguments["esoid.claimed_id"];
            string openid_claimedid = StateContainer.RequestArguments["openid.claimed_id"];

            if (!String.IsNullOrEmpty(esoid_claimedid))
            {
                Identity = esoid_claimedid;
            }
            else if (!String.IsNullOrEmpty(openid_claimedid))
            {
                Identity = openid_claimedid.Split(new string[] { "#" }, StringSplitOptions.None)[0];
            }

            // Turn on Identity authentication support
            new PlugIns.Extensions.IdentityAuthentication(StateContainer);
        }

        /// <summary>
        /// When Directed Identity is enabled, populate the LastDiscoveryResult
        /// variable with fake discovery data.
        /// </summary>
        /// <remarks>
        /// This is needed, as discovery does not occur in Directed Identity 
        /// mode.
        /// </remarks>
        private void SetupDirectedIdentity()
        {
            identity = ProtocolUri.IdentifierSelect.AbsoluteUri;
            LastDiscoveryResult = new DiscoveryResult();
            LastDiscoveryResult.AuthVersion = ProtocolVersion.V2Dot0;
            LastDiscoveryResult.ClaimedId = ProtocolUri.IdentifierSelect.AbsoluteUri;
            LastDiscoveryResult.LocalId = ProtocolUri.IdentifierSelect.AbsoluteUri;
            LastDiscoveryResult.ServerUrl = ProviderUrl;
        }

        #region Constructors

        /// <summary>
        /// Provides a new OpenIdClient object with default settings.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Default settings assumes an ASP.NET environment, with
        /// Stateless authentication.  Use EnableStatefulMode() to
        /// switch to Stateful mode.
        /// </para>
        /// </remarks>
        public OpenIdClient()
        {
            EnsureAspNetOnConstructor();
            StateContainer.RequestArguments.Add(HttpContext.Current.Request.Params);
            Init();
        }

        /// <summary>
        /// Provides a new OpenIdClient object using a custom set of request arguments.
        /// </summary>
        /// <remarks>
        /// Stateless mode is enabled by default.  Use EnableStatefulMode() to switch
        /// to Stateful mode.
        /// </remarks>
        /// <param name="requestArguments">NameValueCollection containing arguments for this request.</param>
        public OpenIdClient(NameValueCollection requestArguments)
        {
            StateContainer.RequestArguments.Add(requestArguments);
            Init();
        }

        #endregion

    }
}
