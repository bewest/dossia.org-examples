using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using ExtremeSwank.OpenId.Persistence;

namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// State object passed among static methods that implement OpenID authentication steps.
    /// </summary>
    [Serializable]
    public class StateContainer
    {
        AuthenticationMode _AuthMode = AuthenticationMode.Stateless;
        string _TrustRoot;
        Uri _ReturnToUrl;
        List<IExtension> _ExtensionPlugIns = new List<IExtension>();
        List<IDiscovery> _DiscoveryPlugIns = new List<IDiscovery>();
        IAssociationPersistence _AssociationManager;
        ISessionPersistence _SessionManager;
        NameValueCollection _RequestArguments = new NameValueCollection();
        ErrorCondition _ErrorState;

        /// <summary>
        /// Creates a new StateContainer.
        /// </summary>
        public StateContainer()
        {
            AssociationManager = new ApplicationAssociationManager();
            SessionManager = new SessionSessionManager();
        }

        /// <summary>
        /// Creates a new StateContainer.
        /// </summary>
        /// <param name="associationManager">Association persistence manager.</param>
        /// <param name="sessionManager">Session persistence manager.</param>
        public StateContainer(IAssociationPersistence associationManager, ISessionPersistence sessionManager)
        {
            AssociationManager = associationManager;
            SessionManager = sessionManager;
        }

        /// <summary>
        /// Gets or sets the Association persistence manager.
        /// </summary>
        public IAssociationPersistence AssociationManager
        {
            get { return _AssociationManager; }
            set 
            { 
                _AssociationManager = value;
                if (value == null)
                {
                    _AuthMode = AuthenticationMode.Stateless;
                    _SessionManager = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Session persistence manager.
        /// </summary>
        public ISessionPersistence SessionManager
        {
            get { return _SessionManager; }
            set 
            {
                _SessionManager = value;
                if (value == null)
                {
                    _AuthMode = AuthenticationMode.Stateless;
                    _AssociationManager = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the authentication mode currently being used.
        /// </summary>
        public AuthenticationMode AuthMode
        {
            get { return _AuthMode; }
            set 
            {
                _AuthMode = value;
                if (value == AuthenticationMode.Stateless)
                {
                    _AssociationManager = null;
                    _SessionManager = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the root URL of this web server or domain.
        /// </summary>
        public string TrustRoot
        {
            get { return _TrustRoot; }
            set { _TrustRoot = value; }
        }

        /// <summary>
        /// Persists the cnonce value in the current session so it can
        /// be verified when the authentication response is received.
        /// </summary>
        /// <remarks>
        /// This is extremely important to ensure that simply replaying
        /// the authentication response does not result in successful
        /// authentication.  Cnonce is not populated until an authentication
        /// request has been triggered, and it is cleared as soon as the 
        /// matching request is received and verified.
        /// </remarks>
        internal int Nonce
        {
            get
            {
                return _SessionManager.Nonce;
            }
            set { _SessionManager.Nonce = value; }
        }

        /// <summary>
        /// Gets or sets the URL which the User Agent will be returned to.
        /// </summary>
        public Uri ReturnToUrl
        {
            get { return _ReturnToUrl; }
            set { _ReturnToUrl = value; }
        }

        /// <summary>
        /// Gets or sets the list of Extension Plugins registered into the current state.
        /// </summary>
        public IList<IExtension> ExtensionPlugIns
        {
            get
            {
                return _ExtensionPlugIns;
            }
        }
        /// <summary>
        /// Gets or sets the list of Discovery Plugins registered into the current state.
        /// </summary>
        public IList<IDiscovery> DiscoveryPlugIns
        {
            get
            {
                return _DiscoveryPlugIns;
            }
        }

        /// <summary>
        /// Gets or sets the request arguments.
        /// </summary>
        public NameValueCollection RequestArguments
        {
            get
            {
                return _RequestArguments;
            }
        }
        /// <summary>
        /// Gets or sets the currently recorded error state.
        /// </summary>
        public ErrorCondition ErrorState
        {
            get { return _ErrorState; }
            set { _ErrorState = value; }
        }

        /// <summary>
        /// Registers a discovery plugin.
        /// </summary>
        /// <param name="plugIn">IDiscovery object to register.</param>
        public void RegisterPlugIn(IDiscovery plugIn)
        {
            for (int i = 0; i < _DiscoveryPlugIns.Count; i++)
            {
                if (_DiscoveryPlugIns[i].GetType() == plugIn.GetType())
                {
                    _DiscoveryPlugIns[i] = plugIn;
                    return;
                }
            }
            _DiscoveryPlugIns.Add(plugIn);
        }
        /// <summary>
        /// Registers an extension plugin.
        /// </summary>
        /// <param name="plugIn">IExtension object to register.</param>
        public void RegisterPlugIn(IExtension plugIn)
        {
            for (int i = 0; i < _ExtensionPlugIns.Count; i++)
            {
                if (_ExtensionPlugIns[i].GetType() == plugIn.GetType())
                {
                    _ExtensionPlugIns[i] = plugIn;
                    return;
                }
            }
            _ExtensionPlugIns.Add(plugIn);
        }
    }
}
