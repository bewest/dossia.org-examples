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

namespace ExtremeSwank.OAuth
{
    internal static class OAuthArguments
    {
        public const string OAuthToken = "oauth_token";
        public const string OAuthTokenSecret = "oauth_token_secret";
        public const string OAuthNonce = "oauth_nonce";
        public const string OAuthConsumerKey = "oauth_consumer_key";
        public const string OAuthSignature = "oauth_signature";
        public const string OAuthSignatureMethod = "oauth_signature_method";
        public const string OAuthTimestamp = "oauth_timestamp";
        public const string OAuthVersion = "oauth_version";
        public const string OAuthCallback = "oauth_callback";
        public const string XOAuthRequestorId = "xoauth_requestor_id";
        public const string Scope = "scope";
    }

    internal static class Strings
    {
        public const string Realm = "realm";
        public const string OAuth = "OAuth";
        public const string TokenStorageProvider = "OAuth_TokenStorageProvider";
        public const string DbStorageConnection = "OAuth_DbStorageConnection";
        public const string DbStorageTableName = "OAuth_DbStorageTableName";
        public const string ExRsaCertificateRequired = "RSA-SHA1 signing requires the RsaCertificate property to be set.";
    }
}
