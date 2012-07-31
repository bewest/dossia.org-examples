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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Security.Cryptography.X509Certificates;

namespace ExtremeSwank.OAuth
{
    /// <summary>
    /// Shared utility methods between OAuth client and server implementations.
    /// </summary>
    static class OAuthUtility
    {
        /// <summary>
        /// Serialize an object to a string.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="input">The object to serialize.</param>
        /// <returns>A string representing the object.</returns>
        internal static string Serialize<T>(T input) where T : class
        {
            if (input == null) return null;
            string output = null;

            using (MemoryStream s = new MemoryStream())
            {
                using (Stream gzs = new GZipStream(s, CompressionMode.Compress, true))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(gzs, input);
                }

                byte[] data = new byte[s.Length];
                s.Seek(0, 0);
                s.Read(data, 0, data.Length);

                output = Convert.ToBase64String(data, 0, data.Length, Base64FormattingOptions.None);
            }
            return output;
        }

        /// <summary>
        /// Convert previously serialized object data back into an object. 
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="input">The input data.</param>
        /// <returns>A populated object.</returns>
        internal static T Deserialize<T>(string input) where T : class
        {
            T item = null;
            if (String.IsNullOrEmpty(input)) return null;
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] data = Convert.FromBase64String(input);
                ms.Write(data, 0, data.Length);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                BinaryFormatter bf = new BinaryFormatter();

                using (GZipStream gzs = new GZipStream(ms, CompressionMode.Decompress))
                {
                    item = bf.Deserialize(gzs) as T;
                }
            }
            return item;
        }

        /// <summary>
        /// Translate SignatureMethod value into a protcol-compatible string.
        /// </summary>
        /// <param name="sigMethod">The value to translate.</param>
        /// <returns>A string representing the input value.</returns>
        internal static SignatureMethod StringToSigMethod(string sigMethod)
        {
            switch (sigMethod)
            {
                case "HMAC-SHA1":
                    return SignatureMethod.HmacSha1;
                case "RSA-SHA1":
                    return SignatureMethod.RsaSha1;
                case "PLAINTEXT":
                    return SignatureMethod.Plaintext;
                case "HMAC-SHA256":
                    return SignatureMethod.HmacSha256;
                case "HMAC-SHA512":
                    return SignatureMethod.HmacSha512;
            }
            throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture, "Signature method '{0}' is not yet supported.", sigMethod.ToString()));
        }

        /// <summary>
        /// Convert a SignatureMethod to a OAuth-compatible
        /// string representation.
        /// </summary>
        /// <param name="method">Input value.</param>
        /// <returns>String in OAuth-compatible format.</returns>
        internal static string SigMethodToString(SignatureMethod method)
        {
            switch (method)
            {
                case SignatureMethod.HmacSha1:
                    return "HMAC-SHA1";
                case SignatureMethod.Plaintext:
                    return "PLAINTEXT";
                case SignatureMethod.RsaSha1:
                    return "RSA-SHA1";
                case SignatureMethod.HmacSha256:
                    return "HMAC-SHA256";
                case SignatureMethod.HmacSha512:
                    return "HMAC-SHA512";
                default:
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "{0} is not a valid Signature Method.", method.ToString()), "method");
            }
        }
        
        /// <summary>
        /// Strip all arguments from a URI.
        /// </summary>
        /// <param name="uri">URI to process.</param>
        /// <returns>A URI with no query arguments or fragments.</returns>
        static Uri BaseUrl(Uri uri)
        {
            UriBuilder ub = new UriBuilder(uri);
            ub.Query = null;
            ub.Fragment = null;
            return ub.Uri;
        }

        /// <summary>
        /// Generate an OAuth Base String for signing.
        /// </summary>
        /// <param name="uri">URL for the request.</param>
        /// <param name="args">Arguments to include in the string.</param>
        /// <param name="method">HTTP request method.</param>
        /// <returns>A combined base string, per the OAuth specification.</returns>
        internal static string GenerateBaseString(Uri uri, NameValueCollection args, string method)
        {
            string bs = String.Format(CultureInfo.InvariantCulture, "{0}&{1}&{2}", method, UrlEncode(BaseUrl(uri).AbsoluteUri), UrlEncode(ArgsToVal(args, AuthenticationMethod.Get)));
            Trace.WriteLine(bs, "BaseString");
            return bs;
        }

        /// <summary>
        /// Perform URL Encoding on a string per the OAuth specification.
        /// </summary>
        /// <remarks>
        /// OAuth specification is more rigid than standard URL encoding.
        /// For instance, all hexadecimal character codes must be in all
        /// upper-case.
        /// </remarks>
        /// <param name="str">Input string.</param>
        /// <returns>URL Encoded string.</returns>
        static string UrlEncode(string str)
        {
            if (String.IsNullOrEmpty(str)) return null;
            string UrlEncodedString = HttpUtility.UrlEncode(str);
            StringBuilder s = new StringBuilder(UrlEncodedString);
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '%')
                {
                    if (Char.IsDigit(s[i + 1]) && Char.IsLetter(s[i + 2]))
                    {
                        s[i + 2] = Char.ToUpper(s[i + 2], CultureInfo.InvariantCulture);
                    }
                }
            }

            s = s.Replace("(", "%28");
            s = s.Replace(")", "%29");

            return s.ToString();
        }

        /// <summary>
        /// Generate an OAuth-compliant signature.
        /// </summary>
        /// <param name="text">Input test.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="tokenSecret">The Token secret.</param>
        /// <param name="sigMethod">The type of signature to generate.</param>
        /// <param name="rsaCert">The X509 certificate containing the private key to be used with RSA-SHA1.</param>
        /// <returns>An OAuth-compliant signature string.</returns>
        internal static string GenerateSignature(string text, string consumerSecret, string tokenSecret, X509Certificate2 rsaCert, SignatureMethod sigMethod)
        {
            string inputstr = String.Format(CultureInfo.InvariantCulture, "{0}&{1}", UrlEncode(consumerSecret), UrlEncode(tokenSecret));

            string retstring = null;

            switch (sigMethod)
            {
                case SignatureMethod.HmacSha1:
                    HMACSHA1 hmsha = new HMACSHA1(Encoding.ASCII.GetBytes(inputstr), true);
                    byte[] bin = Encoding.UTF8.GetBytes(text);
                    retstring = Convert.ToBase64String(hmsha.ComputeHash(bin, 0, bin.Length), Base64FormattingOptions.None);
                    break;
                case SignatureMethod.HmacSha256:
                    HMACSHA256 hmsha256 = new HMACSHA256(Encoding.ASCII.GetBytes(inputstr));
                    byte[] bin256 = Encoding.UTF8.GetBytes(text);
                    retstring = Convert.ToBase64String(hmsha256.ComputeHash(bin256, 0, bin256.Length), Base64FormattingOptions.None);
                    break;
                case SignatureMethod.HmacSha512:
                    HMACSHA512 hmsha512 = new HMACSHA512(Encoding.ASCII.GetBytes(inputstr));
                    byte[] bin512 = Encoding.UTF8.GetBytes(text);
                    retstring = Convert.ToBase64String(hmsha512.ComputeHash(bin512, 0, bin512.Length), Base64FormattingOptions.None);
                    break;
                case SignatureMethod.RsaSha1:
                    AsymmetricAlgorithm algo = rsaCert.PrivateKey;
                    RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(algo);
                    formatter.SetHashAlgorithm("SHA1");
                    byte[] input = Encoding.UTF8.GetBytes(text);
                    byte[] hash = SHA1.Create().ComputeHash(input);
                    retstring = Convert.ToBase64String(formatter.CreateSignature(hash));
                    break;
                case SignatureMethod.Plaintext:
                    retstring = inputstr;
                    break;
            }
            return retstring;
        }

        /// <summary>
        /// Generate a valid Realm for a URI.
        /// </summary>
        /// <param name="uri">Input URI.</param>
        /// <returns>A valid realm.</returns>
        internal static string Realm(Uri uri)
        {
            UriBuilder ub = new UriBuilder(uri);
            ub.Path = null;
            ub.Query = null;
            ub.Fragment = null;
            return ub.Uri.AbsoluteUri;
        }

        /// <summary>
        /// Generates a timestamp representing the current time.
        /// </summary>
        /// <returns>64-bit integer representing the number of seconds since the epoch.</returns>
        internal static long Timestamp()
        {
            DateTime epoch = DateTime.Parse("01/01/1970 00:00:00 GMT", CultureInfo.InvariantCulture).ToUniversalTime();
            DateTime now = DateTime.UtcNow;
            TimeSpan ts = now - epoch;
            return Convert.ToInt64(ts.TotalSeconds);
        }

        /// <summary>
        /// Transforms all arguments in a NameValueCollection into a string.
        /// </summary>
        /// <param name="arguments">Arguments to process.</param>
        /// <param name="format">The format of the return string.</param>
        /// <returns>A formatted string containing all arguments.</returns>
        internal static string ArgsToVal(NameValueCollection arguments, AuthenticationMethod format)
        {
            List<string> retargs = new List<string>();
            string retstring = null;

            switch (format)
            {
                case AuthenticationMethod.Header:
                    foreach (string key in arguments.AllKeys)
                    {
                        retargs.Add(String.Format(CultureInfo.InvariantCulture, "{0}=\"{1}\"", key, UrlEncode(arguments[key])));
                    }
                    retargs.Sort();
                    retstring = string.Join(", ", retargs.ToArray());
                    break;
                case AuthenticationMethod.Post:
                    foreach (string key in arguments.AllKeys)
                    {
                        retargs.Add(String.Format(CultureInfo.InvariantCulture, "{0}={1}", key, UrlEncode(arguments[key])));
                    }
                    retargs.Sort();
                    retstring = string.Join("&", retargs.ToArray());
                    break;
                case AuthenticationMethod.Get:
                    foreach (string key in arguments.AllKeys)
                    {
                        retargs.Add(String.Format(CultureInfo.InvariantCulture, "{0}={1}", key, UrlEncode(arguments[key])));
                    }
                    retargs.Sort();
                    retstring = string.Join("&", retargs.ToArray());
                    break;
            }
            return retstring;
        }
    }
}
