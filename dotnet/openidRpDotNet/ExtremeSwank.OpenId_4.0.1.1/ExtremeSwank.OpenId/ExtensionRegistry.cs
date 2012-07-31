using System;
using System.Collections.Generic;
using ExtremeSwank.OpenId.PlugIns.Extensions;

namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Provides a simple registry of OpenID Extension namespaces to
    /// the supporting IExtension type.
    /// </summary>
    /// <remarks>Facilitates auto-loading extensions based on data
    /// received from Identity Providers.</remarks>
    public static class ExtensionRegistry
    {
        static Dictionary<string, Type> _Registry;

        /// <summary>
        /// Look up a plugin's Type using its namespace URI.
        /// </summary>
        /// <param name="name">The namespace URI to match.</param>
        /// <returns>The Type matching the namespace URI.</returns>
        public static Type Get(string name) 
        {
            if (String.IsNullOrEmpty(name)) { return null; }
            if (_Registry == null) { _Registry = new Dictionary<string, Type>(); Fill(); }
            if (_Registry.ContainsKey(name)) { return _Registry[name]; }
            return null;
        }

        /// <summary>
        /// Add a plugin's Type to the registry. 
        /// </summary>
        /// <remarks>The registry is in-memory, and will only remain in the current processing context.
        /// By default, ExtensionRegistry automatically registers all IExtension
        /// Types shipped with the library.  If you are registering custom
        /// IExtension Types, ensure you register the Type upon each page load.</remarks>
        /// <param name="name">The namespace URI to register</param>
        /// <param name="type">The Type to register</param>
        public static void Set(string name, Type type)
        {
            if (_Registry == null) { _Registry = new Dictionary<string, Type>(); Fill(); }
            _Registry[name] = type;
        }

        static void Fill()
        {
            _Registry[ProtocolUri.AttributeExchange1Dot0.AbsoluteUri] = typeof(AttributeExchange);
            _Registry[ProtocolUri.SimpleRegistration1Dot1.AbsoluteUri] = typeof(SimpleRegistration);
            _Registry[ProtocolUri.AuthenticationPolicy1Dot0.AbsoluteUri] = typeof(AuthenticationPolicy);
        }

    }
}
