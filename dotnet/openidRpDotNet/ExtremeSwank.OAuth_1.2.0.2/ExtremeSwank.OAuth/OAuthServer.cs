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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Security.Cryptography.X509Certificates;

namespace ExtremeSwank.OAuth
{
    /// <summary>
    /// OAuth mode for processing token requests.
    /// </summary>
    public enum ServerRequestMode
    {
        /// <summary>
        /// Current request is for a Request Token.
        /// </summary>
        RequestToken,
        /// <summary>
        /// Current request is for an Access Token.
        /// </summary>
        AccessToken
    }

    /// <summary>
    /// Server-side OAuth 1.0 implementation.
    /// </summary>
    public static class OAuthServer
    {
        /// <summary>
        /// Convert a public certificate in PEM text blob format to an
        /// X509 certificate.
        /// </summary>
        /// <param name="input">String containing public certificate in PEM format.</param>
        /// <returns>A populated X509 certificate.s</returns>
        public static X509Certificate2 PemToCertificate(string input)
        {
            input = input.Replace("\r", "").Replace("\n", "");
            input = input.Substring(input.IndexOf("-----BEGIN CERTIFICATE-----", StringComparison.Ordinal) + 27);
            input = input.Substring(0, input.IndexOf("-----END CERTIFICATE-----", StringComparison.Ordinal));
            Console.WriteLine(input);
            byte[] data = Convert.FromBase64String(input);
            X509Certificate2 cert = new X509Certificate2(data);            
            return cert;
        }
        
        /// <summary>
        /// Get the configured Token Storage provider from the current web.config file.
        /// Should only be used in ASP.NET environments.
        /// </summary>
        /// <returns>Configured Token Storage provider.</returns>
        public static IServerTokenStore GetConfiguredStorageProvider()
        {
            string tokenstoragetype = ConfigurationSettings.AppSettings[Strings.TokenStorageProvider];
            if (String.IsNullOrEmpty(tokenstoragetype)) throw new ConfigurationErrorsException(String.Format(CultureInfo.InvariantCulture, "{0} setting is not set in the web.config file.", Strings.TokenStorageProvider));
            Type t = Type.GetType(tokenstoragetype);
            return (IServerTokenStore)Activator.CreateInstance(t, true);
        }

        /// <summary>
        /// Generate a psuedo-random string value (SHA-256 hash).
        /// </summary>
        /// <returns>A pseudo-random value.</returns>
        public static string GenerateRandomValue()
        {
            Guid g1 = Guid.NewGuid();
            Guid g2 = Guid.NewGuid();
            DateTime now = DateTime.UtcNow;
            int millseconds = now.Millisecond;
            int randomInt = new Random(now.Millisecond).Next();

            string inStr = g1.ToString(null, CultureInfo.InvariantCulture) + g2.ToString(null, CultureInfo.InvariantCulture) + now.Ticks.ToString(CultureInfo.InvariantCulture) + randomInt.ToString(CultureInfo.InvariantCulture);
            SHA256 sha = SHA256.Create();
            byte[] data = Encoding.UTF8.GetBytes(inStr);
            byte[] hash = sha.ComputeHash(data, 0, data.Length);
            string retval = Convert.ToBase64String(hash);
            Trace.WriteLine(retval, "GenerateRandomValue");
            return retval;
        }

        /// <summary>
        /// Authenticate a user HTTP Request.
        /// </summary>
        /// <param name="request">HttpRequest to authenticate.</param>
        /// <param name="store">Token storage object to use.</param>
        /// <returns>If valid, returns the matching ServerAccessToken.  If not valid, returns null.</returns>
        public static ServerAccessToken AuthenticateUser(HttpRequest request, IServerTokenStore store) 
        {
            AbstractRequest arequest = new AbstractRequest(request);
            NameValueCollection nvc = DecodeRequest(arequest);
            if (nvc == null) return null;
            return AuthenticateUser(arequest.Url, arequest.HttpMethod, nvc, store);
        }

        /// <summary>
        /// Authenticate a user HTTP Request.
        /// </summary>
        /// <param name="request">HttpRequest to authenticate.</param>
        /// <param name="store">Token storage object to use.</param>
        /// <returns>If valid, returns the matching ServerAccessToken.  If not valid, returns null.</returns>
        public static ServerAccessToken AuthenticateUser(HttpListenerRequest request, IServerTokenStore store)
        {
            AbstractRequest arequest = new AbstractRequest(request);
            NameValueCollection nvc = DecodeRequest(arequest);
            if (nvc == null) return null;
            return AuthenticateUser(arequest.Url, arequest.HttpMethod, nvc, store);
        }

        /// <summary>
        /// Authenticate a user given raw request arguments.
        /// </summary>
        /// <param name="uri">The URI of the resource.</param>
        /// <param name="requestMethod">The request method used.</param>
        /// <param name="arguments">The arugments in the request.</param>
        /// <param name="store">The Token Storage Provider.</param>
        /// <returns>If valid, returns the matching ServerAccessToken.  If not valid, returns null.</returns>
        static ServerAccessToken AuthenticateUser(Uri uri, string requestMethod, NameValueCollection arguments, IServerTokenStore store)
        {
            CheckResult result = CheckRequest(ServerRequestMode.AccessToken, uri, requestMethod, arguments, store);
            if (!result.Success) return null;
            return result.AccessToken;
        }

        /// <summary>
        /// Create a new Request Token using the given arguments, and store it using the supplied
        /// token storage.
        /// </summary>
        /// <param name="consumerKey">Key of the consumer that is making the request.</param>
        /// <param name="parameters">Additional parameters that will be stored in the Request Token.</param>
        /// <param name="store">Token storage provider.</param>
        /// <returns>A new ServerRequestToken.</returns>
        public static ServerRequestToken BuildAndStoreRequestToken(string consumerKey, NameValueCollection parameters, IServerTokenStore store)
        {
            ServerRequestToken requestToken = new ServerRequestToken(consumerKey, GenerateRandomValue(), GenerateRandomValue(), null, parameters);
            store.StoreRequestToken(requestToken);
            return requestToken;
        }

        /// <summary>
        /// Authorize a Request Token, thereby generating an associated Access Token.
        /// </summary>
        /// <param name="requestTokenKey">Key of the request token.</param>
        /// <param name="userAccount">The associated user account.</param>
        /// <param name="approvedScope">The optional scope parameters.</param>
        /// <param name="store">Token storage object to use.</param>
        public static void AuthorizeRequestToken(string requestTokenKey, string userAccount, string[] approvedScope, IServerTokenStore store)
        {
            ServerRequestToken requestToken = store.FindRequestToken(requestTokenKey);
            if (requestToken == null)
            {
                throw new TokenNotFoundException(String.Format(CultureInfo.InvariantCulture, "Request Token '{0}' is not present.", requestTokenKey));
            }
            if (!String.IsNullOrEmpty(requestToken.AccessTokenKey))
            {
                throw new TokenAlreadyAuthorizedException(String.Format(CultureInfo.InvariantCulture, "Request Token '{0}' is already authorized.", requestTokenKey));
            }
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add(requestToken.Parameters);
            if (approvedScope != null) { nvc["scope"] = String.Join(" ", approvedScope); }
            else { nvc["scope"] = ""; }

            ServerAccessToken accessToken = new ServerAccessToken(requestToken.ConsumerKey, GenerateRandomValue(), GenerateRandomValue(), userAccount, nvc);
            requestToken.AccessTokenKey = accessToken.Key;
            store.StoreAccessToken(accessToken);
            store.StoreRequestToken(requestToken);
        }

        /// <summary>
        /// Using the provided Request Token, retrieve the associated Access Token,
        /// if one has been authorized.
        /// </summary>
        /// <param name="requestTokenKey">The key of the Request Token.</param>
        /// <param name="store">Token storage provider.</param>
        /// <returns>If the Access Token is valid, returns the token.  If not, returns null.</returns>
        public static ServerAccessToken RetrieveAccessTokenUsingRequestToken(string requestTokenKey, IServerTokenStore store)
        {
            ServerRequestToken requestToken = store.FindRequestToken(requestTokenKey);
            if (requestToken == null || String.IsNullOrEmpty(requestToken.AccessTokenKey))
            {
                return null;
            }
            ServerAccessToken accessToken = store.FindAccessToken(requestToken.AccessTokenKey);
            store.DeleteRequestToken(requestTokenKey);
            return accessToken;
        }

        /// <summary>
        /// Process and respond to a token request using HttpRequest and HttpResponse objects.
        /// </summary>
        /// <param name="request">Request to process.</param>
        /// <param name="response">Response object to respond with.</param>
        /// <param name="mode">Token request mode.</param>
        /// <param name="store">Token storage object.</param>
        public static void HandleTokenRequest(HttpRequest request, HttpResponse response, ServerRequestMode mode, IServerTokenStore store)
        {
            AbstractRequest arequest = new AbstractRequest(request);
            AbstractResponse aresponse = new AbstractResponse(response);
            HandleHttpTokenRequest(arequest, aresponse, mode, store);
        }

        /// <summary>
        /// Process and respond to a token request using HttpListenerRequest and HttpListenerResponse objects.
        /// </summary>
        /// <param name="request">Request to process.</param>
        /// <param name="response">Response object to respond with.</param>
        /// <param name="mode">Token request mode.</param>
        /// <param name="store">Token storage object.</param>
        public static void HandleTokenRequest(HttpListenerRequest request, HttpListenerResponse response, ServerRequestMode mode, IServerTokenStore store)
        {
            AbstractRequest arequest = new AbstractRequest(request);
            AbstractResponse aresponse = new AbstractResponse(response);
            HandleHttpTokenRequest(arequest, aresponse, mode, store);
        }

        /// <summary>
        /// Abstract method to handle token requests.
        /// </summary>
        /// <param name="request">Abstracted HTTP request object.</param>
        /// <param name="response">Abstracted HTTP response object.</param>
        /// <param name="mode">Token request mode.</param>
        /// <param name="store">Token storage provider.</param>
        static void HandleHttpTokenRequest(AbstractRequest request, AbstractResponse response, ServerRequestMode mode, IServerTokenStore store)
        {
            StreamWriter sw = new StreamWriter(response.OutputStream);
            NameValueCollection nvc = null;
            nvc = DecodeRequest(request);
            if (nvc == null) 
            {
                response.StatusCode = 400;
                sw.Write("Unsupported HTTP request method");
                sw.Dispose();
                return;
            }

            TokenProcessingResult result = HandleTokenRequest(mode, request.Url, request.HttpMethod, nvc, store);
            if (!result.Success)
            {
                switch (result.FailGenericCondition)
                {
                    case FailureGenericType.BadRequest:
                        response.StatusCode = 400;
                        break;
                    case FailureGenericType.Unauthorized:
                        response.StatusCode = 401;
                        break;
                }
                sw.Write(result.FailReason);
                sw.Dispose();
                return;
            }

            sw.Write(OAuthUtility.ArgsToVal(result.ResponseArguments, AuthenticationMethod.Post));
            sw.Dispose();
        }

        /// <summary>
        /// Handle a token request using raw request arguments.
        /// </summary>
        /// <param name="mode">The request OAuth mode for processing token requests.</param>
        /// <param name="uri">The URI of the resource.</param>
        /// <param name="requestMethod">The request method used.</param>
        /// <param name="arguments">The arguments in the request.</param>
        /// <param name="store">The Token Storage Provider.</param>
        /// <returns>A processing result object that contains all the important result data.</returns>
        static TokenProcessingResult HandleTokenRequest(ServerRequestMode mode, Uri uri, string requestMethod, NameValueCollection arguments, IServerTokenStore store)
        {
            NameValueCollection nvc = new NameValueCollection(arguments);
            nvc.Remove(Strings.Realm);

            CheckResult result = CheckRequest(ServerRequestMode.RequestToken, uri, requestMethod, nvc, store);
            if (!result.Success)
            {
                return new TokenProcessingResult(false, result.FailReason, result.FailGenericCondition, result.FailSpecificCondition);
            }

            OAuthToken token = null;

            switch (mode)
            {
                case ServerRequestMode.RequestToken:
                    token = BuildAndStoreRequestToken(result.Consumer.ConsumerKey, null, store);
                    break;
                case ServerRequestMode.AccessToken:
                    token = RetrieveAccessTokenUsingRequestToken(nvc[OAuthArguments.OAuthToken], store);
                    if (token == null) return new TokenProcessingResult(false, FailureTypeToString(FailureSpecificType.InvalidToken), FailureGenericType.Unauthorized, FailureSpecificType.InvalidToken);
                    break;
            }

            if (token != null)
            {
                NameValueCollection outcollection = new NameValueCollection();
                outcollection.Add(OAuthArguments.OAuthToken, token.Key);
                outcollection.Add(OAuthArguments.OAuthTokenSecret, token.Secret);
                TokenProcessingResult tresult = new TokenProcessingResult(true, null, FailureGenericType.None, FailureSpecificType.None);
                tresult.ResponseArguments.Add(outcollection);
                return tresult;
            }
            return new TokenProcessingResult(false, "Bad request", FailureGenericType.BadRequest, FailureSpecificType.None);
        }

        /// <summary>
        /// Given an abstracted HTTP request object, extract all OAuth parameters and return the arguments.
        /// </summary>
        /// <param name="request">The abstracted HTTP request object.</param>
        /// <returns>A populated collection of OAuth parameters in the request.</returns>
        static NameValueCollection DecodeRequest(AbstractRequest request)
        {
            NameValueCollection nvc = new NameValueCollection();
            if (request.Headers["Authorization"] != null
                && request.Headers["Authorization"].StartsWith("OAuth", StringComparison.Ordinal))
            {
                string arguments = request.Headers["Authorization"];
                string a = arguments.Substring(arguments.IndexOf("OAuth ", StringComparison.Ordinal) + 5);
                string[] args = a.Split(',');
                foreach (string s in args)
                {
                    string[] keyval = s.Trim().Split('=');
                    if (keyval.Length == 2)
                    {
                        nvc[keyval[0]] = HttpUtility.UrlDecode(keyval[1]).Trim('\"');
                    }
                }
            }
            nvc.Add(request.QueryString);
            nvc.Add(request.Form);
            if (nvc.Keys.Count == 0) return null;
            return nvc;
        }

        /// <summary>
        /// Perform complete validation of the OAuth request and return a comprehensive result.
        /// </summary>
        /// <param name="requestMode">Desired token request mode.</param>
        /// <param name="uri">URI of the resource.</param>
        /// <param name="httpMethod">Request method.</param>
        /// <param name="arguments">Arguments in the request.</param>
        /// <param name="store">The Token Storage Provider.</param>
        /// <returns>A populated CheckResult object.</returns>
        static CheckResult CheckRequest(ServerRequestMode requestMode, Uri uri, string httpMethod, NameValueCollection arguments, IServerTokenStore store)
        {
            if (!IsSupportedVersion(arguments))
            {
                return new CheckResult(false, FailureTypeToString(FailureSpecificType.UnsupportedVersion), FailureGenericType.BadRequest, FailureSpecificType.UnsupportedVersion);
            }
            if (!HasRequiredArguments(arguments))
            {
                return new CheckResult(false, FailureTypeToString(FailureSpecificType.MissingParameter), FailureGenericType.BadRequest, FailureSpecificType.MissingParameter);
            }
            if (!HasOnlySingleArguments(arguments))
            {
                return new CheckResult(false, FailureTypeToString(FailureSpecificType.DuplicatedParameter), FailureGenericType.BadRequest, FailureSpecificType.DuplicatedParameter);
            }

            if (!IsValidTimestamp(arguments[OAuthArguments.OAuthTimestamp]))
            {
                return new CheckResult(false, FailureTypeToString(FailureSpecificType.InvalidTimestamp) + " - " + OAuthUtility.Timestamp().ToString(CultureInfo.InvariantCulture), FailureGenericType.BadRequest, FailureSpecificType.InvalidTimestamp); 
            }

            if (requestMode == ServerRequestMode.AccessToken)
            {
                if (!IsValidNonce(arguments[OAuthArguments.OAuthConsumerKey], arguments[OAuthArguments.OAuthNonce]))
                {
                    return new CheckResult(false, FailureTypeToString(FailureSpecificType.InvalidNonce), FailureGenericType.Unauthorized, FailureSpecificType.InvalidNonce);
                }
            }
            
            ConsumerRegistration cr = store.FindConsumerRegistration(arguments[OAuthArguments.OAuthConsumerKey]);
            if (cr == null)
            {
                return new CheckResult(false, FailureTypeToString(FailureSpecificType.InvalidConsumerKey), FailureGenericType.Unauthorized, FailureSpecificType.InvalidConsumerKey);
            }

            string tokenSecret = null;
            ServerAccessToken accessToken = null;
            switch (requestMode) 
            {
                case ServerRequestMode.RequestToken:
                    ServerRequestToken rt = store.FindRequestToken(arguments[OAuthArguments.OAuthToken]);
                    if (rt != null) tokenSecret = rt.Secret;
                    break;
                case ServerRequestMode.AccessToken:
                    accessToken = store.FindAccessToken(arguments[OAuthArguments.OAuthToken]);
                    if (accessToken != null) tokenSecret = accessToken.Secret;
                    break;
            }

            try
            {
                if (!IsValidSignature(uri, httpMethod, arguments, cr.ConsumerSecret, tokenSecret, cr.RsaCertificate))
                {
                    string failReason = FailureTypeToString(FailureSpecificType.InvalidSignature) + " - BaseString: " + OAuthUtility.GenerateBaseString(uri, arguments, httpMethod);
                    return new CheckResult(false, failReason, FailureGenericType.Unauthorized, FailureSpecificType.InvalidSignature);
                }
            }
            catch (NotSupportedException)
            {
                string failReason = FailureTypeToString(FailureSpecificType.UnsupportedSignatureMethod);
                return new CheckResult(false, failReason, FailureGenericType.BadRequest, FailureSpecificType.UnsupportedSignatureMethod);
            }

            CheckResult result = new CheckResult(true, null, FailureGenericType.None, FailureSpecificType.None);
            result.Consumer = cr;
            result.AccessToken = accessToken;

            return result;
        }

        // Individual Validity Checks

        /// <summary>
        /// Checks to ensure the request is for a supported version.
        /// </summary>
        /// <param name="arguments">Request arguments.</param>
        /// <returns>True if supported, false if not.</returns>
        static bool IsSupportedVersion(NameValueCollection arguments)
        {
            string ver = arguments[OAuthArguments.OAuthVersion];
            if (ver == null) return true;
            if (ver != "1.0") return false;
            return true;
        }
        /// <summary>
        /// Checks to ensure all the required arguments for OAuth are
        /// present.
        /// </summary>
        /// <param name="arguments">Request arguments.</param>
        /// <returns>True if all are present, false if not.</returns>
        static bool HasRequiredArguments(NameValueCollection arguments)
        {
            string[] requiredArguments = { OAuthArguments.OAuthConsumerKey,
                                             OAuthArguments.OAuthNonce,
                                             OAuthArguments.OAuthSignature,
                                             OAuthArguments.OAuthSignatureMethod,
                                             OAuthArguments.OAuthTimestamp };

            foreach (string arg in requiredArguments)
            {
                string val = arguments[arg];
                if (String.IsNullOrEmpty(val)) return false;
            }
            return true;
        }
        /// <summary>
        /// Checks to ensure all OAuth parameters only contain single arguments.
        /// </summary>
        /// <param name="arguments">Request arguments.</param>
        /// <returns>True if valid, false if invalid.</returns>
        static bool HasOnlySingleArguments(NameValueCollection arguments)
        {
            string[] oauthArguments = { OAuthArguments.OAuthConsumerKey,
                                        OAuthArguments.OAuthNonce,
                                        OAuthArguments.OAuthSignature,
                                        OAuthArguments.OAuthSignatureMethod,
                                        OAuthArguments.OAuthTimestamp,
                                        OAuthArguments.OAuthVersion,
                                        OAuthArguments.OAuthToken
                                      };

            foreach (string arg in oauthArguments)
            {
                string val = arguments[arg];
                if (!String.IsNullOrEmpty(val))
                {
                    string[] items = val.Split(',');
                    if (items.Length > 1) return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Confirms the validity of the OAuth request signature.
        /// </summary>
        /// <param name="uri">URI of the resource.</param>
        /// <param name="httpMethod">Request method.</param>
        /// <param name="arguments">Request arguments.</param>
        /// <param name="consumerSecret">Consumer secret.</param>
        /// <param name="tokenSecret">Token secret.</param>
        /// <param name="rsaCert">For RSA-SHA1 signing, the X509 certificate containing the public key used to verify the signature.</param>
        /// <returns>True if the signature is valid, false if not.</returns>
        static bool IsValidSignature(Uri uri, string httpMethod, NameValueCollection arguments, string consumerSecret, string tokenSecret, X509Certificate2 rsaCert)
        {
            NameValueCollection args = new NameValueCollection(arguments);
            string receivedsig = args[OAuthArguments.OAuthSignature];
            args.Remove(OAuthArguments.OAuthSignature);
            string sigMethod = args[OAuthArguments.OAuthSignatureMethod];
            args.Remove(Strings.Realm);

            string basestring = OAuthUtility.GenerateBaseString(uri, args, httpMethod);
            if (OAuthUtility.StringToSigMethod(sigMethod) == SignatureMethod.RsaSha1)
            {
                RSAPKCS1SignatureDeformatter verifier = new RSAPKCS1SignatureDeformatter();
                verifier.SetKey(rsaCert.PublicKey.Key);
                verifier.SetHashAlgorithm("SHA1");
                byte[] input = Encoding.UTF8.GetBytes(basestring);
                byte[] hash = SHA1.Create().ComputeHash(input);
                byte[] sig = Convert.FromBase64String(receivedsig);
                return verifier.VerifySignature(hash, sig);
            }
            else
            {
                string calcsig = OAuthUtility.GenerateSignature(basestring, consumerSecret, tokenSecret, null, OAuthUtility.StringToSigMethod(sigMethod));
                if (receivedsig == calcsig) { return true; }
            }
            return false;
        }
        /// <summary>
        /// Checks to ensure that the received timestamp is plus or minus 30 seconds from
        /// the current server's time.
        /// </summary>
        /// <param name="timestamp">Received timestamp.</param>
        /// <returns>True if valid, false if not.</returns>
        static bool IsValidTimestamp(string timestamp)
        {
            long now = OAuthUtility.Timestamp();
            long received = 0;
            if (!long.TryParse(timestamp, out received)) return false;
            long delta = now - received;
            if (delta > 30 || delta < -30) return false;
            return true;
        }
        /// <summary>
        /// Checks to ensure that the received nonce has not been received before.
        /// </summary>
        /// <param name="consumerKey">Consumer key.</param>
        /// <param name="nonce">Received nonce.</param>
        /// <returns>True if check has passed, false if not.</returns>
        static bool IsValidNonce(string consumerKey, string nonce)
        {
            string strNonceCache = "NonceCache";
            Dictionary<string, List<string>> NonceCache = (Dictionary<string, List<string>>)HttpRuntime.Cache.Get(strNonceCache);
            if (NonceCache == null) NonceCache = new Dictionary<string, List<string>>();
            if (!NonceCache.ContainsKey(consumerKey)) NonceCache.Add(consumerKey, new List<string>());
            if (NonceCache[consumerKey].Contains(nonce)) return false;
            NonceCache[consumerKey].Add(nonce);
            if (NonceCache[consumerKey].Count > 20)
            {
                string toRemove = NonceCache[consumerKey][0];
                NonceCache.Remove(toRemove);
            }
            HttpRuntime.Cache.Insert(strNonceCache, NonceCache);
            return true;
        }
        /// <summary>
        /// Translate a FailureSpecificType to a description string.
        /// </summary>
        /// <param name="ftype">Input value.</param>
        /// <returns>A descriptive string.</returns>
        internal static string FailureTypeToString(FailureSpecificType ftype)
        {
            switch (ftype)
            {
                case FailureSpecificType.DuplicatedParameter:
                    return "Duplicated OAuth Protocol Parameter";
                case FailureSpecificType.InvalidConsumerKey:
                    return "Invalid Consumer Key";
                case FailureSpecificType.InvalidNonce:
                    return "Invalid / used nonce";
                case FailureSpecificType.InvalidSignature:
                    return "Invalid signature";
                case FailureSpecificType.InvalidTimestamp:
                    return "Timestamp is not close enough to current time";
                case FailureSpecificType.InvalidToken:
                    return "Invalid / expired Token";
                case FailureSpecificType.MissingParameter:
                    return "Missing required parameter";
                case FailureSpecificType.None:
                    return null;
                case FailureSpecificType.TokenRequired:
                    return "Token Required";
                case FailureSpecificType.UnsupportedParameter:
                    return "Unsupported parameter";
                case FailureSpecificType.UnsupportedSignatureMethod:
                    return "Unsupported signature method";
                case FailureSpecificType.UnsupportedVersion:
                    return "Unsupported OAuth version";
            }
            return null;
        }
    }

    /// <summary>
    /// High-level failure categories.
    /// </summary>
    enum FailureGenericType
    {
        None,
        Unauthorized,
        BadRequest
    }

    /// <summary>
    /// Specific failure types.
    /// </summary>
    enum FailureSpecificType
    {
        None,
        DuplicatedParameter,
        InvalidConsumerKey,
        InvalidNonce,
        InvalidSignature,
        InvalidToken,
        InvalidTimestamp,
        MissingParameter,
        TokenRequired,
        UnsupportedParameter,
        UnsupportedSignatureMethod,
        UnsupportedVersion
    }

    /// <summary>
    /// Comprehensive request validation result data.
    /// </summary>
    class CheckResult
    {
        /// <summary>
        /// Whether or not all checks had passed.
        /// </summary>
        public bool Success { get; private set; }
        /// <summary>
        /// The descriptive string speaking to the error condition that occurred, if any.
        /// </summary>
        public string FailReason { get; private set; }
        /// <summary>
        /// The specific failure condition.
        /// </summary>
        public FailureSpecificType FailSpecificCondition { get; private set; }
        /// <summary>
        /// The high-level category of the failure condition.
        /// </summary>
        public FailureGenericType FailGenericCondition { get; private set; }
        /// <summary>
        /// If a valid request, the ConsumerRegistration object that is appropriate for this request.
        /// </summary>
        public ConsumerRegistration Consumer { get; set; }
        /// <summary>
        /// If a valid request, the AccessToken object that is appropriate for this request.
        /// </summary>
        public ServerAccessToken AccessToken { get; set; }

        /// <summary>
        /// Create a new CheckResult.
        /// </summary>
        /// <param name="success">Whether or not all checks have passed.</param>
        /// <param name="failReason">The descriptive reason for the failure.</param>
        /// <param name="gFailType">Generic failure condition.</param>
        /// <param name="sFailType">Specific failure condition.</param>
        public CheckResult(bool success, string failReason, FailureGenericType gFailType, FailureSpecificType sFailType)
        {
            Success = success;
            FailReason = failReason;
            FailGenericCondition = gFailType;
            FailSpecificCondition = sFailType;
        }
    }

    /// <summary>
    /// High-level result from a token processing operation.
    /// </summary>
    class TokenProcessingResult
    {
        /// <summary>
        /// Whether or not the operation was successful.
        /// </summary>
        public bool Success { get; private set; }
        /// <summary>
        /// The descriptive reason for the failure, if any.
        /// </summary>
        public string FailReason { get; private set; }
        /// <summary>
        /// The high-level failure category.
        /// </summary>
        public FailureGenericType FailGenericCondition { get; private set; }
        /// <summary>
        /// The specific failure condition.
        /// </summary>
        public FailureSpecificType FailSpecificCondition { get; private set; }
        /// <summary>
        /// For successful operations, the response arguments that should be passed back to the requestor.
        /// </summary>
        public NameValueCollection ResponseArguments { get; private set; }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="success">Whether or not the operation was successful.</param>
        /// <param name="failReason">The descriptive reason for the failure.</param>
        /// <param name="gFailType">The generic failure condition.</param>
        /// <param name="sFailType">The specific failue condition.</param>
        public TokenProcessingResult(bool success, string failReason, FailureGenericType gFailType, FailureSpecificType sFailType)
        {
            Success = success;
            FailReason = failReason;
            FailGenericCondition = gFailType;
            FailSpecificCondition = sFailType;
            ResponseArguments = new NameValueCollection();
        }
    }
}
