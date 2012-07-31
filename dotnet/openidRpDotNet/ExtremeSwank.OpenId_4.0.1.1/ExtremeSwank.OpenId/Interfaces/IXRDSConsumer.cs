using System.Xml.XPath;
using ExtremeSwank.OpenId.PlugIns.Discovery;

namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Interface used for Extension plugins that utilize XRDS data directly.
    /// </summary>
    public interface IXrdsConsumer
    {
        /// <summary>
        /// Parent <see cref="StateContainer"/> object.
        /// </summary>
        StateContainer Parent { get; }
        /// <summary>
        /// Process the XRDS data provided by the <see cref="Xrds"/> Discovery plugin.
        /// </summary>
        /// <param name="xrdsDoc">XmlDocument object containing XRDS document.</param>
        void ProcessXrds(IXPathNavigable xrdsDoc);
    }
}
