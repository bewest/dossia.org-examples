
namespace ExtremeSwank.OpenId.PlugIns.Extensions
{
    /// <summary>
    /// Represents a mode supported by the Attribute Exchange extension.
    /// </summary>
    public enum AttributeExchangeMode
    {
        /// <summary>
        /// Retrieve data from the OpenID Provider.
        /// </summary>
        Fetch,
        /// <summary>
        /// Request that the OpenID Provider store data.
        /// </summary>
        Store
    }
}
