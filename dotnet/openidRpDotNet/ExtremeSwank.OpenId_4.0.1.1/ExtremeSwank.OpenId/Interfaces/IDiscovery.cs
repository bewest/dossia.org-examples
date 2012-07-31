using ExtremeSwank.OpenId.PlugIns.Discovery;

namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Interface used for Discovery plugins.
    /// Discovery plugins extend the OpenID Client to support additional identifier discovery methods.
    /// </summary>
    public interface IDiscovery
    {
        /// <summary>
        /// Human-readable name of plugin.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Parent <see cref="StateContainer" /> object.
        /// </summary>
        StateContainer Parent { get; }
        /// <summary>
        /// Method called during discovery process.
        /// </summary>
        /// <param name="content">HTTP response output from request.</param>
        /// <returns>An array of <see cref="DiscoveryResult"/> objects.</returns>
        DiscoveryResult[] Discover(string content);
        /// <summary>
        /// Method called prior to discovery process.  Accepts a claimed identifier and returns
        /// the normalized identifier, and an endpoint URL.
        /// </summary>
        /// <param name="openid">String containing claimed identifier.</param>
        /// <returns>A populated <see cref="NormalizationEntry"/> object.</returns>
        NormalizationEntry ProcessId(string openid);
        /// <summary>
        /// Based on discovery, returns highest protocol version supported by endpoint. Used by consumer
        /// to determine which version of protocol to use when connecting to Identity Provider.
        /// </summary>
        ProtocolVersion Version { get; }
    }

}
