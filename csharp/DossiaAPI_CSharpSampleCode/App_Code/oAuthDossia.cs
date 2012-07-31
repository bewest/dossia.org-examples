/*
 * Copyright 2009 Dossia
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Net;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Specialized;

namespace DossiaAPITest
{
    public class oAuthDossia : OAuthBase
    {
        #region Enum for setting Method type
        public enum Method { GET, POST, DELETE };
        #endregion

        #region Constants for API Call-SHOULD BE CONFIGURABLE 
        //-----------------------------URL CONFIG---------------------------------------
        //Dossia development environment request url
        public const string OATH_DEV_REQUEST_URL = "https://dev-api.dossia.org/authserver/request_token";
        //Dossia development environment authorize url
        public const string OATH_DEV_AUTHORIZE_URL = "https://dev-api.dossia.org/authserver/authorize";
        //Dossia development environment access url
        public const string OATH_DEV_ACCESS_URL = "https://dev-api.dossia.org/authserver/access_token";
        //Dossia API Call Url
        public const string DOSSIA_API_URL = "https://dev-api.dossia.org/dossia-restful-api/services/v2.0/";
        //--------------------------------------------------------------------------------

        //Get Records
        public const string API_GETRECORDS = "records";

        public const string OATH_CONSUMERKEY = "trigentWeight";
        public const string OATH_CONSUMER_SECRET = "trigentSecret";
        //-----------------------------------------------------------------------------------
       
        //Session values to pass the token id and secret from one page to other pages from APITest page
        public const string SESSION_TOKEN = "oAuthToken";
        public const string SESSION_TOKEN_SECRET = "oAuthTokenSecret";
        #endregion

        #region Private variables
        private string _token = "";
        private string _tokenSecret = "";
        #endregion

        #region Properties
        public string Token { get { return _token; } set { _token = value; } }
        public string TokenSecret { get { return _tokenSecret; } set { _tokenSecret = value; } }
        #endregion

        #region Pubilc Methods
        /// <summary>
        /// Method to get the tokens
        /// </summary>
        /// <returns></returns>
        public string GetTokens()
        {
            string redirectUrl = string.Empty;
            string authUrl = string.Empty;
            string signature = string.Empty;
            string baseUrl = string.Empty;
            //get the request for the Authorization url
            authUrl = this.AuthorizationLinkGet();            
            //set the redirect url
            redirectUrl = string.Concat(authUrl, "&oauth_callback=", HttpContext.Current.Request.Url.AbsoluteUri);
            //set the token and secret in session ,so it is used in the call back url page
            HttpContext.Current.Session[oAuthDossia.SESSION_TOKEN] = this.Token;
            HttpContext.Current.Session[oAuthDossia.SESSION_TOKEN_SECRET] = this.TokenSecret;
            return redirectUrl;
        }


        /// <summary>
        /// Exchange the request token for an access token.
        /// </summary>        
        public void AccessTokenGet()
        {
            this.Token = HttpContext.Current.Session[oAuthDossia.SESSION_TOKEN].ToString();
            this.TokenSecret = HttpContext.Current.Session[oAuthDossia.SESSION_TOKEN_SECRET].ToString();

            string response = oAuthWebRequest(Method.GET, OATH_DEV_ACCESS_URL, String.Empty);

            if (response.Length > 0)
            {
                //Store the Token and Token Secret
                NameValueCollection qs = HttpUtility.ParseQueryString(response);
                if (qs["oauth_token"] != null)
                {
                    this.Token = qs["oauth_token"];
                }
                if (qs["oauth_token_secret"] != null)
                {
                    this.TokenSecret = qs["oauth_token_secret"];
                }
            }
        }
        
        /// <summary>
        /// Get the link to dosia's authorization page for this application.
        /// </summary>
        /// <returns>The url with a valid request token, or a null string.</returns>
        public string AuthorizationLinkGet()
        {
            string ret = null;

            string response = oAuthWebRequest(Method.GET, OATH_DEV_REQUEST_URL, String.Empty);
            if (response.Length > 0)
            {
                //response contains token and token secret.  We only need the token.
                NameValueCollection qs = HttpUtility.ParseQueryString(response);
                if (qs["oauth_token"] != null)
                {
                    ret = OATH_DEV_AUTHORIZE_URL + "?oauth_token=" + qs["oauth_token"] + "&oauth_token_secret=" + qs["oauth_token_secret"];
                    _token = qs["oauth_token"].ToString();
                    _tokenSecret = qs["oauth_token_secret"].ToString();
                }
            }
            
            return ret;
        }

        /// <summary>
        /// Get the link to dosia's authorization page for this application for the post
        /// </summary>        
        public string AuthorizationLinkPost()
        {
            string ret = null;

            string response = oAuthWebRequest(Method.POST, OATH_DEV_REQUEST_URL, String.Empty);
            if (response.Length > 0)
            {
                //response contains token and token secret.  We only need the token.
                NameValueCollection qs = HttpUtility.ParseQueryString(response);
                if (qs["oauth_token"] != null)
                {
                    ret = OATH_DEV_AUTHORIZE_URL + "?oauth_token=" + qs["oauth_token"] + "&oauth_token_secret=" + qs["oauth_token_secret"];
                    _token = qs["oauth_token"].ToString();
                    _tokenSecret = qs["oauth_token_secret"].ToString();
                }
            }

            return ret;
        }

        /// <summary>
        /// Access to get the API use
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sToken"></param>
        /// <param name="sSecret"></param>
        /// <param name="sMethod"></param>
        /// <returns></returns>
        public string AccessToAPILink(string url,string sToken, string sSecret, string sMethod)
        {
            string text;
            string sNormalizedUrl;
            string outParameters;
            Uri uri = new Uri(url);

            OAuthBase oAuth = new OAuthBase();
            string sNonce = oAuth.GenerateNonce();
            string sTimeStamp = oAuth.GenerateTimeStamp();

            string sigt = oAuth.GenerateSignature(uri,
                    OATH_CONSUMERKEY, OATH_CONSUMER_SECRET,
                    sToken, sSecret,
                sMethod, sTimeStamp, sNonce,
                OAuthBase.SignatureTypes.HMACSHA1,
                  out sNormalizedUrl,
                out outParameters);

            sigt = HttpUtility.UrlEncode(sigt);

            StringBuilder sStringb = new StringBuilder(uri.ToString());

            sStringb.AppendFormat("?oauth_consumer_key={0}&", OATH_CONSUMERKEY);
            sStringb.AppendFormat("oauth_nonce={0}&", sNonce);
            sStringb.AppendFormat("oauth_timestamp={0}&", sTimeStamp);
            sStringb.AppendFormat("oauth_signature_method={0}&", "HMAC-SHA1");
            if (!(sToken.Equals("")))
            {
                sStringb.AppendFormat("oauth_token={0}&", sToken);
            }
            sStringb.AppendFormat("oauth_version={0}&", "1.0");
            sStringb.AppendFormat("oauth_signature={0}", sigt);
          
            text = (sStringb.ToString());


            return text;
        }
        
        /// <summary>
        /// Method to do the request and get the response
        /// </summary>
        /// <param name="restCall">url to call</param>
        /// <returns></returns>
        public string MakeWebRequest(string restCall)
        {
            
            return MakeRequest(restCall, Method.GET.ToString());
           
        }

        /// <summary>
        /// Method to do the request to delete the document.
        /// </summary>
        /// <param name="restCall">url to call</param>
        /// <returns></returns>
        public string MakeRequest(string restCall, string method)
        {
            string response_text;
            response_text = null;

            try
            {
                WebRequest request = HttpWebRequest.Create(restCall);
                request.Method = method;

                using (WebResponse response = request.GetResponse())

                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    response_text = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                throw ex;

            }
            return response_text;
        }

        /// <summary>
        /// Method to do the request and get the response
        /// </summary>
        /// <param name="restCall">url to call</param>
        /// <returns></returns>
        public string MakeWebRequest(string restCall,out string contentType, out byte[] responseContent)
        {
            string response_text;
            response_text = null;

            try
            {
                WebRequest request = HttpWebRequest.Create(restCall);

                using (WebResponse response = request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                {
                    contentType = response.ContentType;
                    StreamReader reader = new StreamReader(stream);

                    byte[] imageData = new byte[response.ContentLength];
                    stream.Read(imageData, 0, imageData.Length);

                    responseContent = imageData;

                    response_text = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return response_text;
        }

        /// <summary>
        /// Methdo for getting the post request
        /// </summary>
        /// <param name="apiUrl">url of the api</param>
        /// <param name="inputText">Input stream for post request</param>
        /// <returns></returns>
        public string MakePostRequest(string apiUrl, string inputText)
        {
            string responseText = string.Empty;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(inputText);

            WebRequest request = HttpWebRequest.Create(apiUrl);
            request.Method = "POST";
            request.ContentType = "application/xml";
            request.ContentLength = bytes.Length;
            Stream requestStream;
            HttpWebResponse response;

            using (requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            using (response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseText = reader.ReadToEnd();                       
                    }
                }
                else
                {
                    string message = String.Format("POST failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }
            }
            return responseText;
        }

        /// <summary>
        /// Method to handle the response code 200
        /// only for the method "Relating existing documents as parent and child documents"
        /// </summary>
        /// <param name="apiUrl"></param>
        /// <returns></returns>
        public string MakePostRequest(string apiUrl)
        {
            string responseText = string.Empty;
            //byte[] bytes = System.Text.Encoding.UTF8.GetBytes("");

            WebRequest request = HttpWebRequest.Create(apiUrl);
            request.Method = "POST";
            request.ContentType = "application/xml";
            //request.ContentLength = bytes.Length;
            //Stream requestStream;
            HttpWebResponse response;
            try
            {
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    //Equivalent to HTTP status 204. NoContent indicates that the request has been 
                    //successfully processed and that the response is intentionally blank.
                    if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.OK)
                    {
                        
                    }
                    else
                    {
                        string message = String.Format("POST failed. Received HTTP {0}", response.StatusCode);
                       throw new ApplicationException(message);
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
            return responseText;
        }




        /// <summary>
        /// Methdo for getting the post request
        /// </summary>
        /// <param name="apiUrl">url of the api</param>
        /// <param name="inputText">Input stream for post request</param>
        /// <returns></returns>
        public string MakePostRequest(string apiUrl, byte[] inputByte,string contentType)
        {
            string responseText = string.Empty;
            byte[] bytes = inputByte;

            WebRequest request = HttpWebRequest.Create(apiUrl);
            request.Method = "POST";
            request.ContentType = contentType;
            request.ContentLength = bytes.Length;
            Stream requestStream;
            HttpWebResponse response;

            using (requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            using (response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseText = reader.ReadToEnd();
                    }
                }
                else
                {
                    string message = String.Format("POST failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }
            }
            return responseText;
        }




        /// <summary>
        /// Method to get the extract base url
        /// </summary>
        /// <returns>returns a base url</returns>
        public string ExtractBaseURLFromURL()
        {
            Uri uri = new Uri(HttpContext.Current.Request.Url.AbsoluteUri);
            string redirectUrl = uri.GetLeftPart(UriPartial.Authority).ToString() + HttpContext.Current.Request.ApplicationPath;
            return redirectUrl;

        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Submit a web request using oAuth.
        /// </summary>
        /// <param name="method">GET or POST</param>
        /// <param name="url">The full url, including the querystring.</param>
        /// <param name="postData">Data to post (querystring format)</param>
        /// <returns>The web server response.</returns>
        private string oAuthWebRequest(Method method, string url, string postData)
        {
            string outUrl = "";
            string querystring = "";
            string ret = "";


            //Setup postData for signing.
            //Add the postData to the querystring.
            if (method == Method.POST)
            {
                if (postData.Length > 0)
                {
                    //Decode the parameters and re-encode using the oAuth UrlEncode method.
                    NameValueCollection qs = HttpUtility.ParseQueryString(postData);
                    postData = "";
                    foreach (string key in qs.AllKeys)
                    {
                        if (postData.Length > 0)
                        {
                            postData += "&";
                        }
                        qs[key] = HttpUtility.UrlDecode(qs[key]);
                        qs[key] = this.UrlEncode(qs[key]);
                        postData += key + "=" + qs[key];

                    }
                    if (url.IndexOf("?") > 0)
                    {
                        url += "&";
                    }
                    else
                    {
                        url += "?";
                    }
                    url += postData;
                }
            }

            Uri uri = new Uri(url);
            
            string nonce = this.GenerateNonce();
            string timeStamp = this.GenerateTimeStamp();

            //Generate Signature
            string sig = this.GenerateSignature(uri,
                OATH_CONSUMERKEY,
                OATH_CONSUMER_SECRET,
                this.Token,
                this.TokenSecret,
                method.ToString(),
                timeStamp,
                nonce,
                out outUrl,
                out querystring);

            querystring += "&oauth_signature=" + HttpUtility.UrlEncode(sig);

            //Convert the querystring to postData
            if (method == Method.POST)
            {
                postData = querystring;
                querystring = "";
            }

            if (querystring.Length > 0)
            {
                outUrl += "?";
            }

            ret = WebRequest(method, outUrl +  querystring, postData);

            return ret;
        }

        /// <summary>
        /// Web Request Wrapper
        /// </summary>
        /// <param name="method">Http Method</param>
        /// <param name="url">Full url to the web resource</param>
        /// <param name="postData">Data to post in querystring format</param>
        /// <returns>The web server response.</returns>
        private string WebRequest(Method method, string url, string postData)
        {
            HttpWebRequest webRequest = null;
            StreamWriter requestWriter = null;
            string responseData = "";
            
            webRequest = System.Net.HttpWebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = method.ToString();
            webRequest.ServicePoint.Expect100Continue = false;
            webRequest.UserAgent  = "Identify your application please.";
            webRequest.Timeout = 20000;

            if (method == Method.POST)
            {
                webRequest.ContentType = "application/x-www-form-urlencoded";

                //POST the data.
                requestWriter = new StreamWriter(webRequest.GetRequestStream());
                try
                {
                    requestWriter.Write(postData);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    requestWriter.Close();
                    requestWriter = null;
                }
            }

            responseData = WebResponseGet(webRequest);            
            

            webRequest = null;

            return responseData;

        }

        /// <summary>
        /// Process the web response.
        /// </summary>
        /// <param name="webRequest">The request object.</param>
        /// <returns>The response data.</returns>
        private string WebResponseGet(HttpWebRequest webRequest)
        {
            StreamReader responseReader = null;
            string responseData = "";

            try
            {
                responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                responseData = responseReader.ReadToEnd();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {               
               
            }

            return responseData;
        }
        #endregion

    }
}
