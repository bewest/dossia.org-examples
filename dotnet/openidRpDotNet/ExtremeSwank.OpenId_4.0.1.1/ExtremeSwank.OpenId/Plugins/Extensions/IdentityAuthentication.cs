using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using ExtremeSwank.OpenId.PlugIns.Discovery;

namespace ExtremeSwank.OpenId.PlugIns.Extensions
{
    /// <summary>
    /// Implements the optional "identity" and "claimed_id" fields.
    /// Loaded automatically by <see cref="OpenIdClient"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// OpenID 2.0 includes support for extensions that do not necessarily
    /// require that an Identity be authenticated.  This can be used in
    /// some cases where you want the Consumer and the OpenID Provider
    /// to exchange information that does not involve a specific user
    /// account.
    /// </para>
    /// <para>
    /// As of this writing, there are currently no OpenID extensions 
    /// which use this feature of the protocol.
    /// </para>
    /// <para>
    /// This extension plug-in must be used for OpenID 1.1 servers, and
    /// is optional for OpenID 2.0 servers, under the conditions stated above.  
    /// Should only be omitted if another Extension plug-in is used, and the 
    /// extension in question does not require identity assertion.
    /// </para>
    /// </remarks>
    public class IdentityAuthentication : IExtension
    {
        const string _ExtensionName = "OpenID Authentication";
        StateContainer _Parent;

        #region IExtension Members

        /// <summary>
        /// Gets the human-readable name of this plug-in.
        /// </summary>
        public string Name
        {
            get { return _ExtensionName; }
        }

        /// <summary>
        /// Gets the parent StateContainer object.
        /// </summary>
        public StateContainer Parent
        {
            get { return _Parent; }
        }

        /// <summary>
        /// Gets the extension URI that this plug-in implements.
        /// </summary>
        public Uri NamespaceUri
        {
            get { return ProtocolUri.OpenId2Dot0; }
        }

        /// <summary>
        /// Gets the information that will be sent in the authentication
        /// request to the OpenID Provider.
        /// </summary>
        /// <param name="discResult">The DiscoveryResult object to use.</param>
        public NameValueCollection BuildAuthorizationData(DiscoveryResult discResult)
        {
            NameValueCollection pms = new NameValueCollection();

            pms["openid.identity"] = discResult.LocalId;

            if (discResult.AuthVersion == ProtocolVersion.V2Dot0)
            {
                pms["openid.claimed_id"] = discResult.ClaimedId;
            }
            else
            {
                pms["esoid.ReturnUrl"] = "esoid.claimed_id=" + HttpUtility.UrlEncode(discResult.ClaimedId);
            }
            return pms;
        }

        /// <summary>
        /// After a response has been received from the Identity Provider,
        /// performs a extension or plug-in specific check to ensure the
        /// response is valid.
        /// </summary>
        /// <returns>True if valid, false if not.</returns>
        /// <remarks>
        /// If the response is found to be invalid, OpenIdClient will
        /// fail authentication validation.
        /// </remarks>
        public bool Validation()
        {
            return true;
        }

        /// <summary>
        /// Gets the user object data needed to populate an <see cref="OpenIdUser"/> object.
        /// </summary>
        /// <remarks>
        /// Specifically sets the Identity and BaseIdentity properties of
        /// the OpenIdUser object.
        /// </remarks>
        /// <param name="userObject">The OpenIdUser object to populate.</param>
        public void PopulateUserObject(OpenIdUser userObject)
        {
            NameValueCollection arguments = Parent.RequestArguments;
            
            string identity = arguments["openid.identity"];
            string claimed_id = null;
            string local_id = null;

            if (userObject.LastDiscoveryResult != null) 
            { 
                claimed_id = userObject.LastDiscoveryResult.ClaimedId;
                local_id = userObject.LastDiscoveryResult.LocalId;
            }

            userObject.BaseIdentity = identity;

            if (!String.IsNullOrEmpty(claimed_id)) { userObject.Identity = claimed_id; }

            if (claimed_id == local_id) { userObject.Identity = Utility.Normalize(identity, Parent.DiscoveryPlugIns).FriendlyId; }
            else { userObject.Identity = Utility.Normalize(claimed_id, Parent.DiscoveryPlugIns).FriendlyId; }
        }

        #endregion

        /// <summary>
        /// Creates a new Authentication plugin and registers it with a StateContainer object.
        /// </summary>
        /// <param name="state">Parent <see cref="StateContainer"/> object to attach.</param>
        public IdentityAuthentication(StateContainer state)
        {
            _Parent = state;
            _Parent.RegisterPlugIn(this);
        }

        /// <summary>
        /// Creates a new Authentication plugin and registers it with ClientCore object.
        /// </summary>
        /// <param name="client">The <see cref="ClientCore"/> object to attach.</param>
        public IdentityAuthentication(ClientCore client)
        {
            _Parent = client.StateContainer;
            _Parent.RegisterPlugIn(this);
        }
    }
}
