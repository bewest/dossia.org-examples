using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using ExtremeSwank.OpenId.Persistence;

namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Provides OpenID client support for desktop applications.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The user's web browser will need to be launched to perform authentication.
    /// All data will be passed back to the application through an embedded web server that will
    /// only start as needed to receive the authentication response.
    /// </para>
    /// <para>
    /// Use BeginAuthentication() to asynchronously start
    /// the temporary HTTP server and return the redirect URL.  This URL should be passed to the client host
    /// and launched in its web browser.  Use RetrieveAuthenticationResponse() to wait for the authentication
    /// response, or subscribe to the available events if blocking is not desired.
    /// </para>
    /// <para>
    /// In either case, the HTTP server will be closed if the timeout has expired.
    /// </para>
    /// </remarks>
    public class OpenIdDesktopClient : IDisposable
    {
        #region Fields and Properties

        HttpListener listener;
        OpenIdClient openid;
        bool responsereceived;
        bool authSuccessful;
        ErrorCondition _ErrorState;
        int _Timeout;
        string _Identity;
        int _Port;
        bool _UseRandomPort;
        string _Hostname;
        DateTime startTime;
        ISessionPersistence _SessionManager;
        IAssociationPersistence _AssociationManager;
        Uri _DirectedProviderUrl;
        int _lowPort = 1024;
        int _highPort = 5000;
        bool _StatefulMode;

        /// <summary>
        /// Generates a URL prefix that will be used to initialize
        /// the temporary HTTP server.
        /// </summary>
        string Prefix
        {
            get { return "http://*:" + _Port + "/"; }
        }

        /// <summary>
        /// The OpenIdClient object used for processing.
        /// </summary>
        public OpenIdClient Consumer
        {
            get { return openid; }
        }

        /// <summary>
        /// Current error status.
        /// </summary>
        public ErrorCondition Error
        {
            get { return _ErrorState; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new instance of OpenIdDesktopClient.
        /// </summary>
        /// <param name="identity">The OpenID Identity to authenticate.</param>
        /// <param name="timeout">The amount of time to wait for a response back from the OpenID Provider.</param>
        /// <param name="hostName">The hostname that will be used for the return URL.</param>
        /// <param name="port">The port number that should be used to receive the authentication response from the User's web browser, if random ports are not desired.</param>
        public OpenIdDesktopClient(string identity, int timeout, string hostName, int port)
        {
            Init(identity);
            _Hostname = hostName;
            _Timeout = timeout;
            _Port = port;
            openid = GetConsumer(new NameValueCollection());
        }

        /// <summary>
        /// Create a new instance of OpenIdDesktopClient.
        /// </summary>
        /// <param name="identity">The OpenID Identity to authenticate.</param>
        /// <param name="timeout">The amount of time to wait for a response back from the OpenID Provider.</param>
        /// <param name="hostName">The hostname that will be used for the return URL.</param>
        public OpenIdDesktopClient(string identity, int timeout, string hostName)
        {
            _UseRandomPort = true;
            Init(identity);
            _Hostname = hostName;
            _Timeout = timeout;
            openid = GetConsumer(new NameValueCollection());
        }

        /// <summary>
        /// Create a new instance of OpenIdDesktopClient.
        /// </summary>
        /// <param name="identity">The OpenID Identity to authenticate.</param>
        /// <param name="timeout">The amount of time to wait for a response back from the OpenID Provider.</param>
        public OpenIdDesktopClient(string identity, int timeout)
        {
            _UseRandomPort = true;
            Init(identity);
            _Timeout = timeout;
            openid = GetConsumer(new NameValueCollection());
        }

        /// <summary>
        /// Create a new instance of OpenIdDesktopClient.
        /// </summary>
        /// <param name="identity">The OpenID Identity to authenticate.</param>
        /// <param name="timeout">The amount of time to wait for a response back from the OpenID Provider.</param>
        /// <param name="hostName">The hostname that will be used for the return URL.</param>
        /// <param name="lowPort">The lowest port number in the desired range.</param>
        /// <param name="highPort">The highest port number in the desired range.</param>
        /// <remarks>
        /// Use a random port between two supplied port numbers.
        /// </remarks>
        public OpenIdDesktopClient(string identity, int timeout, string hostName, int lowPort, int highPort)
        {
            _lowPort = lowPort;
            _highPort = highPort;
            _UseRandomPort = true;
            Init(identity);
            _Hostname = hostName;
            _Timeout = timeout;
            openid = GetConsumer(new NameValueCollection());
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start listening for an authentication response, and return the redirect URL that the web browser
        /// will be pointed to.
        /// </summary>
        /// <returns>The redirect URL for the remote web browser.</returns>
        public Uri BeginAuthentication()
        {
            NonblockingListener();
            Uri url = openid.CreateRequest(false, false);
            if (url == null)
            {
                _ErrorState = openid.ErrorState;
                CancelAuthentication();
            }
            else
            {
                AuthenticationTimer at = new AuthenticationTimer(WaitForResponse);
                at.BeginInvoke(null, null);
            }
            return url;
        }

        /// <summary>
        /// Stop listening for the authentication response.
        /// </summary>
        public void CancelAuthentication()
        {
            if (listener != null)
            {
                listener.Abort();
                listener.Close();
                listener = null;
            }
        }

        /// <summary>
        /// Wait until an authentication response has been received, and return the result.
        /// </summary>
        /// <returns>True if succeeded, false if not.</returns>
        public bool RetrieveAuthenticationResponse()
        {
            return BlockWaitForResponse();
        }

        /// <summary>
        /// Enable Stateful authentication using supplied persistence objects.
        /// </summary>
        /// <param name="associationManager">IAssociationPersistence object to use when persisting associations.</param>
        /// <param name="sessionManager">ISessionPersistence object to use when persisting per-user data.</param>
        public void EnableStatefulMode(IAssociationPersistence associationManager, ISessionPersistence sessionManager)
        {
            if (associationManager == null) { throw new ArgumentNullException("associationManager"); }
            if (sessionManager == null) { throw new ArgumentNullException("sessionManager"); }
            _StatefulMode = true;
            _AssociationManager = associationManager;
            _SessionManager = sessionManager;
            openid.EnableStatefulMode(_AssociationManager, _SessionManager);
        }

        /// <summary>
        /// Enable Stateful authentication using default (volatile) persistence objects.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="SingularAssociationManager"/> and <see cref="SingularSessionManager"/>
        /// objects.  These are volatile, and all contained data will be destroyed when 
        /// the <see cref="OpenIdDesktopClient"/> object is disposed.
        /// </remarks>
        public void EnableStatefulMode()
        {
            _StatefulMode = true;
            _AssociationManager = new SingularAssociationManager();
            _SessionManager = new SingularSessionManager();
            openid.EnableStatefulMode(_AssociationManager, _SessionManager);
        }

        /// <summary>
        /// Enable Directed Identity mode for a specific OpenID Provider.
        /// </summary>
        /// <remarks>
        /// Consumer will only authenticate with the supplied provider URL,
        /// and will accept the OpenID returned in the authentication response.
        /// </remarks>
        /// <param name="openidServer">URL to the desired OpenID server</param>
        public void EnableDirectedIdentity(Uri openidServer)
        {
            _DirectedProviderUrl = openidServer;
            openid.UseDirectedIdentity = true;
            openid.ProviderUrl = openidServer;
        }

        #endregion

        #region Private Methods and Properties

        /// <summary>
        /// Shared initialization method used by all constructors.
        /// </summary>
        /// <param name="identity">OpenID Identity to authenticate.</param>
        void Init(string identity)
        {
            GenerateRandomPort();
            authSuccessful = false;
            responsereceived = false;
            _Identity = identity;
            _Hostname = System.Net.Dns.GetHostName().ToLowerInvariant();
        }

        /// <summary>
        /// Create a new OpenIdClient object with settings appropriate for this class.
        /// </summary>
        /// <param name="arguments">The <see cref="NameValueCollection"/> containing the received arguments.</param>
        /// <returns>The created OpenIdClient object.</returns>
        OpenIdClient GetConsumer(NameValueCollection arguments)
        {
            openid = new OpenIdClient(arguments);
            if (_StatefulMode)
            {
                openid.EnableStatefulMode(_AssociationManager, _SessionManager);
            }
            openid.Identity = _Identity;
            if (_DirectedProviderUrl != null)
            {
                openid.UseDirectedIdentity = true;
                openid.ProviderUrl = _DirectedProviderUrl;
            }
            SetConsumerUrls();

            return openid;
        }

        /// <summary>
        /// Updates the <see cref="OpenIdClient"/> object with the
        /// correct TrustRoot and RetunURL values.
        /// </summary>
        void SetConsumerUrls()
        {
            if (openid != null)
            {
                openid.TrustRoot = "http://" + _Hostname + ":" + _Port + "/";
                openid.ReturnUrl = new Uri(openid.TrustRoot + "return");
            }
        }

        /// <summary>
        /// If a port was not specified in the constructor, generate a
        /// random port number between 1024 and 5000.
        /// </summary>
        void GenerateRandomPort()
        {
            if (_UseRandomPort)
            {
                Random r = new Random(DateTime.Now.Second);
                _Port = r.Next(_lowPort, _highPort);
                SetConsumerUrls();
            }
        }

        /// <summary>
        /// Intended to be called asynchronously.  Check every second to see if the user's authentication
        /// response has been received.  If timeout occurs, automatically close HTTP server.
        /// </summary>
        /// <returns>True if a response has been received, false if timed out.</returns>
        bool WaitForResponse()
        {
            while (true)
            {
                if (responsereceived) { break; }
                if (DateTime.Now > startTime.AddSeconds(_Timeout))
                {
                    _ErrorState = ErrorCondition.SessionTimeout;
                    authSuccessful = false;
                    CancelAuthentication();
                    if (AuthenticationResponseTimedOut != null) { AuthenticationResponseTimedOut.Invoke(this, new EventArgs()); }
                    break;
                }
                Thread.Sleep(1000);
            }
            return authSuccessful;
        }

        /// <summary>
        /// Intended to be called synchronously.  Check every second to see if the user's authentication
        /// response has been received.  Will wait until the automatically called asynchronous WaitForResponse()
        /// function has returned.
        /// </summary>
        /// <returns>True if authentication is successful, false if not.</returns>
        bool BlockWaitForResponse()
        {
            while (true)
            {
                if (responsereceived) { break; }
                if (listener == null) { break; }
                if (!listener.IsListening) { break; }
                Thread.Sleep(1000);
            }
            return authSuccessful;
        }

        /// <summary>
        /// Start the temporary HTTP server, and register
        /// the callback method.
        /// </summary>
        void NonblockingListener()
        {
            bool success = false;
            int attempts = 0;
            while (!success)
            {
                try
                {
                    listener = new HttpListener();
                    listener.Prefixes.Add(Prefix);
                    listener.Start();
                    startTime = DateTime.Now;
                    success = true;
                    listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
                }
                catch (HttpListenerException e)
                {
                    Console.WriteLine("Error opening HTTP listening port: " + e.Message);
                    startTime = new DateTime();
                    GenerateRandomPort();
                    success = false;
                }
                attempts++;
                if (attempts >= 5)
                {
                    throw new OperationCanceledException("Could not open HTTP listening port after 5 attempts.  Ensure the process is running with Administrator privileges.");
                }
            }

        }

        #endregion

        #region Private Event Callbacks

        /// <summary>
        /// The callback method for the temporary HTTP server.
        /// </summary>
        /// <param name="result">The <see cref="IAsyncResult"/> object representing the active <see cref="HttpListener"/>.</param>
        void ListenerCallback(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            if (!listener.IsListening) { return; }

            try
            {
                // Call EndGetContext to complete the asynchronous operation.
                HttpListenerContext context = listener.EndGetContext(result);
                HttpListenerRequest request = context.Request;

                openid = GetConsumer(request.QueryString);
                openid.ValidationSucceeded += new EventHandler(openid_ValidationSucceeded);
                openid.ValidationFailed += new EventHandler(openid_ValidationFailed);
                openid.ReceivedCancel += new EventHandler(openid_ReceivedCancel);
                openid.DetectAndHandleResponse();

                // Obtain a response object.
                HttpListenerResponse response = context.Response;
                // Construct a response.
                string responseString = "<HTML><BODY onLoad=\"javascript:window.close();\">Please close this browser window.</BODY></HTML>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
                responsereceived = true;
                listener.Close();
                if (AuthenticationResponseReceived != null)
                {
                    AuthenticationResponseReceived.Invoke(this, new EventArgs());
                }
            }
            catch (HttpListenerException e)
            {
                Tracer.Write("HttpListener error: " + e.Message);
            }
        }

        /// <summary>
        /// Callback method for when OpenIdClient invokes its <see cref="ClientCore.ReceivedCancel"/> event.
        /// </summary>
        /// <param name="sender">The OpenIdClient object invoking the event.</param>
        /// <param name="e">Event arguments.</param>
        void openid_ReceivedCancel(object sender, EventArgs e)
        {
            _ErrorState = ErrorCondition.RequestCanceled;
            if (AuthenticationCanceled != null) { AuthenticationCanceled.Invoke(this, new EventArgs()); }
        }

        /// <summary>
        /// Callback method for when OpenIdClient invokes its <see cref="ClientCore.ValidationSucceeded"/> event.
        /// </summary>
        /// <param name="sender">The OpenIdClient object invoking the event.</param>
        /// <param name="e">Event arguments.</param>
        void openid_ValidationSucceeded(object sender, EventArgs e)
        {
            authSuccessful = true;
            if (AuthenticationSuccessful != null) { AuthenticationSuccessful.Invoke(this, new EventArgs()); }
        }

        /// <summary>
        /// Callback method for when OpenIdClient invokes its <see cref="ClientCore.ValidationFailed"/> event.
        /// </summary>
        /// <param name="sender">The OpenIdClient object invoking the event.</param>
        /// <param name="e">Event arguments.</param>
        void openid_ValidationFailed(object sender, EventArgs e)
        {
            if (AuthenticationFailed != null) { AuthenticationFailed.Invoke(this, new EventArgs()); }
        }

        /// <summary>
        /// Delegate used for the asynchronous timeout checker.
        /// </summary>
        /// <returns>Value not used.</returns>
        delegate bool AuthenticationTimer();

        #endregion

        #region Public Events

        /// <summary>
        /// Authentication response has been received.
        /// </summary>
        public event EventHandler AuthenticationResponseReceived;

        /// <summary>
        /// Authentication response has timed out.
        /// </summary>
        public event EventHandler AuthenticationResponseTimedOut;

        /// <summary>
        /// Authentication was successful.
        /// </summary>
        public event EventHandler AuthenticationSuccessful;

        /// <summary>
        /// Authentication failed.
        /// </summary>
        public event EventHandler AuthenticationFailed;

        /// <summary>
        /// Authentication was cancelled at the OpenID Provider.
        /// </summary>
        public event EventHandler AuthenticationCanceled;

        #endregion


        #region IDisposable Members

        /// <summary>
        /// Dispose all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose all resources.
        /// </summary>
        /// <param name="disposing">If true, dispose both managed and unmanaged resources.</param>
        protected virtual void Dispose(bool disposing) 
        {
            if (disposing) 
            {
                openid = null;
                _SessionManager = null;
                _AssociationManager = null;
            }
            if (listener != null)
            {
                listener.Close();
            }
        }

        #endregion
    }
}
