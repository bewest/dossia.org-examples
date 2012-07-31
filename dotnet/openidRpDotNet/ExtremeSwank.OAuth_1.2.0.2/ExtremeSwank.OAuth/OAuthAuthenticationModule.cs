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

using System.Diagnostics;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Web;

namespace ExtremeSwank.OAuth
{
    /// <summary>
    /// Provides a module compatible with the .NET security framework.
    /// </summary>
    class OAuthAuthenticationModule : IAuthenticationModule
    {
        #region IAuthenticationModule Members

        /// <summary>
        /// Return OAuth authentication data based on the supplied credentials.
        /// </summary>
        /// <param name="challenge">Authentication challenge</param>
        /// <param name="request">Web request</param>
        /// <param name="credentials">OAuth credentials object</param>
        /// <returns>Populated Authorization object.</returns>
        public Authorization Authenticate(string challenge, WebRequest request, ICredentials credentials)
        {
            return Auth(request, credentials);
        }

        /// <summary>
        /// The authentication type.  Always returns "OAuth".
        /// </summary>
        public string AuthenticationType
        {
            get { return "OAuth"; }
        }

        /// <summary>
        /// This module is able to perform pre-authentication.
        /// </summary>
        public bool CanPreAuthenticate
        {
            get { return true; }
        }

        /// <summary>
        /// Pre-authenticate the web request.
        /// </summary>
        /// <param name="request">Current web request.</param>
        /// <param name="credentials">OAuthCredentials object.</param>
        /// <returns>Populated Authorization object.</returns>
        public Authorization PreAuthenticate(WebRequest request, ICredentials credentials)
        {
            return Auth(request, credentials);
        }

        #endregion

        /// <summary>
        /// Authenticate the web request.
        /// </summary>
        /// <param name="request">Current web request.</param>
        /// <param name="credentials">OAuthCredentials object.</param>
        /// <returns>Populated Authorization object.</returns>
        static Authorization Auth(WebRequest request, ICredentials credentials)
        {
            OAuthCredentialsStandard cred = credentials as OAuthCredentialsStandard;
            if (cred != null)
            {
                NameValueCollection postArgs = new NameValueCollection();
                string realm = OAuthUtility.Realm(request.RequestUri);
                string authstr = "OAuth realm=\"" + realm + "\", " + OAuthClient.GetAuthParameters(request.RequestUri, cred.Token, cred.Arguments, cred.Consumer.ConsumerKey, cred.Consumer.ConsumerSecret, cred.RsaCertificate, AuthenticationMethod.Header, request.Method, cred.Consumer.SignatureType);
                Trace.WriteLine(authstr, "Credentials");
                Authorization auth = new Authorization(authstr);
                return auth;
            }

            OAuthCredentialsTwoLegged tcred = credentials as OAuthCredentialsTwoLegged;
            if (tcred != null)
            {
                string realm = OAuthUtility.Realm(request.RequestUri);
                string authstr = "OAuth realm=\"" + realm + "\", " + OAuthClient.GetAuthParameters(request.RequestUri, tcred.RequestorId, tcred.Arguments, tcred.Consumer.ConsumerKey, tcred.Consumer.ConsumerSecret, tcred.RsaCertificate, AuthenticationMethod.Header, request.Method, tcred.Consumer.SignatureType);
                Trace.WriteLine(authstr, "Credentials");
                Authorization auth = new Authorization(authstr);
                return auth;
            }

            return null;
        }
    }
}
