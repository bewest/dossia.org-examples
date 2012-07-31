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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ExtremeSwank.OAuth
{
    /// <summary>
    /// OAuth 1.0 Consumer
    /// </summary>
    /// <remarks>
    /// Implements OAuth 1.0 Core, with support for two-legged OAuth.
    /// </remarks>
    /// <example>
    /// <para>
    /// First, initialize the OAuth Consumer.
    /// </para>
    /// <code>
    /// OAuthClient client = new OAuthClient()
    /// {
    ///     ConsumerKey = "key_from_provider",
    ///     ConsumerSecret = "secret_from_provider",
    ///     RequestTokenUrl = new Uri("http://requestTokenUrl/from/provider"),
    ///     AuthorizeTokenUrl = new Uri("http://authorizeurl/from/provider"),
    ///     AccessTokenUrl = new Uri("http://accessTokenUrl/from/provider")
    /// };
    /// </code>
    /// <para>
    /// Then, get a Request Token from the service provider.
    /// </para>
    /// <code>
    /// // Get a Request Token
    /// RequestToken requestToken = client.GetRequestToken(null);
    /// 
    /// // Get user approval for the Token, directing the service provider
    /// // to send the user to the CallbackUri when finished
    /// Response.Redirect(client.GetRedirect(new Uri("http://this/url"), requestToken, null));
    /// </code>
    /// <para>
    /// Once the user has approved the request for a token, you can then request the permanent
    /// Access Token.
    /// </para>
    /// <code>
    /// AccessToken accessToken = client.GetAccessToken(requestToken);
    /// </code>
    /// <para>
    /// Once the Access Token is received, save it, associated with that user.  Any time you
    /// want to get that user's information from the service provider, just do a normal WebRequest
    /// with the OAuth credentials.
    /// </para>
    /// <code>
    /// WebRequest request = WebRequest.Create(new Uri("http://api/url"));
    /// request.Credentials = client.GetCredentials(accessToken, null);
    /// </code>
    /// </example>
    public class OAuthClient
    {
        /// <summary>
        /// The OAuth Consumer Key provided by the service provider.
        /// </summary>
        public string ConsumerKey { get; set; }
        /// <summary>
        /// The OAuth Consumer Secret privided by the service provider.
        /// </summary>
        public string ConsumerSecret { get; set; }
        /// <summary>
        /// The signature signing method that will be used.
        /// </summary>
        /// <remarks>
        /// HMAC-SHA1, RSA-SHA1, and PLAINTEXT are standard signature 
        /// formats supported by OAuth 1.0.
        /// </remarks>
        public SignatureMethod SignatureType { get; set; }
        /// <summary>
        /// The HTTP request method and authentication arugment
        /// format that will be used to request tokens
        /// from the service provider.
        /// </summary>
        /// <remarks>
        /// Arguments will be placed in the HTTP Header by default.
        /// Other options are A) passing via querystring using an HTTP GET request,
        /// or B) passing via the body of an HTTP POST request.  Only applies
        /// when performing token operations.
        /// </remarks>
        public AuthenticationMethod TokenRequestFormat { get; set; }
        /// <summary>
        /// The URL where Request Tokens can be requested.
        /// </summary>
        /// <remarks>
        /// This is provided by the OAuth Service Provider.
        /// </remarks>
        public Uri RequestTokenUrl { get; set; }
        /// <summary>
        /// The URL where a user will be directed to authorize tokens.
        /// </summary>
        /// <remarks>
        /// This is provided by the OAuth Service Provider.
        /// </remarks>
        public Uri AuthorizeTokenUrl { get; set; }
        /// <summary>
        /// The URL where Request Tokens can be upgraded to Access Tokens.
        /// </summary>
        /// <remarks>
        /// This is provided by the OAuth Service Provider.
        /// </remarks>
        public Uri AccessTokenUrl { get; set; }

        /// <summary>
        /// The X509 certificate containing the private key used for the RSA-SHA1 signing method.
        /// Optional for other signing methods.
        /// </summary>
        public X509Certificate2 RsaCertificate { get; set; }

        /// <summary>
        /// Creates a new instance of OAuthClient.
        /// </summary>
        public OAuthClient()
        {
            RegisterAuthenticationModule();
            SignatureType = SignatureMethod.HmacSha1;
            TokenRequestFormat = AuthenticationMethod.Header;
        }

        /// <summary>
        /// Register the OAuth Authentication Module.
        /// </summary>
        public static void RegisterAuthenticationModule() 
        {
            AuthenticationManager.Register(new OAuthAuthenticationModule());
        }

        #region Public Instance Methods

        /// <summary>
        /// Requests a Request Token from the OAuth Service Provider.
        /// </summary>
        /// <param name="args">Collection of additional arguments that can optionally be included in the request.</param>
        /// <returns>A populated RequestToken object.</returns>
        public RequestToken GetRequestToken(NameValueCollection args)
        {
            if (SignatureType == SignatureMethod.RsaSha1 && RsaCertificate == null) throw new RequiredPropertyNotSetException(Strings.ExRsaCertificateRequired);
            return GetRequestToken(RequestTokenUrl, args, ConsumerKey, ConsumerSecret, RsaCertificate, SignatureType, TokenRequestFormat);
        }

        /// <summary>
        /// Returns the URL that the user should be directed to in order to
        /// upgrade the Request Token to a Access Token.
        /// </summary>
        /// <param name="callbackUri">Optional URL that the user will be directed to when finished.</param>
        /// <param name="token">The Request Token to upgrade.</param>
        /// <param name="args">Optional additional arguments to include in the request.</param>
        /// <returns>The redirect URL.</returns>
        public Uri GetRedirect(Uri callbackUri, RequestToken token, NameValueCollection args)
        {
            return OAuthClient.GetAuthenticationRedirect(AuthorizeTokenUrl, callbackUri, token, args);
        }

        /// <summary>
        /// Requests an Access Token from the OAuth Service Provider.
        /// </summary>
        /// <remarks>
        /// An Access Token can only be created if a Request Token has been received,
        /// and that the Request Token has been authorized by the end-user.
        /// </remarks>
        /// <param name="token">The Request Token to upgrade.</param>
        /// <returns>A populated AccessToken.</returns>
        public AccessToken GetAccessToken(RequestToken token)
        {
            if (SignatureType == SignatureMethod.RsaSha1 && RsaCertificate == null) throw new RequiredPropertyNotSetException(Strings.ExRsaCertificateRequired);
            return GetAccessToken(AccessTokenUrl, token, ConsumerKey, ConsumerSecret, RsaCertificate, SignatureType, TokenRequestFormat);
        }
        
        /// <summary>
        /// Get a string containing the Authentication parameters that should be
        /// passed in the authentication request.  Compatible with standard OAuth
        /// authentication.
        /// </summary>
        /// <remarks>
        /// Not for the faint of heart.  For most situations, use GetCredentials(), 
        /// and pass the output to the WebRequest.Credentials property.
        /// </remarks>
        /// <param name="uri">URL for the request.</param>
        /// <param name="method">HTTP Request method.</param>
        /// <param name="token">AccessToken to use.</param>
        /// <param name="postArgs">Arguments that will be included in the POST body that need to be signed.</param>
        /// <param name="format">Format of the authentication request data.</param>
        /// <returns>String containing the OAuth authentication parameters.</returns>
        public string GetAuthParameters(Uri uri, string method, AccessToken token, NameValueCollection postArgs, AuthenticationMethod format)
        {
            if (SignatureType == SignatureMethod.RsaSha1 && RsaCertificate == null) throw new RequiredPropertyNotSetException(Strings.ExRsaCertificateRequired);
            return GetAuthParameters(uri, token, postArgs, ConsumerKey, ConsumerSecret, RsaCertificate, format, method, SignatureType);
        }

        /// <summary>
        /// Get a string containing the Authentication parameters that should be
        /// passed in the authentication request. Compatible with Two-Legged OAuth 
        /// authentication.
        /// </summary>
        /// <remarks>
        /// Not for the faint of heart.  For most situations, use GetCredentials()
        /// and pass the output to the WebRequest.Credentials property.
        /// </remarks>
        /// <param name="uri">URL for the request.</param>
        /// <param name="method">HTTP Request method.</param>
        /// <param name="requestorId">User account ID.</param>
        /// <param name="postArgs">Argumentst that will be in the POST body that need to be signed.</param>
        /// <param name="format">Format of the authentication request data.</param>
        /// <returns>String containing the OAuth authentication parameters.</returns>
        public string GetAuthParameters(Uri uri, string method, string requestorId, NameValueCollection postArgs, AuthenticationMethod format)
        {
            if (SignatureType == SignatureMethod.RsaSha1 && RsaCertificate == null) throw new RequiredPropertyNotSetException(Strings.ExRsaCertificateRequired);
            return GetAuthParameters(uri, requestorId, postArgs, ConsumerKey, ConsumerSecret, RsaCertificate, format, method, SignatureType);
        }

        /// <summary>
        /// Using an Access Token, get an ICredentials object that can be
        /// used with WebRequest.
        /// </summary>
        /// <param name="token">Valid OAuth Access Token for the remote site.</param>
        /// <param name="postArgs">If an HTTP POST request, all arguments that will be included in the POST body.  This ensures a correct OAuth signature is generated.</param>
        /// <returns>Populated ICredentials object.</returns>
        public ICredentials GetCredentials(AccessToken token, NameValueCollection postArgs)
        {
            if (SignatureType == SignatureMethod.RsaSha1 && RsaCertificate == null) throw new RequiredPropertyNotSetException(Strings.ExRsaCertificateRequired);
            OAuthCredentialsStandard creds = new OAuthCredentialsStandard() { Consumer = this, Token = token, RsaCertificate = RsaCertificate };
            if (postArgs != null) creds.Arguments.Add(postArgs);
            return creds;
        }

        /// <summary>
        /// Get an ICredentials object for two-legged OAuth authentication.
        /// </summary>
        /// <param name="requestorId">Name of the account to access.</param>
        /// <param name="postArgs">If an HTTP POST request, all arguments that will be included in the POST body.  This ensures a correct OAuth signature is generated.</param>
        /// <returns>Populated ICredentials object.</returns>
        public ICredentials GetCredentials(string requestorId, NameValueCollection postArgs)
        {
            if (SignatureType == SignatureMethod.RsaSha1 && RsaCertificate == null) throw new RequiredPropertyNotSetException(Strings.ExRsaCertificateRequired);
            OAuthCredentialsTwoLegged creds = new OAuthCredentialsTwoLegged() { Consumer = this, RequestorId = requestorId, RsaCertificate = RsaCertificate };
            if (postArgs != null) creds.Arguments.Add(postArgs);
            return creds;
        }

        #endregion

        #region Private static methods

        /// <summary>
        /// Using a Request Token, generate the URL that the user must
        /// visit in order to authorize the token.
        /// </summary>
        /// <param name="uri">Destination URL.</param>
        /// <param name="callbackUri">Optional URL to send the user back to when the token has been verified.</param>
        /// <param name="token">The Request Token to verify.</param>
        /// <param name="parameters">Optional additinal parameters to include.</param>
        /// <returns>A URL to which the user should be directed.</returns>
        static Uri GetAuthenticationRedirect(Uri uri, Uri callbackUri, RequestToken token, NameValueCollection parameters)
        {
            UriBuilder ub = new UriBuilder(uri);
            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
            if (token != null) nvc.Add(OAuthArguments.OAuthToken, token.Key);
            if (callbackUri != null) nvc.Add(OAuthArguments.OAuthCallback, callbackUri.AbsoluteUri);
            if (parameters != null) nvc.Add(parameters);
            ub.Query = OAuthUtility.ArgsToVal(nvc, AuthenticationMethod.Get);
            return ub.Uri;
        }

        /// <summary>
        /// Convert AuthenticationMethods to the appropriate
        /// HTTP method for token requests.
        /// </summary>
        /// <param name="method">Input value.</param>
        /// <returns>Appropriate HTTP method.</returns>
        static string AuthenticationMethodToString(AuthenticationMethod method)
        {
            switch (method)
            {
                case AuthenticationMethod.Get:
                    return "GET";
                case AuthenticationMethod.Header:
                    return "POST";
                case AuthenticationMethod.Post:
                    return "POST";
            }
            return "POST";
        }

        /// <summary>
        /// Get a new RequestToken from the service provider.
        /// </summary>
        /// <param name="uri">Request token URL.</param>
        /// <param name="args">Arguments to include in the request.</param>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="sigMethod">The signature signing method.</param>
        /// <param name="mode">The HTTP connection and argument format to use.</param>
        /// <param name="rsaCert">The X509 certificate containing the private key for RSA-SHA1.</param>
        /// <returns>A populated Request Token.</returns>
        static RequestToken GetRequestToken(Uri uri, NameValueCollection args, string consumerKey, string consumerSecret, X509Certificate2 rsaCert, SignatureMethod sigMethod, AuthenticationMethod mode)
        {
            NameValueCollection nvc = TokenArgs(uri, args, consumerKey, consumerSecret, null, null, rsaCert, sigMethod, AuthenticationMethodToString(mode));
            WebResponse response = Request(uri, nvc, mode);
            NameValueCollection rnvc = FormatResponse(response);
            return new RequestToken(rnvc);
        }

        /// <summary>
        /// Upgrade a Request Token to an Access Token.
        /// </summary>
        /// <param name="uri">Access token URL.</param>
        /// <param name="token">RequestToken to upgrade.</param>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="sigMethod">The signature signing method.</param>
        /// <param name="mode">The HTTP connection and argument format to use.</param>
        /// <param name="rsaCert">The X509 certificate containing the private key used for RSA-SHA1.</param>
        /// <returns>A populated AccessToken.</returns>
        static AccessToken GetAccessToken(Uri uri, RequestToken token, string consumerKey, string consumerSecret, X509Certificate2 rsaCert, SignatureMethod sigMethod, AuthenticationMethod mode)
        {
            NameValueCollection nvc = TokenArgs(uri, null, consumerKey, consumerSecret, token.Key, token.Secret, rsaCert, sigMethod, AuthenticationMethodToString(mode));
            WebResponse response = Request(uri, nvc, mode);
            NameValueCollection rparams = FormatResponse(response);
            return new AccessToken(rparams);
        }

        /// <summary>
        /// Generate arguments for Token requests.
        /// </summary>
        /// <remarks>
        /// For Request tokens, leave tokenName and tokenSecret as null.
        /// For Accept tokens, leave args as null.
        /// </remarks>
        /// <param name="uri">Token operation URL.</param>
        /// <param name="postArgs">HTTP POST arguments to include in signature generation.</param>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="tokenName">The token key, if required.</param>
        /// <param name="tokenSecret">The token secret, if required.</param>
        /// <param name="sigMethod">Signature generation method.</param>
        /// <param name="method">HTTP method for the request.</param>
        /// <param name="rsaCert">The X509 certificate containing the private key used for RSA-SHA1.</param>
        /// <returns>All required arguments for the Token request.</returns>
        static NameValueCollection TokenArgs(Uri uri, NameValueCollection postArgs, string consumerKey, string consumerSecret, string tokenName, string tokenSecret, X509Certificate2 rsaCert, SignatureMethod sigMethod, string method)
        {
            NameValueCollection nvc = new NameValueCollection();
            if (postArgs != null) nvc.Add(postArgs);
            nvc[OAuthArguments.OAuthConsumerKey] = consumerKey;
            if (!String.IsNullOrEmpty(tokenName)) nvc[OAuthArguments.OAuthToken] = tokenName;
            nvc[OAuthArguments.OAuthSignatureMethod] = OAuthUtility.SigMethodToString(sigMethod);
            long tstamp = OAuthUtility.Timestamp();
            nvc[OAuthArguments.OAuthTimestamp] = tstamp.ToString(CultureInfo.InvariantCulture);
            nvc[OAuthArguments.OAuthNonce] = Nonce(tstamp);
            nvc[OAuthArguments.OAuthVersion] = "1.0";
            string sig = OAuthUtility.GenerateSignature(OAuthUtility.GenerateBaseString(uri, nvc, method), consumerSecret, tokenSecret, rsaCert, sigMethod);
            Trace.WriteLine(sig, "AuthSignature");
            nvc[OAuthArguments.OAuthSignature] = sig;
            // Additional arguments for signing should not be included in the authentication data.
            if (postArgs != null)
            {
                foreach (string key in postArgs.Keys)
                {
                    nvc.Remove(key);
                }
            }
            return nvc;
        }

        /// <summary>
        /// Get authentication parameters to access an OAuth
        /// protected resource.
        /// </summary>
        /// <param name="uri">Destination URL.</param>
        /// <param name="token">Valid access token.</param>
        /// <param name="arguments">Arguments to include in the request.</param>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="format">The format of the resulting string.</param>
        /// <param name="method">HTTP method that will be used during the request.</param>
        /// <param name="sigMethod">Signature signing method.</param>
        /// <param name="rsaCert">The X509 certificate containing the private key used for RSA-SHA1.</param>
        /// <returns>String containing all authentication parameters, in the specified format.</returns>
        internal static string GetAuthParameters(Uri uri, AccessToken token, NameValueCollection arguments, string consumerKey, string consumerSecret, X509Certificate2 rsaCert, AuthenticationMethod format, string method, SignatureMethod sigMethod)
        {
            NameValueCollection sigColl = new NameValueCollection();
            if (arguments != null) sigColl.Add(arguments);
           
            NameValueCollection queryArgs = HttpUtility.ParseQueryString(uri.Query);
            sigColl.Add(queryArgs);
            NameValueCollection nvc = TokenArgs(uri, sigColl, consumerKey, consumerSecret, token.Key, token.Secret, rsaCert, sigMethod, method);

            foreach (string key in queryArgs.Keys)
            {
                nvc.Remove(key);
            }

            return OAuthUtility.ArgsToVal(nvc, format);
        }

        /// <summary>
        /// Get authentication parameters to access an OAuth
        /// protected resource.  Used for two-legged OAuth.
        /// </summary>
        /// <param name="uri">Destination URL.</param>
        /// <param name="requestorId">Name of the user account at the remote site.</param>
        /// <param name="arguments">Arguments to include in the request.</param>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="format">The format of the resulting string.</param>
        /// <param name="method">HTTP method that will be used during the request.</param>
        /// <param name="rsaCert">The X509 certificate containing the private key used for RSA-SHA1.</param>
        /// <param name="sigMethod">Signature signing method.</param>
        /// <returns>String containing all authentication parameters, in the specified format.</returns>
        internal static string GetAuthParameters(Uri uri, string requestorId, NameValueCollection arguments, string consumerKey, string consumerSecret, X509Certificate2 rsaCert, AuthenticationMethod format, string method, SignatureMethod sigMethod)
        {
            NameValueCollection sigColl = new NameValueCollection();
            if (arguments != null) sigColl.Add(arguments);
            sigColl[OAuthArguments.XOAuthRequestorId] = requestorId;

            NameValueCollection queryArgs = HttpUtility.ParseQueryString(uri.Query);
            sigColl.Add(queryArgs);
            NameValueCollection nvc = TokenArgs(uri, sigColl, consumerKey, consumerSecret, null, null, rsaCert, sigMethod, method);

            foreach (string key in queryArgs.Keys)
            {
                nvc.Remove(key);
            }

            return OAuthUtility.ArgsToVal(nvc, format);
        }

        /// <summary>
        /// Perform an HTTP request with OAuth authentication arguments.
        /// </summary>
        /// <remarks>
        /// Used for Token requests.
        /// </remarks>
        /// <param name="uri">Destination URL.</param>
        /// <param name="oauthArguments">Arguments to include in the OAuth authentication data.</param>
        /// <param name="format">The required OAuth argument format.</param>
        /// <returns>The resulting WebResponse object.</returns>
        static WebResponse Request(Uri uri, NameValueCollection oauthArguments, AuthenticationMethod format)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            string realm = OAuthUtility.Realm(uri);

            Trace.WriteLine(uri.AbsoluteUri, "TokenWebRequest");

            WebResponse response = null;
            switch (format)
            {
                case AuthenticationMethod.Header:
                    request.Method = "POST";
                    string headerstr = String.Format(CultureInfo.InvariantCulture, "{0} realm={1}, {2}", Strings.OAuth, OAuthUtility.Realm(uri), OAuthUtility.ArgsToVal(oauthArguments, AuthenticationMethod.Header));
                    Trace.WriteLine(headerstr, "HttpHeader_Authorization");
                    request.Headers.Add(HttpRequestHeader.Authorization, headerstr);
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = 0;
                    break;
                case AuthenticationMethod.Get:
                    request.Method = "GET";
                    UriBuilder ub = new UriBuilder(uri);
                    if (String.IsNullOrEmpty(ub.Query))
                    {
                        ub.Query = OAuthUtility.ArgsToVal(oauthArguments, format);
                    }
                    else
                    {
                        ub.Query += "&" + OAuthUtility.ArgsToVal(oauthArguments, format);
                    }
                    Trace.WriteLine(ub.Uri.Query, "GetRequestArguments");
                    request = (HttpWebRequest)HttpWebRequest.Create(ub.Uri);
                    break;
                case AuthenticationMethod.Post:
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    string vals = OAuthUtility.ArgsToVal(oauthArguments, format);
                    Trace.WriteLine(vals, "PostRequestArguments");
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(vals);
                    request.ContentLength = data.Length;
                    Stream s = request.GetRequestStream();
                    s.Write(data, 0, data.Length);
                    s.Close();
                    break;
            }
            try
            {
                response = request.GetResponse();
            }
            catch (WebException e)
            {
                string err = null;
                if (e.Response != null)
                {
                    using (StreamReader sr = new StreamReader(e.Response.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                    }
                    HttpWebResponse hwr = (HttpWebResponse)e.Response;
                    Trace.Write(string.Format(CultureInfo.InvariantCulture, "HTTP Status Code: {0}, Description: {1}", hwr.StatusCode, hwr.StatusDescription));
                    throw new WebException(String.Format(CultureInfo.CurrentCulture, "Message Received From Server: {0}", err), e);
                }
                throw;
            }
            return response;
        }

        /// <summary>
        /// Generates a nonce value.
        /// </summary>
        /// <param name="input">64-bit integer representing the number of seconds since the epoch.</param>
        /// <returns>A hexadecimal-encoded randomly generated value.</returns>
        static string Nonce(long input)
        {
            Random r = new Random(DateTime.Now.Millisecond);
            string val = (input + r.Next()).ToString(CultureInfo.InvariantCulture);
            StringBuilder sb = new StringBuilder();
            foreach (char c in val)
            {
                sb.Append(Convert.ToInt32(c).ToString("x2", CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Extracts all arguments from a WebResponse and builds
        /// a NameValueCollection.
        /// </summary>
        /// <param name="response">Input web response.</param>
        /// <returns>Populated NameValueCollection containing all arguments.</returns>
        static NameValueCollection FormatResponse(WebResponse response)
        {
            string content = null;
            NameValueCollection nvc = new NameValueCollection();
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                content = sr.ReadToEnd();
            }
            Trace.WriteLine(content, "TokenResponse");
            if (!String.IsNullOrEmpty(content))
            {
                nvc = HttpUtility.ParseQueryString(content);
            }
            return nvc;
        }

        #endregion
    }
}
