using System;

namespace ExtremeSwank.OpenId.PlugIns.Discovery
{
    /// <summary>
    /// Holds the three different usable forms of a given OpenID Identity.
    /// </summary>
    public class NormalizationEntry
    {
        string _FriendlyId;
        string _NormalizedId;
        Uri _DiscoveryUrl;

        /// <summary>
        /// Gets or sets the database-compatible friendly identity name.
        /// </summary>
        public string FriendlyId
        {
            get { return _FriendlyId; }
            set { _FriendlyId = value; }
        }

        /// <summary>
        /// Gets or sets the ID, normalized per the OpenID specification
        /// </summary>
        public string NormalizedId
        {
            get { return _NormalizedId; }
            set { _NormalizedId = value; }
        }

        /// <summary>
        /// Gets or sets the discovery URL that should be used by
        /// the discovery engine.
        /// </summary>
        public Uri DiscoveryUrl
        {
            get { return _DiscoveryUrl; }
            set { _DiscoveryUrl = value; }
        }
    }
}
