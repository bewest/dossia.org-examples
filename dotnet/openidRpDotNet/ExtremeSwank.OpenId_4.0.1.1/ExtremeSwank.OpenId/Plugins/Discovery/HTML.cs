using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ExtremeSwank.OpenId.PlugIns.Discovery
{
    /// <summary>
    /// HTML Discovery Plugin.  Provides everything needed to
    /// discover OpenIDs using HTML documents.
    /// </summary>
    [Serializable]
    public class Html : IDiscovery
    {
        const string _Name = "HTML Discovery Plugin";
        private StateContainer _Parent;

        /// <summary>
        /// Gets the human-readable name of this Discovery plug-in.
        /// </summary>
        /// <remarks>Always returns "HTML Discovery Plugin".</remarks>
        public string Name
        {
            get
            {
                return _Name;
            }
        }
        /// <summary>
        /// Gets or sets the parent StateContainer object.
        /// </summary>
        public StateContainer Parent
        {
            get { return _Parent; }
            set { _Parent = value; }
        }
        ProtocolVersion _PV;
        static readonly string[] _Prefixes = { "http://", "https://" };

        /// <summary>
        /// Processes a claimed identifier and returns a normalized ID and an endpoint URL.
        /// </summary>
        /// <param name="openid">Claimed identifier to process.</param>
        /// <returns>A populated NormalizationEntry object, or null if the identitifer cannot
        /// be processed by this plug-in.</returns>
        public NormalizationEntry ProcessId(string openid)
        {
            NormalizationEntry ne = new NormalizationEntry();

            if (openid.StartsWith("https://xri.net/", StringComparison.OrdinalIgnoreCase)
                || openid.StartsWith("http://xri.net/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            foreach (string prefix in _Prefixes)
            {
                if (openid.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    string a = openid;
                    a = a.Substring(prefix.Length, a.Length - prefix.Length);
                    ne.FriendlyId = a.Trim('/');
                    ne.DiscoveryUrl = new Uri(openid);
                    ne.NormalizedId = ne.DiscoveryUrl.AbsoluteUri;
                    return ne;
                }
            }

            try
            {
                ne.FriendlyId = openid.Trim('/');
                ne.DiscoveryUrl = new Uri("http://" + openid);
                ne.NormalizedId = ne.DiscoveryUrl.AbsoluteUri;
            }
            catch (UriFormatException)
            {
                return null;
            }

            return ne;
        }
        /// <summary>
        /// Parse HTTP response for OpenID Identity Providers.
        /// </summary>
        /// <param name="content">HTTP response content to parse.</param>
        /// <returns>An array of DiscoveryResult objects.</returns>
        public DiscoveryResult[] Discover(string content)
        {
            List<string> supportedVersions = new List<string>();

            string newcontent = Utility.RemoveHtmlComments(content);

            Regex rx_linktag1 = new Regex("<link[^>]*rel=[\"']([^\"']+)[\"'][^>]*href=[\"']([^\"']+)[\"'][^>]*");
            Regex rx_linktag2 = new Regex("<link[^>]*href=[\"']([^\"']+)[\"'][^>]*rel=[\"']([^\"']+)[\"'][^>]*");

            MatchCollection m1 = rx_linktag1.Matches(newcontent);
            MatchCollection m2 = rx_linktag2.Matches(newcontent);
            List<string[]> links = new List<string[]>();

            foreach (Match m in m1)
            {
                string[] l = { m.Groups[1].Value, m.Groups[2].Value };
                links.Add(l);
            }
            foreach (Match m in m2)
            {
                string[] l = { m.Groups[2].Value, m.Groups[1].Value };
                links.Add(l);
            }

            DiscoveryResult de1 = new DiscoveryResult();
            de1.AuthVersion = ProtocolVersion.V1Dot1;
            DiscoveryResult de2 = new DiscoveryResult();
            de2.AuthVersion = ProtocolVersion.V2Dot0;

            foreach (string[] link in links)
            {
                string relattr = link[0];
                string hrefattr = link[1];

                Uri u = null;

                if (relattr.Contains("openid2.provider"))
                {
                    Uri.TryCreate(hrefattr, UriKind.Absolute, out u);
                    if (u != null)
                    {
                        de2.ServerUrl = u;
                        supportedVersions.Add("2.0");
                    }
                }
                else if (relattr.Contains("openid.server"))
                {
                    Uri.TryCreate(hrefattr, UriKind.Absolute, out u);
                    if (u != null)
                    {
                        de1.ServerUrl = u;
                        supportedVersions.Add("1.x");
                    }
                }
                else if (relattr.Contains("openid2.local_id"))
                {
                    de2.LocalId = hrefattr;
                    supportedVersions.Add("2.0");
                }
                else if (relattr.Contains("openid.delegate"))
                {
                    de1.LocalId = hrefattr;
                    supportedVersions.Add("1.x");
                }
            }

            if (supportedVersions.Contains("2.0"))
            {
                _PV = ProtocolVersion.V2Dot0;
            }
            else if (supportedVersions.Contains("1.x"))
            {
                _PV = ProtocolVersion.V1Dot1;
            }

            List<DiscoveryResult> list = new List<DiscoveryResult>();

            if (de2.ServerUrl != null)
            {
                list.Add(de2);
            }
            if (de1.ServerUrl != null)
            {
                list.Add(de1);
            }

            if (list.Count == 0) { return null; }
            return list.ToArray();
        }
        /// <summary>
        /// Gets the highest version of OpenID protocol supported by discovered Identity Provider.
        /// </summary>
        public ProtocolVersion Version
        {
            get
            {
                return _PV;
            }
        }
        /// <summary>
        /// Creates a new HTML discovery plugin and automatically registers it with
        /// the supplied StateContainer object.
        /// </summary>
        /// <param name="state">Parent StateContainer object</param>
        public Html(StateContainer state)
        {
            Parent = state;
            state.RegisterPlugIn(this);
        }

        /// <summary>
        /// Creates a new HTML discovery plugin and automatically registers it with
        /// the supplied StateContainer object.
        /// </summary>
        /// <param name="client">Parent ClientCore object.</param>
        public Html(ClientCore client)
        {
            Parent = client.StateContainer;
            Parent.RegisterPlugIn(this);
        }

        /// <summary>
        /// Get the human-readable name of this plug-in.
        /// </summary>
        /// <returns>A string containing the plug-in name.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
