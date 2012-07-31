using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ExtremeSwank.OpenId.Persistence;
using ExtremeSwank.OpenId.PlugIns.Discovery;
using Mono.Security.Cryptography;

namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Common functions used by main classes and plugins.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Ensures that the byte array converts to a positive value. 
        /// </summary>
        /// <param name="inputBytes">Unsigned byte-array.</param>
        /// <returns>A corrected byte-array.</returns>
        internal static byte[] EnsurePositive(byte[] inputBytes)
        {
            if (inputBytes == null) { throw new ArgumentNullException("inputBytes"); }
            if (inputBytes.Length < 1) { throw new ArgumentOutOfRangeException("inputBytes"); }

            int i = Convert.ToInt32(inputBytes[0].ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
            if (i > 127)
            {
                byte[] temp = new byte[inputBytes.Length + 1];
                temp[0] = 0;
                inputBytes.CopyTo(temp, 1);
                inputBytes = temp;
            }
            return inputBytes;
        }
        /// <summary>
        /// Positively-ensures an input byte-array and converts to a Base64 string.
        /// </summary>
        /// <param name="inputBytes">Unsigned byte-array.</param>
        /// <returns>A Base64 string representing the byte-array.</returns>
        internal static string UnsignedToBase64(byte[] inputBytes)
        {
            return Convert.ToBase64String(EnsurePositive(inputBytes));
        }

        /// <summary>
        /// Converts HTTP response to key-value pairs.
        /// </summary>
        /// <param name="response">HTTP response.</param>
        /// <returns>Dictionary&lt;string, string&gt; object representing information in response.</returns>
        internal static NameValueCollection SplitResponse(string response)
        {
            NameValueCollection sd = new NameValueCollection();
            if (String.IsNullOrEmpty(response)) { return sd; }

            string[] rarr = response.Split('\n');

            foreach (string l in rarr)
            {
                string line = l.Trim();
                if (!String.IsNullOrEmpty(line))
                {
                    char[] delimiter = { ':' };
                    string[] keyval = line.Split(delimiter, 2);
                    if (keyval.Length == 2)
                    {
                        sd[keyval[0].Trim()] = keyval[1].Trim();
                    }
                }
            }
            return sd;
        }
        /// <summary>
        /// Converts a Dictionary&lt;string, string&gt; to a URL string.
        /// </summary>
        /// <param name="arr">Dictionary&lt;string, string&gt; to convert.</param>
        /// <returns>A URL string.</returns>
        internal static string Keyval2URL(NameValueCollection arr)
        {
            if (arr == null) { throw new ArgumentNullException("arr"); }
            string[] vals = new string[arr.Count];
            int i = 0;
            foreach (string key in arr.Keys)
            {
                vals[i] = key + "=" + HttpUtility.UrlEncode(arr[key]);
                i++;
            }

            string qstr = String.Join("&", vals);
            return qstr;
        }

        /// <summary>
        /// Given a URL and a set of arguments, creates a complete and proper HTTP GET URL.
        /// </summary>
        /// <param name="url">Base URL</param>
        /// <param name="arr">Dictionary&lt;string, string&gt; containing argument keys and values</param>
        /// <returns>An HTTP GET URL that includes all supplied arguments.</returns>
        internal static Uri MakeGetURL(Uri url, NameValueCollection arr)
        {
            if (url == null) { throw new ArgumentNullException("url"); }
            if (arr == null) { throw new ArgumentNullException("arr"); }

            string retval;
            string arguments = Keyval2URL(arr);
            // if (url == null) { return null; }
            if (url.AbsoluteUri.Contains("?"))
            {
                retval = url.AbsoluteUri + "&" + arguments;
            }
            else
            {
                retval = url.AbsoluteUri + "?" + arguments;
            }
            return new Uri(retval);
        }

        /// <summary>
        /// Appends the cnonce variable to the end of a URL.
        /// </summary>
        /// <param name="url">URL to process.</param>
        /// <param name="rp">StateContainer to use.</param>
        /// <returns>A combined URL.</returns>
        internal static string AddExtraVariables(string url, StateContainer rp)
        {
            if (String.IsNullOrEmpty(url)) { return url; }
            if (rp == null) { throw new ArgumentNullException("rp"); }

            string retval = url;
            if (rp.AuthMode == AuthenticationMode.Stateful)
            {
                if (retval.Contains("?"))
                {
                    retval += "&cnonce=" + rp.Nonce;
                }
                else
                {
                    retval += "?cnonce=" + rp.Nonce;
                }
            }

            return retval;
        }

        /// <summary>
        /// Performs an HTTP request and returns the response.
        /// </summary>
        /// <param name="url">URL to make request to.</param>
        /// <param name="method">Request type, either "GET" or "POST".</param>
        /// <param name="pms">Dictionary&lt;string, string&gt; containing key-value pairs to send.</param>
        /// <param name="actualLocation">The real URL for this request, after redirects.</param>
        /// <returns>String containing HTTP response.</returns>
        internal static string MakeRequest(Uri url, string method, NameValueCollection pms, out string actualLocation)
        {
            if (url == null) { throw new ArgumentNullException("url"); }

            Uri uri;

            if (String.IsNullOrEmpty(method)) { method = "GET"; }
            if (method == "GET" && pms != null)
            {
                uri = MakeGetURL(url, pms);
            }
            else
            {
                uri = url;
            }

            Tracer.Write("URI: " + uri);
            HttpWebRequest hwp;
            try
            {
                hwp = (HttpWebRequest)WebRequest.Create(uri);
            }
            catch (WebException we)
            {
                Tracer.Write("Connection failed: " + we.Message);
                actualLocation = null;
                return null;
            }
            hwp.UserAgent = "ExtremeSwank OpenID Consumer " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            hwp.Method = method;
            hwp.Timeout = 10000;
            Stream s;

            // Open connection, write request to server
            if (method == "POST" && pms != null)
            {
                hwp.ContentType = "application/x-www-form-urlencoded";
                string vals = Keyval2URL(pms);
                byte[] data = System.Text.Encoding.UTF8.GetBytes(vals);
                hwp.ContentLength = data.Length;
                s = hwp.GetRequestStream();
                s.Write(data, 0, data.Length);
                s.Close();
            }

            // Get response from server
            WebResponse response = null;
            try
            {
                response = hwp.GetResponse();
            }
            catch (WebException we)
            {
                actualLocation = null;
                HandleHttpError(we.Response, null);
                return null;
            }

            s = response.GetResponseStream();
            actualLocation = response.ResponseUri.AbsoluteUri;
            StreamReader sr = new StreamReader(s);
            string body = sr.ReadToEnd();
            string headers = "";

            foreach (string header in response.Headers)
            {
                headers += header + ":" + response.Headers[header] + "\n";
            }

            // Close connection
            response.Close();

            string ret = headers + body;
            return ret;
        }
        /// <summary>
        /// Performs an HTTP request and returns the response.
        /// </summary>
        /// <param name="url">URL to make request to.</param>
        /// <param name="actualLocation">The final URL found after redirects.</param>
        /// <returns>String containing HTTP response.</returns>
        internal static string MakeRequest(Uri url, out string actualLocation)
        {
            return MakeRequest(url, null, null, out actualLocation);
        }
        /// <summary>
        /// Remove HTML comments from string.
        /// </summary>
        /// <param name="content">String containing HTML.</param>
        /// <returns>String with HTML comments removed.</returns>
        internal static string RemoveHtmlComments(string content)
        {
            if (String.IsNullOrEmpty(content)) { return content; }

            Regex rx_comments = new Regex(@"((<!-- )((?!<!-- ).)*( -->))(\r\n)*", RegexOptions.Singleline);

            string newcontent = rx_comments.Replace(content, string.Empty);
            return newcontent;
        }
        /// <summary>
        /// Processes errors received during HTTP requests.
        /// </summary>
        /// <param name="response">WebResponse object to handle.</param>
        /// <param name="rp">StateContainer object where error state will be recorded.</param>
        internal static void HandleHttpError(WebResponse response, StateContainer rp)
        {
            Stream s;
            StreamReader sr;
            string body = "";
            if (response == null)
            {
                Tracer.Write("Error: Received null response to HTTP request.");
                if (rp != null) rp.ErrorState = ErrorCondition.HttpError;
                return;
            }

            HttpWebResponse hwr = (HttpWebResponse)response;
            switch (hwr.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    s = response.GetResponseStream();
                    sr = new StreamReader(s);
                    body = sr.ReadToEnd();
                    if (body != null)
                    {
                        NameValueCollection data = SplitResponse(body);
                        if (data["error"] != null)
                        {
                            Tracer.Write("Received error: " + data["error"]);
                            if (rp != null) rp.ErrorState = ErrorCondition.RequestRefused;
                            return;
                        }
                    }
                    if (rp != null) rp.ErrorState = ErrorCondition.HttpError;
                    break;
                default:
                    Tracer.Write("HTTP request failed, response code: " + hwr.StatusCode.ToString());
                    break;
            }
        }

        /// <summary>
        /// Return a URL representing the current host
        /// </summary>
        internal static string WebRoot
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    HttpRequest Request = HttpContext.Current.Request;
                    string root = "";
                    if (Request.ServerVariables["SERVER_PORT_SECURE"] == "0")
                    {
                        if (Request.ServerVariables["SERVER_PORT"] != "80")
                        {
                            root = "http://" + Request.ServerVariables["SERVER_NAME"] + ":" + Request.ServerVariables["SERVER_PORT"];
                        }
                        else
                        {
                            root = "http://" + Request.ServerVariables["SERVER_NAME"];
                        }
                    }
                    else
                    {
                        if (Request.ServerVariables["SERVER_PORT"] != "443")
                        {
                            root = "https://" + Request.ServerVariables["SERVER_NAME"] + ":" + Request.ServerVariables["SERVER_PORT"];
                        }
                        else
                        {
                            root = "https://" + Request.ServerVariables["SERVER_NAME"];
                        }
                    }
                    return root;
                }
                return null;
            }
        }

        /// <summary>
        /// Validate a stateful mode response from an OpenID Provider.
        /// </summary>
        /// <param name="server">The OpenID Provider URL.</param>
        /// <param name="rp">The StateContainer object needed to process the validation.</param>
        /// <returns>Whether or not validation was successful.</returns>
        internal static bool ValidateStatefulResponse(Uri server, StateContainer rp) 
        {
            if (server == null) { throw new ArgumentNullException("server"); }
            if (rp == null) { throw new ArgumentNullException("rp"); }

            if (rp.Nonce == -1)
            {
                Tracer.Write("Error: The cnonce is not valid.");
                rp.ErrorState = ErrorCondition.SessionTimeout;
                return false;
            }
            else
            {
                if (!String.IsNullOrEmpty(rp.RequestArguments["cnonce"]) && rp.Nonce != Convert.ToInt32(rp.RequestArguments["cnonce"], CultureInfo.InvariantCulture))
                {
                    Tracer.Write("Error: The cnonce has expired.");
                    rp.ErrorState = ErrorCondition.SessionTimeout;
                    return false;
                }
            }

            // Run validation checks in each plug-in
            foreach (IExtension ext in rp.ExtensionPlugIns)
            {
                if (!ext.Validation()) { return false; }
            }

            Tracer.Write("Looking up association in association table.");
            Association assoc = rp.AssociationManager.FindByHandle(rp.RequestArguments["openid.assoc_handle"]);

            if (assoc == null)
            {
                // Check to see if the handle has been invalidated
                if (rp.RequestArguments["openid.invalidate_handle"] != null)
                {
                    Tracer.Write("Association handle has been invalidated.");
                    return false;
                }
                else
                {
                    Tracer.Write("Association handle was not found in the table.");
                    return false;
                }
            }

            // Ensure association key has not expired
            if (assoc.Expiration < DateTime.UtcNow)
            {
                Tracer.Write("Association has expired, removing from table.");
                rp.AssociationManager.Remove(assoc);
                return false;
            }

            // Check for someone trying to forge a response from another
            // OpenID Provider
            if (server.AbsoluteUri != assoc.Server)
            {
                Tracer.Write("Received response handle is not valid for the specified OpenID Provider.");
                return false;
            }

            // Compare data from browser to association handle from server
            if (assoc.Handle == rp.RequestArguments["openid.assoc_handle"])
            {
                string[] tokens = rp.RequestArguments["openid.signed"].ToString().Split(',');
                string token_contents = "";
                foreach (string token in tokens)
                {
                    token_contents += token + ":" + rp.RequestArguments["openid." + token] + "\n";
                }
                Tracer.Write("Generating signature for tokens: " + token_contents);
                string sig = rp.RequestArguments["openid.sig"].ToString();

                byte[] secretkey = assoc.Secret;
                byte[] tokenbyte = ASCIIEncoding.ASCII.GetBytes(token_contents);
                HashAlgorithm hmac = null;

                if (assoc.AssociationType == "HMAC-SHA1")
                {
                    hmac = new HMACSHA1(secretkey);
                }
                else if (assoc.AssociationType == "HMAC-SHA256")
                {
                    hmac = new HMACSHA256(secretkey);
                }
                byte[] realHash = hmac.ComputeHash(tokenbyte);
                string strrealHash = Convert.ToBase64String(realHash);

                Tracer.Write("Expected signature: " + sig);
                Tracer.Write("Generated signature: " + strrealHash);

                if (sig != strrealHash) 
                { 
                    Tracer.Write("Received signature does not match generated signature");
                    return false;
                }
                return true;
            }
            Tracer.Write("Received association handle does not match cached handle.");
            return false;
        }

        /// <summary>
        /// Validate a stateless mode response from an OpenID Provider.
        /// </summary>
        /// <param name="server">The OpenID Provider</param>
        /// <param name="fallback">Whether or not this is a fallback check due to a failed stateful validation attempt.</param>
        /// <param name="rp">The StateContainer object containing the arguments to process.</param>
        /// <returns>True if the validation is successful, false if not.</returns>
        internal static bool ValidateStatelessResponse(Uri server, bool fallback, StateContainer rp)
        {
            if (server == null) { throw new ArgumentNullException("server"); }
            if (rp == null) { throw new ArgumentNullException("rp"); }

            NameValueCollection pms = new NameValueCollection();
            if (fallback)
            {
                if (!String.IsNullOrEmpty(rp.RequestArguments["openid.invalidate_handle"]))
                {
                    pms["openid.invalidate_handle"] = rp.RequestArguments["openid.invalidate_handle"];
                }
            }

            // Run validation checks in each plug-in
            foreach (IExtension ext in rp.ExtensionPlugIns)
            {
                if (!ext.Validation()) { return false; }
            }

            // Send only required parameters to confirm validity
            string[] arr_signed = rp.RequestArguments["openid.signed"].Split(',');
            for (int i = 0; i < arr_signed.Length; i++)
            {
                string s = arr_signed[i];
                string c = rp.RequestArguments["openid." + arr_signed[i]];
                pms["openid." + s] = c;
            }
            if (server == null)
            {
                Tracer.Write("No OpenID servers found");
                rp.ErrorState = ErrorCondition.NoServersFound;
                return false;
            }

            pms["openid.mode"] = "check_authentication";
            pms["openid.assoc_handle"] = rp.RequestArguments["openid.assoc_handle"];
            pms["openid.signed"] = rp.RequestArguments["openid.signed"];
            pms["openid.sig"] = rp.RequestArguments["openid.sig"];

            string actualLocation = null;
            // Connect to IdP
            string response = Utility.MakeRequest(server, "POST", pms, out actualLocation);
    
            if (String.IsNullOrEmpty(response))
            {
                Tracer.Write("No response from Identity Provider using POST, trying GET.");
                response = Utility.MakeRequest(server, "GET", pms, out actualLocation);
                if (String.IsNullOrEmpty(response))
                {
                    return false;
                }
            }

            // Parse reponse
            NameValueCollection data = Utility.SplitResponse(response);

            // Check for validity of authentication request
            if (data["is_valid"] == "true")
            {
                Tracer.Write("Server has validated authentication response.");
                return true;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (string key in data.Keys)
                {
                    sb.AppendLine(key + ":" + data[key]);
                }
                Tracer.Write("Server has not validated authentication response.  Response received: " + sb.ToString());
                rp.ErrorState = ErrorCondition.RequestRefused;
                return false;
            }
        }
        /// <summary>
        /// Converts a supplied OpenID into two distinct entities - a normalized name and a URI
        /// </summary>
        /// <param name="openid">OpenID to normalize</param>
        /// <param name="plugins">IDiscovery plugins to use to process the OpenID</param>
        /// <returns>A populated NormalizationEntry object.</returns>
        internal static NormalizationEntry Normalize(string openid, IEnumerable<IDiscovery> plugins)
        {
            if (String.IsNullOrEmpty(openid)) { throw new ArgumentNullException("openid"); }
            if (plugins == null) { throw new ArgumentNullException("plugins"); }

            NormalizationEntry result = new NormalizationEntry();

            // Loop through the registered Discovery Plugins
            // and attempt to get a processed ID.
            foreach (IDiscovery disc in plugins)
            {
                result = disc.ProcessId(openid);
                if (result != null) { break; }
            }

            return result;
        }

        /// <summary>
        /// Returns the URL to which the User Agent should be redirected for the initial authentication request.
        /// </summary>
        /// <param name="props">RequestProperties object holding the current state.</param>
        /// <param name="discResult">The DiscoveryResult object created from the previous discovery process.</param>
        /// <param name="immediate">Whether or not an Immediate Mode request is being generated.</param>
        /// <returns>The complete URL to which the User Agent should be redirected.</returns>
        internal static Uri GetRedirectURL(StateContainer props, DiscoveryResult discResult, bool immediate)
        {
            if (props == null) { throw new ArgumentNullException("props"); }
            if (discResult == null) { throw new ArgumentNullException("discResult"); }

            NameValueCollection pms = new NameValueCollection();

            if (immediate)
            {
                pms["openid.mode"] = "checkid_immediate";
            }
            else
            {
                pms["openid.mode"] = "checkid_setup";
            }

            string returl = props.ReturnToUrl.AbsoluteUri;

            switch (props.AuthMode)
            {
                case AuthenticationMode.Stateful:
                    Association assoc = props.AssociationManager.FindByServer(discResult.ServerUrl.AbsoluteUri);

                    Random r = new Random();
                    props.Nonce = r.Next();

                    switch (discResult.AuthVersion)
                    {
                        case ProtocolVersion.V1Dot1:
                            pms["openid.assoc_handle"] = assoc.Handle;
                            pms["openid.trust_root"] = props.TrustRoot;

                            break;
                        case ProtocolVersion.V2Dot0:
                            pms["openid.ns"] = ProtocolUri.OpenId2Dot0.AbsoluteUri;
                            pms["openid.assoc_handle"] = assoc.Handle;
                            pms["openid.realm"] = props.TrustRoot;
                            break;
                    }
                    break;
                case AuthenticationMode.Stateless:
                    switch (discResult.AuthVersion)
                    {
                        case ProtocolVersion.V1Dot1:
                            pms["openid.trust_root"] = props.TrustRoot;
                            break;
                        case ProtocolVersion.V2Dot0:
                            pms["openid.ns"] = ProtocolUri.OpenId2Dot0.AbsoluteUri;
                            pms["openid.realm"] = props.TrustRoot;
                            break;
                    }
                    break;
            }

            foreach (IExtension e in props.ExtensionPlugIns)
            {
                NameValueCollection authdata = e.BuildAuthorizationData(discResult);
                foreach (string key in authdata.Keys)
                {
                    if (key == "esoid.ReturnUrl")
                    {
                        if (returl.Contains("?"))
                        {
                            returl += "&" + authdata[key];
                        }
                        else
                        {
                            returl += "?" + authdata[key];
                        }
                    }
                    else
                    {
                        pms[key] = authdata[key];
                    }
                }
            }

            pms["openid.return_to"] = Utility.AddExtraVariables(returl, props);
            return Utility.MakeGetURL(discResult.ServerUrl, pms);
        }

        /// <summary>
        /// Retrieve the URL of the OpenID Provider using configured discovery plugins
        /// </summary>
        /// <returns>The URL of the OpenID Provider</returns>
        internal static DiscoveryResult GetProviderUrl(string identity, IList<IDiscovery> discoveryPlugIns)
        {
            if (String.IsNullOrEmpty(identity)) { throw new ArgumentNullException("identity"); }
            if (discoveryPlugIns == null) { throw new ArgumentNullException("discoveryPlugIns"); }

            DiscoveryResult dr = new DiscoveryResult();

            Tracer.Warn("Creating HTTP Request.");

            DiscoveryResult[] systems = null;
            string actualLocation = null;

            NormalizationEntry ne = Utility.Normalize(identity, discoveryPlugIns);
            if (ne == null) { return null; }

            string response = Utility.MakeRequest(ne.DiscoveryUrl, out actualLocation);

            if (String.IsNullOrEmpty(response)) { return null; }

            dr.ClaimedId = Utility.Normalize(actualLocation, discoveryPlugIns).NormalizedId;

            Tracer.Write("HTTP Request successful.  Passing to discovery plugins.");

            for (int i = 0; i < discoveryPlugIns.Count; i++)
            {
                IDiscovery disc = (IDiscovery)discoveryPlugIns[i];
                Tracer.Write("Trying plugin " + disc.Name);
                systems = disc.Discover(response);
                if (systems != null)
                {
                    Tracer.Write("Plugin discovered endpoint.");
                    dr.AuthVersion = disc.Version;
                    break;
                }
            }

            if (systems == null)
            {
                Tracer.Write("Plugins did not discover endpoint.");
                return null;
            }

            if (systems.Length == 0)
            {
                Tracer.Write("No servers found.");
                return null;
            }

            DiscoveryResult entry = systems[0];

            if (entry.LocalId != null)
            {
                Tracer.Write("Delegated OpenID.");
                dr.LocalId = entry.LocalId;
            }
            dr.ServerUrl = entry.ServerUrl;
            Tracer.Write("Discovery successful - " + dr.ServerUrl.AbsoluteUri);

            if (dr.LocalId == null)
            {
                dr.LocalId = actualLocation;
            }
            return dr;
        }

        /// <summary>
        /// Parses the arguments and returns the requested OpenID mode.
        /// </summary>
        /// <param name="arguments">Arguments to parse.</param>
        /// <returns>The mode requested in the arguments.</returns>
        internal static RequestedMode GetRequestedMode(NameValueCollection arguments)
        {
            if (arguments == null) { throw new ArgumentNullException("arguments"); }

            switch (arguments["openid.mode"])
            {
                case "id_res":
                    Tracer.Write("Resolution response received from OpenID Provider.");
                    if (!String.IsNullOrEmpty(arguments["openid.user_setup_url"])) { return RequestedMode.SetupNeeded; }
                    return RequestedMode.IdResolution;
                case "cancel":
                    Tracer.Write("Cancel response received from OpenID Provider.");
                    return RequestedMode.CanceledByUser;
                case "setup_needed":
                    Tracer.Write("SetupNeeded response received from OpenID Provider.");
                    return RequestedMode.SetupNeeded;
                case "error":
                    Tracer.Write("Received error from OpenID Provider: " + arguments["openid.error"]);
                    return RequestedMode.Error;
            }

            return RequestedMode.None;
        }

        /// <summary>
        /// Matches up Extension namespace URIs to their aliases.
        /// </summary>
        /// <param name="arguments">Arguments received during the current request.</param>
        /// <returns>A dictionary with the namespace URI as the key, and the variable name as the value.</returns>
        public static NameValueCollection GetExtNamespaceAliases(NameValueCollection arguments)
        {
            NameValueCollection ret = new NameValueCollection();
            if (arguments == null) { return ret; }

            foreach (string k in arguments.Keys)
            {
                if (k != null && k.StartsWith("openid.ns.", StringComparison.OrdinalIgnoreCase))
                {
                    ret.Add(arguments[k], k.Substring(10));       
                }
            }
            return ret;
        }

        /// <summary>
        /// Negotiate a new association with the OpenID Provider and add it to persistence.
        /// </summary>
        /// <param name="server">OpenID Provider URL.</param>
        /// <param name="associationManager">The IAssociationPersistence object to use for persistence.</param>
        /// <param name="version">The OpenID Version supported by the OpenID Provider discovery process.</param>
        /// <returns>True if the association was created successfully, false if not.</returns>
        internal static bool BuildAssociation(Uri server, IAssociationPersistence associationManager, ProtocolVersion version)
        {
            if (server == null) { throw new ArgumentNullException("server"); }
            if (associationManager == null) { throw new ArgumentNullException("associationManager"); }
            if (version == ProtocolVersion.Invalid) { throw new ArgumentException("Argument 'version' must not be set to 'ProtocolVersion.Invalid'"); }

            // Look for pre-existing valid association
            Tracer.Write("Looking up OpenID Provider in Associations table");
            Association assoc = associationManager.FindByServer(server.AbsoluteUri);
            if (assoc != null)
            {
                if (assoc.Expiration > DateTime.UtcNow)
                {
                    Tracer.Write("Valid association found.");
                    return true;
                }
            }

            // No valid pre-existing association. Create a new association.
            Tracer.Write("No valid association found.");

            Association association = Utility.CreateAssociation(server, associationManager, version, KeyEncryption.DHSHA256);
            if (association != null)
            {
                associationManager.Add(association);
                Tracer.Write("Successfully added association to table.");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Perform an association request with an OpenID Provider.
        /// </summary>
        /// <param name="server">URL to the OpenID Provider.</param>
        /// <param name="associationManager">The IAssociationPersistence object to use for persistence.</param>
        /// <param name="version">The ProtocolVersion to use.</param>
        /// <param name="encryption">The key encryption type to use.</param>
        /// <returns>Populated Association object, or null.</returns>
        internal static Association CreateAssociation(Uri server, IAssociationPersistence associationManager,
                                                      ProtocolVersion version, KeyEncryption encryption)
        {
            if (server == null) { throw new ArgumentNullException("server"); }
            if (associationManager == null) { throw new ArgumentNullException("associationManager"); }

            NameValueCollection sd = new NameValueCollection();
            DiffieHellmanManaged dhm = new DiffieHellmanManaged();
            sd["openid.mode"] = "associate";

            switch (version)
            {
                case ProtocolVersion.V2Dot0:
                    sd["openid.ns"] = ProtocolUri.OpenId2Dot0.AbsoluteUri;
                    break;
                case ProtocolVersion.V1Dot1:
                    if (encryption == KeyEncryption.DHSHA256)
                    {
                        encryption = KeyEncryption.DHSHA1;
                    }
                    break;
            }

            byte[] pubkey = null;
            DHParameters dp;

            switch (encryption)
            {
                case KeyEncryption.None:
                    sd["openid.assoc_type"] = "HMAC-SHA1";
                    switch (version)
                    {
                        case ProtocolVersion.V2Dot0:
                            sd["openid.session_type"] = "no-encryption";
                            break;
                        case ProtocolVersion.V1Dot1:
                            sd["openid.session_type"] = "";
                            break;
                    }
                    break;
                case KeyEncryption.DHSHA1:
                    pubkey = dhm.CreateKeyExchange();
                    dp = dhm.ExportParameters(true);

                    sd["openid.assoc_type"] = "HMAC-SHA1";
                    sd["openid.session_type"] = "DH-SHA1";
                    sd["openid.dh_modulus"] = Utility.UnsignedToBase64(dp.P);
                    sd["openid.dh_gen"] = Utility.UnsignedToBase64(dp.G);
                    sd["openid.dh_consumer_public"] = Utility.UnsignedToBase64(pubkey);
                    break;
                case KeyEncryption.DHSHA256:
                    pubkey = dhm.CreateKeyExchange();
                    dp = dhm.ExportParameters(true);

                    sd["openid.assoc_type"] = "HMAC-SHA256";
                    sd["openid.session_type"] = "DH-SHA256";
                    sd["openid.dh_modulus"] = Utility.UnsignedToBase64(dp.P);
                    sd["openid.dh_gen"] = Utility.UnsignedToBase64(dp.G);
                    sd["openid.dh_consumer_public"] = Utility.UnsignedToBase64(pubkey);
                    break;
            }

            Tracer.Write("Opening connection to OpenID Provider.");
            string response = "";
            string actualLocation = null;
            response = Utility.MakeRequest(server, "POST", sd, out actualLocation);

            NameValueCollection association = null;
            Association retassoc = null;
            if (response != null)
            {
                Tracer.Write("Association response received.");
                association = Utility.SplitResponse(response);
            }
            else
            {
                Tracer.Write("No association response received.");
                switch (encryption)
                {
                    case KeyEncryption.DHSHA256:
                        Tracer.Write("Falling back to DHSHA1");
                        encryption = KeyEncryption.DHSHA1;
                        retassoc = CreateAssociation(server, associationManager, version, encryption);
                        if (retassoc != null) { return retassoc; }
                        break;
                    case KeyEncryption.DHSHA1:
                        Tracer.Write("Falling back to No-encryption");
                        encryption = KeyEncryption.None;
                        retassoc = CreateAssociation(server, associationManager, version, encryption);
                        if (retassoc != null) { return retassoc; }
                        break;
                }
                return null;
            }

            if (association["error"] != null)
            {
                Tracer.Write("Association response contains error. - " + association["error"]);
                return null;
            }

            if (encryption == KeyEncryption.DHSHA1 || encryption == KeyEncryption.DHSHA256)
            {
                Tracer.Write("Expecting DHSHA1 or DHSHA256.");
                StringBuilder vals = new StringBuilder();
                foreach (string key in association.Keys)
                {
                    vals.AppendLine(key + ": " + association[key]);
                }
                if (association["enc_mac_key"] == null) { Tracer.Write("No encoded MAC key returned! Received " + vals.ToString()); }
                if (association["enc_mac_key"] != null)
                {
                    Tracer.Write("Encrypted association key is present.");
                    byte[] serverpublickey = Convert.FromBase64String(association["dh_server_public"]);
                    byte[] mackey = Convert.FromBase64String(association["enc_mac_key"]);

                    byte[] dhShared = dhm.DecryptKeyExchange(serverpublickey);
                    byte[] shaShared = new byte[0];

                    if (encryption == KeyEncryption.DHSHA1)
                    {
                        Tracer.Write("Decoding DHSHA1 Association.");
                        SHA1 sha1 = new SHA1CryptoServiceProvider();
                        shaShared = sha1.ComputeHash(Utility.EnsurePositive(dhShared));
                    }
                    else if (encryption == KeyEncryption.DHSHA256)
                    {
                        Tracer.Write("Decoding DHSHA256 Association.");
                        SHA256 sha256 = new SHA256Managed();
                        shaShared = sha256.ComputeHash(Utility.EnsurePositive(dhShared));
                    }

                    byte[] secret = new byte[mackey.Length];
                    for (int i = 0; i < mackey.Length; i++)
                    {
                        secret[i] = (byte)(mackey[i] ^ shaShared[i]);
                    }
                    association["mac_key"] = Convert.ToBase64String(secret);
                }
                else
                {
                    Tracer.Write("Error: Received plaintext association when expecting encrypted.");
                    return null;
                }
            }

            Tracer.Write("Building association");
            retassoc = new Association();
            retassoc.AssociationType = association["assoc_type"];
            retassoc.Expiration = DateTime.UtcNow.AddSeconds(Convert.ToDouble(association["expires_in"], CultureInfo.InvariantCulture));
            retassoc.Handle = association["assoc_handle"];
            retassoc.ProtocolVersion = version;
            retassoc.Server = server.AbsoluteUri;
            retassoc.SessionType = association["session_type"];
            retassoc.Secret = Convert.FromBase64String(association["mac_key"]);
            return retassoc;
        }

        /// <summary>
        /// Determine the Extension plugin types that need to be loaded for the current request.
        /// </summary>
        /// <param name="arguments">Current request arguments</param>
        /// <returns>An array of types</returns>
        internal static Type[] GetRequiredExtensionPlugins(NameValueCollection arguments)
        {
            if (arguments == null) { throw new ArgumentNullException("arguments"); }

            List<Type> ret = new List<Type>();

            NameValueCollection currentnames = GetExtNamespaceAliases(arguments);
            foreach (string name in currentnames.Keys)
            {
                Type t = ExtensionRegistry.Get(name);
                if (t != null)
                {
                    ret.Add(t);
                }
            }

            // Special support for Simple Registration extension 1.0
            if (!ret.Contains(typeof(PlugIns.Extensions.SimpleRegistration)))
            {
                foreach (string key in arguments.Keys)
                {
                    if (key != null && key.StartsWith("openid.sreg.", StringComparison.OrdinalIgnoreCase))
                    {
                        ret.Add(typeof(PlugIns.Extensions.SimpleRegistration));
                        break;
                    }
                }
            }

            return ret.ToArray();
        }

        /// <summary>
        /// Look at the current arguments and load the extension plugins needed to service
        /// the request.
        /// </summary>
        /// <param name="rp">StateContainer object to store loaded plugins.</param>
        internal static void AutoLoadExtensionPlugins(StateContainer rp)
        {
            if (rp == null) { throw new ArgumentNullException("rp"); }
            Tracer.Write("Loading extension plugins");
            Type[] types = GetRequiredExtensionPlugins(rp.RequestArguments);
            foreach (Type t in types)
            {
                Tracer.Write("Loading plugin '" + t.ToString() + "'");
                Activator.CreateInstance(t, rp);
            }
        }
    }
}
