// Copyright (c) 2009 John Ehn, ExtremeSwank.com
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Specialized;
using ExtremeSwank.OAuth;
using ExtremeSwank.OpenId.PlugIns.Discovery;

namespace ExtremeSwank.OpenId.PlugIns.Extensions
{
    /// <summary>
    /// OpenID+OAuth Extension 1.0
    /// </summary>
    public class OAuthExtension : IExtension
    {
        StateContainer _Parent;
        const string prefix = "openid.oauth.";

        /// <summary>
        /// Create a new instance of OAuthExtension.
        /// </summary>
        /// <param name="state">OpenIdClient's StateContainer object.</param>
        /// <param name="oauthClient">The OAuth Consumer to use.</param>
        public OAuthExtension(StateContainer state, OAuthClient oauthClient)
        {
            _Parent = state;
            state.RegisterPlugIn(this);
            OAuthClient = oauthClient;
        }

        /// <summary>
        /// Create a new instance of OAuthExtension.
        /// </summary>
        /// <param name="core">The OpenIdClient to use.</param>
        /// <param name="oauthClient">The OAuth Consumer to use.</param>
        public OAuthExtension(ClientCore core, OAuthClient oauthClient)
        {
            _Parent = core.StateContainer;
            _Parent.RegisterPlugIn(this);
            OAuthClient = oauthClient;
        }

        /// <summary>
        /// Set the scope of the extension, per the OAuth specification.
        /// </summary>
        /// <remarks>
        /// This value is specific to the service provider.
        /// </remarks>
        public string Scope { get; set; }

        /// <summary>
        /// The OAuth Consumer to use.
        /// </summary>
        public OAuthClient OAuthClient { get; set; }

        #region IExtension Members

        /// <summary>
        /// The human-readable name of the OpenID extension.
        /// </summary>
        public string Name
        {
            get { return "OpenID OAuth Extension 1.0"; }
        }

        /// <summary>
        /// The Parent StateContainer object linked to the OpenID Consumer.
        /// </summary>
        public StateContainer Parent
        {
            get { return _Parent; }
        }

        /// <summary>
        /// The namespace of this OpenID extension.
        /// </summary>
        public Uri NamespaceUri
        {
            get { return new Uri("http://specs.openid.net/extensions/oauth/1.0"); }
        }

        /// <summary>
        /// Build the authentication arguments that will be sent with the OpenID
        /// request.
        /// </summary>
        /// <param name="discResult">Discovery result data.</param>
        /// <returns>A populated dictionary containing the request arguments.</returns>
        public NameValueCollection BuildAuthorizationData(DiscoveryResult discResult)
        {
            NameValueCollection dict = new NameValueCollection();
            dict.Add("openid.ns.oauth", NamespaceUri.AbsoluteUri);
            dict.Add(prefix + "consumer", OAuthClient.ConsumerKey);
            dict.Add(prefix + "scope", Scope);
            return dict;
        }

        /// <summary>
        /// Perform response validation. Always returns true.
        /// </summary>
        /// <returns>True.</returns>
        public bool Validation()
        {
            return true;
        }

        /// <summary>
        /// Populate the OpenIDUser object.  This method does nothing.
        /// </summary>
        /// <param name="userObject">The OpenIdUser object to populate.</param>
        public void PopulateUserObject(OpenIdUser userObject)
        {
            return;
        }

        /// <summary>
        /// Get the OAuth Access Token from the current OpenID response.
        /// </summary>
        /// <returns>A populated AccessToken.</returns>
        public AccessToken GetAccessToken()
        {
            NameValueCollection request = Parent.RequestArguments;

            NameValueCollection ds = Utility.GetExtNamespaceAliases(request);
            if (ds[NamespaceUri.AbsoluteUri] == null) return null;
            string p = ds[NamespaceUri.AbsoluteUri];
            string _pre = "openid." + p + ".";

            NameValueCollection rta = new NameValueCollection();
            rta["oauth_token"] = request[_pre + "request_token"];
            rta["scope"] = request[_pre + "scope"];
            RequestToken requestToken = new RequestToken(rta);

            return OAuthClient.GetAccessToken(requestToken);
        }

        #endregion
    }
}
