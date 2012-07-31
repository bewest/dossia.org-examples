using System;
using System.Collections.Generic;

namespace ExtremeSwank.OpenId.PlugIns.Discovery
{
    /// <summary>
    /// Holds all important data discovered during the discovery process.
    /// </summary>
    [Serializable]
    public class DiscoveryResult
    {
        Uri _ServerUrl;
        string _LocalId;
        string _ClaimedId;
        int _Priority;
        ProtocolVersion _AuthVersion = ProtocolVersion.V1Dot1;

        /// <summary>
        /// Gets or sets the discovered OpenID Provider URL.
        /// </summary>
        public Uri ServerUrl
        {
            get { return _ServerUrl; }
            set { _ServerUrl = value; }
        }

        /// <summary>
        /// Gets or sets the discovered Local Identitifier.
        /// </summary>
        /// <remarks>
        /// If no local identitifier was discovered, this should be
        /// set to the same value as ClaimedIdUrl.
        /// </remarks>
        public string LocalId
        {
            get { return _LocalId; }
            set { _LocalId = value; }
        }

        /// <summary>
        /// Gets or sets the discovered claimed identifier.
        /// </summary>
        public string ClaimedId
        {
            get { return _ClaimedId; }
            set { _ClaimedId = value; }
        }

        /// <summary>
        /// Gets or sets the authentication protocol version supported
        /// by the OpenID Provider.
        /// </summary>
        public ProtocolVersion AuthVersion
        {
            get { return _AuthVersion; }
            set { _AuthVersion = value; }
        }

        /// <summary>
        /// Gets or sets the priority of the discovery result.
        /// </summary>
        public int Priority
        {
            get { return _Priority; }
            set { _Priority = value; }
        }
    }

    /// <summary>
    /// Compare DiscoveryResult object by the value of the Priority member.
    /// </summary>
    internal class DiscoveryResultComparer : IComparer<DiscoveryResult>
    {
        #region IComparer<DiscoveryResult> Members

        /// <summary>
        /// Compare a <see cref="DiscoveryResult"/> object with another <see cref="DiscoveryResult"/> object.
        /// </summary>
        /// <param name="x">First DiscoveryResult object.</param>
        /// <param name="y">Second DiscoveryResult object.</param>
        /// <returns>Result of comparison of the Priority member values.</returns>
        public int Compare(DiscoveryResult x, DiscoveryResult y)
        {
            return x.Priority.CompareTo(y.Priority);
        }

        #endregion
    }
}
