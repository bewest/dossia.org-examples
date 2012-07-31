using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using ExtremeSwank.OpenId.PlugIns.Discovery;

namespace ExtremeSwank.OpenId.PlugIns.Extensions
{
    /// <summary>
    /// Provides support for the OpenID Provider Authentication Policy Extension.
    /// </summary>
    /// <remarks>
    /// Not all OpenID Providers support all OpenID extensions.  If the expected data is
    /// not returned after a successful request, the OpenID Provider may not support this
    /// extension.
    /// </remarks>
    public class AuthenticationPolicy : IExtension
    {
        StateContainer _Parent;

        /// <summary>
        /// Creates an instance of AuthenticationPolicy extension.
        /// </summary>
        /// <param name="state">The parent StateContainer object.</param>
        public AuthenticationPolicy(StateContainer state)
        {
            _Parent = state;
            _Parent.RegisterPlugIn(this);
            _PreferredPolicies = new List<Uri>();
        }

        /// <summary>
        /// Creates an instance of AuthenticationPolicy extension.
        /// </summary>
        /// <param name="client">The parent <see cref="ClientCore"/> object.</param>
        public AuthenticationPolicy(ClientCore client)
        {
            _Parent = client.StateContainer;
            _Parent.RegisterPlugIn(this);
            _PreferredPolicies = new List<Uri>();
        }

        private long _MaxAge;

        /// <summary>
        /// The longest period of time that can pass since the user was
        /// last authenticated by the Identity Provider.
        /// </summary>
        public long MaxAge
        {
            get { return _MaxAge; }
            set { _MaxAge = value; }
        }

        private List<Uri> _PreferredPolicies;

        /// <summary>
        /// A List of preferred policy URIs that are requested for this authentication
        /// request.  Use arguments from <see cref="AuthenticationPolicySchema"/>.
        /// </summary>
        public IList<Uri> PreferredPolicies
        {
            get { return _PreferredPolicies; }
        }

        #region IExtension Members

        /// <summary>
        /// The human-readable name of this extension.
        /// </summary>
        public string Name
        {
            get { return "OpenID Provider Authentication Policy Extension (PAPE)"; }
        }

        /// <summary>
        /// The StateContainer object that is parent to this extension.
        /// </summary>
        public StateContainer Parent
        {
            get { return _Parent; }
        }

        /// <summary>
        /// The namespace URI of this extension.
        /// </summary>
        public Uri NamespaceUri
        {
            get { return ProtocolUri.AuthenticationPolicy1Dot0; }
        }

        /// <summary>
        /// Name-Value data to be sent to Identity Provider during
        /// initial authentication request.
        /// </summary>
        /// <param name="discResult">The DiscoveryResult object to use.</param>
        public NameValueCollection BuildAuthorizationData(DiscoveryResult discResult)
        {
            NameValueCollection pms = new NameValueCollection();
            pms["openid.ns.pape"] = NamespaceUri.AbsoluteUri;
            pms["openid.pape.max_auth_age"] = MaxAge.ToString(CultureInfo.InvariantCulture);

            List<string> list = new List<string>();
            foreach (Uri u in _PreferredPolicies)
            {
                list.Add(u.AbsoluteUri);
            }
            pms["openid.pape.preferred_auth_policies"] = String.Join(" ", list.ToArray());
            return pms;
        }

        /// <summary>
        /// Whether or not the validation completed per this extension.
        /// </summary>
        /// <returns>Always returns true.</returns>
        public bool Validation()
        {
            return true;
        }

        /// <summary>
        /// Returns data for use by OpenIdUser object.
        /// </summary>
        /// <param name="userObject">The OpenIdUser object to populate.</param>
        public void PopulateUserObject(OpenIdUser userObject)
        {
            NameValueCollection Request = Parent.RequestArguments;
            foreach (string key in Request.Keys)
            {
                if (key != null && key.Contains("openid.pape."))
                {
                    if (Request[key] != null)
                    {
                        userObject.ExtensionData[key] = Request[key];
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Get the human-readable name of this plug-in.
        /// </summary>
        /// <returns>A string containing the plug-in name.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
