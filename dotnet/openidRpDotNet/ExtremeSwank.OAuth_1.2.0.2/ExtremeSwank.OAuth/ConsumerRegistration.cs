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
using System.Web;
using System.Security.Cryptography.X509Certificates;

namespace ExtremeSwank.OAuth
{
    /// <summary>
    /// Represents an OAuth consumer that is registered with the OAuth service provider.
    /// </summary>
    [Serializable]
    public class ConsumerRegistration
    {
        byte[] CertificateData;

        /// <summary>
        /// Key for the Consumer.  Typically the registered FQDN, or an account name.
        /// </summary>
        public string ConsumerKey { get; private set; }
        /// <summary>
        /// A unique, randomized value.  Use OAuthServer.GenerateRandomValue() to ensure
        /// the value is secure.
        /// </summary>
        public string ConsumerSecret { get; private set; }

        /// <summary>
        /// The X509 certificate containing the public key to be used for RSA-SHA1 signature verification.
        /// </summary>
        public X509Certificate2 RsaCertificate
        {
            get
            {
                return new X509Certificate2(CertificateData);
            }
        }

        /// <summary>
        /// Creates a new instance of ConsumerRegistration.
        /// </summary>
        /// <param name="consumerKey">Key for the Consumer.  Typically the registered FQDN, or an account name.</param>
        /// <param name="consumerSecret">A unique, randomized value.  Use OAuthServer.GenerateRandomValue() to ensure the value is secure.</param>
        /// <param name="rsaCertificate">The X509 certificate that will be used with RSA-SHA1 signature verification.</param>
        public ConsumerRegistration(string consumerKey, string consumerSecret, X509Certificate2 rsaCertificate)
        {
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
            CertificateData = rsaCertificate.RawData;
        }

        /// <summary>
        /// Creates a new instance of ConsumerRegistration.
        /// </summary>
        /// <param name="consumerKey">Key for the Consumer.  Typically the registered FQDN, or an account name.</param>
        /// <param name="consumerSecret">A unique, randomized value.  Use OAuthServer.GenerateRandomValue() to ensure the value is secure.</param>
        public ConsumerRegistration(string consumerKey, string consumerSecret)
        {
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
        }

        /// <summary>
        /// Re-build a ConsumerRegistration object using
        /// serialized data from the object's Export method.
        /// </summary>
        /// <param name="objectData">The serialized object data.</param>
        /// <returns>The specifed token object.</returns>
        public static ConsumerRegistration Restore(string objectData)
        {
            return OAuthUtility.Deserialize<ConsumerRegistration>(objectData);
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
}
