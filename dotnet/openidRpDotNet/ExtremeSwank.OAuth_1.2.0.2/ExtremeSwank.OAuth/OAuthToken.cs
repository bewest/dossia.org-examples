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
using System.Globalization;

namespace ExtremeSwank.OAuth
{
    /// <summary>
    /// Abstract OAuth Token object.
    /// </summary>
    /// <remarks>
    /// OAuth uses Tokens for authentication.  Request Tokens
    /// are temporary tokens which must be approved by the 
    /// end user.  Approved Request Tokens can be exchanged for
    /// Access Tokens, which are permanent, and can be used for
    /// subsequent authentication requests.
    /// </remarks>
    [Serializable]
    public abstract class OAuthToken
    {
        /// <summary>
        /// The key of the token, as provided by the service
        /// provider.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// The token's secret, as provided by the service
        /// provider.
        /// </summary>
        public string Secret { get; set; }
        /// <summary>
        /// Additional parameters received from the service provider.
        /// </summary>
        public NameValueCollection Parameters { get; private set; }

        /// <summary>
        /// Create a new instance of OAuthToken.
        /// </summary>
        protected OAuthToken() 
        {
            Parameters = new NameValueCollection();
        }

        /// <summary>
        /// Creates a new instance of OAuthToken.
        /// </summary>
        /// <remarks>
        /// The raw arguments received in the response from the
        /// service provider.
        /// </remarks>
        /// <param name="args">Arguments used to populate the token's properties.</param>
        protected OAuthToken(NameValueCollection args)
        {
            Parameters = new NameValueCollection();
            if (args != null)
            {
                Key = args[OAuthArguments.OAuthToken];
                Secret = args[OAuthArguments.OAuthTokenSecret];
            }
            foreach (string key in args.Keys)
            {
                string k = key.ToUpper(CultureInfo.InvariantCulture);
                if (k != OAuthArguments.OAuthToken.ToUpper(CultureInfo.InvariantCulture) && k != OAuthArguments.OAuthTokenSecret.ToUpper(CultureInfo.InvariantCulture))
                {
                    Parameters.Add(key, args[key]);
                }
            }
        }

        /// <summary>
        /// Re-build a object deriving from OAuthToken using
        /// serialized data from the object's Export method.
        /// </summary>
        /// <typeparam name="T">
        /// Type to restore. Must derive from OAuthToken, 
        /// and type must match the object data.
        /// </typeparam>
        /// <param name="objectData">The serialized object data.</param>
        /// <returns>The specifed token object.</returns>
        public static T Restore<T>(string objectData) where T : OAuthToken
        {
            return OAuthUtility.Deserialize<T>(objectData);
        }

        /// <summary>
        /// Serialize the object to a string that can be used
        /// to restore the object at a later time.
        /// </summary>
        /// <returns>The serialized object data.</returns>
        public string Export()
        {
            return OAuthUtility.Serialize(this);
        }
    }

    /// <summary>
    /// Request Tokens are temporary tokens used to generate
    /// Access Tokens.
    /// </summary>
    [Serializable]
    public class RequestToken : OAuthToken
    {
        /// <summary>
        /// Create a new instance of RequestToken.
        /// </summary>
        protected RequestToken() : base() { }

        /// <summary>
        /// Creates a new instance of RequestToken.
        /// </summary>
        /// <param name="args">Token arguments.</param>
        public RequestToken(NameValueCollection args) : base(args) { }
    }

    /// <summary>
    /// Access Tokens are permanent tokens which can be used
    /// for OAuth authentication requests.
    /// </summary>
    [Serializable]
    public class AccessToken : OAuthToken
    {
        /// <summary>
        /// Creates a new instance of AccessToken.
        /// </summary>
        protected AccessToken() : base() { }

        /// <summary>
        /// Creates a new instance of AccessToken.
        /// </summary>
        /// <param name="args">Token arguments.</param>
        public AccessToken(NameValueCollection args) : base(args) { }
    }

    /// <summary>
    /// Server-side representation of an OAuth Request Token.
    /// </summary>
    /// <remarks>
    /// Holds additional server-side arguments that are needed to
    /// handle authentication requests.
    /// </remarks>
    [Serializable]
    public class ServerRequestToken : RequestToken
    {
        /// <summary>
        /// The Consumer Key associated with this token.
        /// </summary>
        public string ConsumerKey { get; set; }
        /// <summary>
        /// The Token Key of the associated Access Token, if there is one.
        /// </summary>
        public string AccessTokenKey { get; set; }

        /// <summary>
        /// Create a new instance of ServerRequestToken.
        /// </summary>
        /// <param name="consumerKey">Consumer key to associate with this token.</param>
        /// <param name="tokenKey">Key for this token.</param>
        /// <param name="tokenSecret">Secret for this token.</param>
        /// <param name="accessTokenKey">The key of the associated Access Token.</param>
        /// <param name="parameters">Optional parameters to add to the token.</param>
        public ServerRequestToken(string consumerKey, string tokenKey, string tokenSecret, string accessTokenKey, NameValueCollection parameters)
        {
            ConsumerKey = consumerKey;
            Key = tokenKey;
            Secret = tokenSecret;
            AccessTokenKey = accessTokenKey;
            if (parameters != null) Parameters.Add(parameters);
        }
    }

    /// <summary>
    /// Server-side representation of an OAuth Access Token.
    /// </summary>
    /// <remarks>
    /// Holds additional server-side arguments that are needed to
    /// handle authentication requests.
    /// </remarks>
    [Serializable]
    public class ServerAccessToken : AccessToken
    {
        /// <summary>
        /// Key of the Consumer associated with this token.
        /// </summary>
        public string ConsumerKey { get; set; }
        /// <summary>
        /// User account associated with this token.
        /// </summary>
        public string UserAccount { get; set; }

        /// <summary>
        /// Create a new instance of ServerAccessToken.
        /// </summary>
        /// <param name="consumerKey">The key of the associated consumer.</param>
        /// <param name="tokenKey">This token's key.</param>
        /// <param name="tokenSecret">This token's secret.</param>
        /// <param name="userAccount">The associated user account.</param>
        /// <param name="parameters">Optional parameters.</param>
        public ServerAccessToken(string consumerKey, string tokenKey, string tokenSecret, string userAccount, NameValueCollection parameters)
        {
            ConsumerKey = consumerKey;
            Key = tokenKey;
            Secret = tokenSecret;
            UserAccount = userAccount;
            if (parameters != null) Parameters.Add(parameters);
        }
    }
}
