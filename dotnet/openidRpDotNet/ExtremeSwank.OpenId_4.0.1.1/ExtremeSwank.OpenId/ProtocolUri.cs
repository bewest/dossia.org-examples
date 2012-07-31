using System;

namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Central list of namespace URIs used for OpenID Extensions.
    /// </summary>
    internal static class ProtocolUri
    {
        /// <summary>
        /// Attribute Exchange extension 1.0
        /// </summary>
        internal static readonly Uri AttributeExchange1Dot0 = new Uri("http://openid.net/srv/ax/1.0");
        /// <summary>
        /// Simple Registration extension 1.1
        /// </summary>
        internal static readonly Uri SimpleRegistration1Dot1 = new Uri("http://openid.net/extensions/sreg/1.1");
        /// <summary>
        /// Provider Authentication Policy extension 1.0
        /// </summary>
        internal static readonly Uri AuthenticationPolicy1Dot0 = new Uri("http://specs.openid.net/extensions/pape/1.0");
        /// <summary>
        /// OpenID 2.0
        /// </summary>
        internal static readonly Uri OpenId2Dot0 = new Uri("http://specs.openid.net/auth/2.0");
        /// <summary>
        /// Identifier Select - used as Identiy for Directed Identity requests
        /// </summary>
        internal static readonly Uri IdentifierSelect = new Uri("http://specs.openid.net/auth/2.0/identifier_select");
        /// <summary>
        /// OAuth+OpenID 1.0
        /// </summary>
        internal static readonly Uri OAuth1Dot0 = new Uri("http://specs.openid.net/extensions/oauth/1.0");
    }
}
