using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using ExtremeSwank.OpenId.PlugIns.Discovery;

namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Base class for OpenID Relying Party.
    /// </summary>
    /// <remarks>
    /// Supports the core implementation outlined in the OpenID Specification,
    /// and does not provide optional Identity discovery functions.  Inherit 
    /// this class if non-Identity OpenID communication is required.
    /// </remarks>
    public abstract class ClientCore
    {
        #region Fields and Properties

        StateContainer _Rp;
        Uri _ProviderUrl;
        DiscoveryResult _LastDiscoveryResult;

        /// <summary>
        /// Last DiscoveryResult object created during discovery,
        /// or populated manually.
        /// </summary>
        protected DiscoveryResult LastDiscoveryResult
        {
            get { return _LastDiscoveryResult; }
            set { _LastDiscoveryResult = value; }
        }

        /// <summary>
        /// Gets or sets the URL that will serve as the base root of trust - defaults to current domain
        /// </summary>
        public string TrustRoot
        {
            get
            {
                if (String.IsNullOrEmpty(StateContainer.TrustRoot)) { StateContainer.TrustRoot = Utility.WebRoot + "/"; }
                return StateContainer.TrustRoot;
            }
            set { StateContainer.TrustRoot = value; }
        }
        /// <summary>
        /// Gets or sets a URL to transfer user upon approval - defaults to current page
        /// </summary>
        public Uri ReturnUrl
        {
            get { return StateContainer.ReturnToUrl; }
            set { StateContainer.ReturnToUrl = value; }
        }
        /// <summary>
        /// Gets or sets the URL of Identity Provider
        /// </summary>
        /// <remarks>
        /// If UseDirectedIdentity is set to true, ProviderUrl should be set manually.  Otherwise, this will be
        /// automatically set while discovering the claimed identifier.
        /// </remarks>
        public Uri ProviderUrl
        {
            get { return _ProviderUrl; }
            set { _ProviderUrl = value; }
        }

        /// <summary>
        /// Checks the current page request and returns the requested
        /// mode.
        /// </summary>
        /// <returns>RequestedMode representing the current mode.</returns>
        public RequestedMode RequestedMode
        {
            get
            {
                return Utility.GetRequestedMode(StateContainer.RequestArguments);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check for ASP.NET Context. If none is present, throw exception.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if ASP.NET context is not present.</exception>
        protected static void EnsureAspNetOnConstructor()
        {
            if (HttpContext.Current == null)
            {
                throw new InvalidOperationException("Cannot initialize with this constructor if not in an ASP.NET application.  Use 'OpenIdClient(NameValueCollection)' constructor instead.");
            }
        }

        /// <summary>
        /// If in an ASP.NET context, redirect the user to the provided URL.
        /// </summary>
        /// <param name="url">URL to redirect.</param>
        protected static void Redirect(Uri url)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Response.Redirect(url.AbsoluteUri, false);
            }
        }

        /// <summary>
        /// Shared initialization method - should be used by constructor.
        /// </summary>
        protected virtual void Init()
        {
            Tracer.Write("Initializing OpenID Consumer");
            this.TrustRoot = Utility.WebRoot + "/";

            // Build the ReturnUrl from the current HttpContext, if present.
            // If HttpContext is not present, ReturnUrl needs to be set manually.
            if (HttpContext.Current != null)
            {
                NameValueCollection arguments = HttpContext.Current.Request.QueryString;

                StringBuilder qbuilder = new StringBuilder();

                for (int i = 0; i < arguments.Count; i++)
                {
                    string key = arguments.Keys[i];
                    bool shouldAdd = true;

                    if (String.IsNullOrEmpty(key)) { }
                    else if (key.StartsWith("openid.", StringComparison.OrdinalIgnoreCase) || key.StartsWith("esoid.", StringComparison.OrdinalIgnoreCase) || key == "cnonce") { shouldAdd = false; }

                    if (shouldAdd)
                    {
                        if (qbuilder.Length == 0)
                        {
                            qbuilder.Append("?");
                        }
                        else
                        {
                            qbuilder.Append("&");
                        }
                        qbuilder.Append(key + "=" + HttpUtility.UrlEncode(arguments[i]));
                    }
                }

                string queryString = qbuilder.ToString();
                this.ReturnUrl = new Uri(Utility.WebRoot
                    + HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"]
                    + queryString);
            }

            // Initialize all default discovery plug-ins
            new PlugIns.Discovery.Xrds(StateContainer);
            new PlugIns.Discovery.Yadis(StateContainer);
            new PlugIns.Discovery.Html(StateContainer);

            // Automatically load extension plug-ins, based on the
            // information in the received arguments, if present
            Utility.AutoLoadExtensionPlugins(StateContainer);

            Tracer.Write("Finished initialization");
        }

        #endregion

        #region Public Methods

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
        public virtual Uri CreateRequest(bool immediate, bool autoRedirect)
        {
            Tracer.Write("Beginning standard authentication check");

            // If the OpenID Provider was not found, return an error.
            if (ProviderUrl == null)
            {
                StateContainer.ErrorState = ErrorCondition.NoServersFound;
                Tracer.Write("No OpenID Provider found.");
                return null;
            }

            // Set to defaults if LastDiscoveryResult is not set by
            // an inheriting class.
            if (LastDiscoveryResult == null)
            {
                LastDiscoveryResult = new DiscoveryResult();
                LastDiscoveryResult.AuthVersion = ProtocolVersion.V2Dot0;
                LastDiscoveryResult.ServerUrl = ProviderUrl;
            }

            // Adjust the AuthenticationMode setting based on
            // the discovered Provider URL
            StateContainer.AuthMode = Quirks.CheckOpenIDMode(ProviderUrl.AbsoluteUri, StateContainer.AuthMode);

            // Perform Stateful (smart) authentication
            if (StateContainer.AuthMode == AuthenticationMode.Stateful)
            {
                if (ProviderUrl != null)
                {
                    if (Utility.BuildAssociation(ProviderUrl, StateContainer.AssociationManager, LastDiscoveryResult.AuthVersion) == true)
                    {
                        Uri redirectUrl = Utility.GetRedirectURL(StateContainer, LastDiscoveryResult, immediate);
                        Tracer.Write("Returning Stateful URL - " + redirectUrl.AbsoluteUri);
                        if (autoRedirect) { Redirect(redirectUrl); }
                        return redirectUrl;
                    }
                    else
                    {
                        StateContainer.AuthMode = AuthenticationMode.Stateless;
                        Tracer.Write("Stateful key exchange failed, forcing Dumb mode and re-running authentication");
                    }
                }
            }

            // Perform Stateless (dumb) authentication
            if (StateContainer.AuthMode == AuthenticationMode.Stateless)
            {
                if (ProviderUrl != null)
                {
                    Uri redirectUrl = Utility.GetRedirectURL(StateContainer, LastDiscoveryResult, immediate);
                    Tracer.Write("Returning stateless URL - " + redirectUrl.AbsoluteUri);
                    if (autoRedirect) { Redirect(redirectUrl); }
                    return redirectUrl;
                }
            }
            return null;
        }

        /// <summary>
        /// Enable Stateful authentication mode using supplied 
        /// association and session persistence plug-ins.
        /// </summary>
        /// <param name="associationManager">IAssociationPersistence object to use while persisting associations from OpenID Providers.</param>
        /// <param name="sessionManager">ISessionPersistence object to use while persisting user session state.</param>
        public void EnableStatefulMode(IAssociationPersistence associationManager, ISessionPersistence sessionManager)
        {
            if (associationManager == null) { throw new ArgumentNullException("associationManager"); }
            if (sessionManager == null) { throw new ArgumentNullException("sessionManager"); }

            StateContainer.AssociationManager = associationManager;
            StateContainer.SessionManager = sessionManager;
            StateContainer.AuthMode = AuthenticationMode.Stateful;

            StateContainer.AssociationManager.Cleanup();
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
        /// <returns>True if successfully validated, false if not.</returns>
        public virtual bool ValidateResponse()
        {
            Uri server = null;
            if (LastDiscoveryResult != null)
            {
                server = LastDiscoveryResult.ServerUrl;
            }
            if (server == null) { return false; }

            if (StateContainer.AuthMode == AuthenticationMode.Stateful)
            {
                Tracer.Write("Stateful mode enabled, beginning validation check using shared key");
                bool success = Utility.ValidateStatefulResponse(server, StateContainer);
                StateContainer.Nonce = -1;
                if (success)
                {
                    return true;
                }
                else
                {
                    Tracer.Write("Validation failed, performing stateless validation check");
                    StateContainer.AuthMode = AuthenticationMode.Stateless;
                }
            }

            if (StateContainer.AuthMode == AuthenticationMode.Stateless)
            {
                Tracer.Write("Stateless mode enabled, beginning validation request with server");
                return Utility.ValidateStatelessResponse(server, false, StateContainer);
            }
            Tracer.Write("Request refused, authentication failed");
            StateContainer.ErrorState = ErrorCondition.RequestRefused;
            return false;
        }
        /// <summary>
        /// Returns the current error state
        /// </summary>
        /// <returns>An ErrorCondition representing the current error state.</returns>
        public ErrorCondition ErrorState
        {
            get
            {
                return StateContainer.ErrorState;
            }
        }

        /// <summary>
        /// After successful validation, provides an object to hold the user information
        /// </summary>
        /// <returns>OpenIdUser object containing identifier and Extension data</returns>
        public OpenIdUser RetrieveUser()
        {
            OpenIdUser ret = new OpenIdUser(LastDiscoveryResult);
            ret.Retrieve(this);
            return ret;
        }

        /// <summary>
        /// Gets the current StateContainer object in use by this OpenIdClient.
        /// </summary>
        /// <remarks>
        /// This should only need to be used by plugins so they can self-register
        /// upon intialization.
        /// </remarks>
        public StateContainer StateContainer
        {
            get 
            {
                if (_Rp == null) { _Rp = new StateContainer(); }
                return _Rp; 
            }
        }

        /// <summary>
        /// Look at the current request arguments and perform work appropriately,
        /// invoking events as conditions occur.
        /// </summary>
        public void DetectAndHandleResponse()
        {
            RequestedMode rm = Utility.GetRequestedMode(StateContainer.RequestArguments);

            EventArgs e = new EventArgs();
            switch (rm)
            {
                case RequestedMode.IdResolution:
                    if (ReceivedResponse != null)
                    {
                        ReceivedResponse.Invoke(this, e);
                    }
                    bool validated = ValidateResponse();
                    if (validated)
                    {
                        if (ValidationSucceeded != null)
                        {
                            ValidationSucceeded(this, e);
                        }
                    }
                    else
                    {
                        if (ValidationFailed != null)
                        {
                            ValidationFailed(this, e);
                        }
                    }
                    break;
                case RequestedMode.CanceledByUser:
                    if (ReceivedResponse != null)
                    {
                        ReceivedResponse.Invoke(this, e);
                    }
                    if (ReceivedCancel != null)
                    {
                        ReceivedCancel.Invoke(this, e);
                    }
                    break;
                case RequestedMode.SetupNeeded:
                    if (ReceivedResponse != null)
                    {
                        ReceivedResponse.Invoke(this, e);
                    }
                    if (ReceivedSetupNeeded != null)
                    {
                        ReceivedSetupNeeded.Invoke(this, e);
                    }
                    break;
            }
        }

        #endregion

        #region Public Events

        /// <summary>
        /// A response has been received from an OpenID Provider.
        /// </summary>
        public event EventHandler ReceivedResponse;

        /// <summary>
        /// A user-initiated Cancel response has been received from an OpenID Provider.
        /// </summary>
        public event EventHandler ReceivedCancel;

        /// <summary>
        /// The authentication response has been validated successfully.
        /// </summary>
        public event EventHandler ValidationSucceeded;

        /// <summary>
        /// The authentication response has failed validation.
        /// </summary>
        public event EventHandler ValidationFailed;

        /// <summary>
        /// Immediate mode request has failed, should issue a standard
        /// authentication request.
        /// </summary>
        public event EventHandler ReceivedSetupNeeded;

        #endregion
    }
}
