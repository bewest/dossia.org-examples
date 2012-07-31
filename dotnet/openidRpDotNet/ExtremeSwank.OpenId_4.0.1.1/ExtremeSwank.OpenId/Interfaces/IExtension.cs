using System;
using System.Collections.Generic;
using ExtremeSwank.OpenId.PlugIns.Discovery;
using System.Collections.Specialized;

namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Interface used for Extension plugins. 
    /// Extension plugins extend the OpenID Consumer to support additional data-passing specifications.
    /// </summary>
    public interface IExtension
    {
        /// <summary>
        /// Human-readable name of plugin.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Parent <see cref="StateContainer"/> StateContainer object.
        /// </summary>
        StateContainer Parent { get; }
        /// <summary>
        /// Advertised namespace this plug-in supports.
        /// </summary>
        Uri NamespaceUri { get; }
        /// <summary>
        /// Data to be passed to Identity Provider during initial
        /// authenticaton request.
        /// </summary>
        NameValueCollection BuildAuthorizationData(DiscoveryResult discResult);
        /// <summary>
        /// Perform any additional checking that needs to occur during validation.
        /// </summary>
        /// <remarks>If the extension should not perform validation, always return true.</remarks>
        /// <returns>Returns true if validation is successful, false if not.</returns>
        bool Validation();
        /// <summary>
        /// Populate a give OpenIdUser object with extension data received from 
        /// the OpenID Provider.
        /// </summary>
        void PopulateUserObject(OpenIdUser userObject);
    }
}
