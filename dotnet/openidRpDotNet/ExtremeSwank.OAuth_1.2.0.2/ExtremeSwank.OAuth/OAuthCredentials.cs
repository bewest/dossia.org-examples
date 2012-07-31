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
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace ExtremeSwank.OAuth
{
    /// <summary>
    /// OAuth-specific credential data to be used during client HTTP requests.
    /// </summary>
    abstract class OAuthCredentials : ICredentials
    {
        /// <summary>
        /// OAuth Consumer to use to facilitate the authentication.
        /// </summary>
        public OAuthClient Consumer { get; set; }
        /// <summary>
        /// Additional arguments to include in the request.
        /// </summary>
        public NameValueCollection Arguments { get; private set; }
        /// <summary>
        /// The X509 certificate containing the private key to use for RSA-SHA1 signing.
        /// Not necessary for other signing methods.
        /// </summary>
        public X509Certificate2 RsaCertificate { get; set; }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        protected OAuthCredentials()
        {
            Arguments = new NameValueCollection();
        }

        #region ICredentials Members

        /// <summary>
        /// Gets a NetworkCredential object for username/password authentication.
        /// Always returns null.
        /// </summary>
        /// <param name="uri">Destination URL.</param>
        /// <param name="authType">Authentication type.</param>
        /// <returns>Always returns null.</returns>
        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            return null;
        }

        #endregion
    }

    /// <summary>
    /// Standard OAuth credentials for HTTP requests.
    /// </summary>
    class OAuthCredentialsStandard : OAuthCredentials
    {
        /// <summary>
        /// The Access Token to use to access the protected resource.
        /// </summary>
        public AccessToken Token { get; set; }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        public OAuthCredentialsStandard() : base() { }
    }

    /// <summary>
    /// Two-Legged OAuth credentials for HTTP requests.
    /// </summary>
    class OAuthCredentialsTwoLegged : OAuthCredentials
    {
        /// <summary>
        /// The ID of the user account at the service provider.
        /// </summary>
        public string RequestorId { get; set; }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        public OAuthCredentialsTwoLegged() : base() { }
    }
}
