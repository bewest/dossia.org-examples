namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Functions that deal with quirks specific to certain OpenID Providers
    /// </summary>
    internal static class Quirks
    {
        /// <summary>
        /// Check the OpenID identity URL and confirm whether or not the authentication
        /// mode should be changed due to quirks in the OpenID Provider's implementation.
        /// </summary>
        /// <param name="endPoint">OpenID identity URL</param>
        /// <param name="mode">Mode currently set</param>
        /// <returns>Correct AuthenticationMode for the provider</returns>
        internal static AuthenticationMode CheckOpenIDMode(string endPoint, AuthenticationMode mode)
        {
            if (endPoint.ToUpperInvariant().Contains("://GETOPENID.COM/"))
            {
                return AuthenticationMode.Stateless;
            }
            return mode;
        }
    }
}
