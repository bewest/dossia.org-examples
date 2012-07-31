using System;

namespace ExtremeSwank.OpenId.PlugIns.Extensions
{
    /// <summary>
    /// Some static methods that return authentication policy URIs.
    /// </summary>
    public static class AuthenticationPolicySchema
    {
        /// <summary>
        /// Returns the URI for Phishing-Resistant Authentication
        /// </summary>
        public static readonly Uri PhishingResistant = new Uri("http://schemas.openid.net/pape/policies/2007/06/phishing-resistant");

        /// <summary>
        /// Returns the URI for Multi-Factor Authentication
        /// </summary>
        public static readonly Uri MultipleFactor = new Uri("http://schemas.openid.net/pape/policies/2007/06/multi-factor");

        /// <summary>
        /// Returns the URI for Physical Multi-Factor Authentication
        /// </summary>
        public static readonly Uri PhysicalMultipleFactor = new Uri("http://schemas.openid.net/pape/policies/2007/06/multi-factor-physical");
    }

}
