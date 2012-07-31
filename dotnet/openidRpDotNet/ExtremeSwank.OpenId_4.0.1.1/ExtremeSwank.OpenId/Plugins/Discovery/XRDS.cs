using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace ExtremeSwank.OpenId.PlugIns.Discovery
{
    /// <summary>
    /// XRDS Discovery Plugin.  Provides everything needed to
    /// discover OpenIDs using XRDS documents.
    /// </summary>
    [Serializable]
    public class Xrds : IDiscovery
    {
        const string _Name = "XRDS Discovery Plugin";
        ProtocolVersion _PV = ProtocolVersion.Invalid;
        private StateContainer _Parent;

        /// <summary>
        /// Gets the name of this discovery plugin.
        /// </summary>
        /// <remarks>Always returns "XRDS Discovery Plugin".</remarks>
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

        private static readonly string[] Prefixes = { "=", "@", "+", "$", "!", "xri://" };
        /// <summary>
        /// Accepts a claimed identifier and returns
        /// the normalized identifier, and an end-point URL.
        /// </summary>
        /// <param name="openid">String containing claimed identifier.</param>
        /// <returns>A populated NormalizationEntry object, or null if the identitifer cannot
        /// be processed by this plug-in.</returns>
        public NormalizationEntry ProcessId(string openid)
        {
            NormalizationEntry ne = new NormalizationEntry();
            if (openid.StartsWith("http://xri.net/", StringComparison.OrdinalIgnoreCase))
            {
                openid = openid.Substring(15);
            }
            if (openid.StartsWith("https://xri.net/", StringComparison.OrdinalIgnoreCase)) 
            {
                openid = openid.Substring(16);
            }

            foreach (string prefix in Prefixes)
            {
                if (openid.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (prefix.Length == 1)
                    {
                        ne.FriendlyId = openid;
                        ne.NormalizedId = openid;
                        ne.DiscoveryUrl = new Uri("https://xri.net/" + openid);
                    }
                    else
                    {
                        string a = openid;
                        a = a.Substring(prefix.Length, a.Length - prefix.Length);
                        ne.FriendlyId = a;
                        ne.NormalizedId = a;
                        ne.DiscoveryUrl = new Uri("https://xri.net/" + a);
                    }
                }
            }
            if (ne.DiscoveryUrl == null)
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
            if (!content.Contains("<?xml"))
            {
                return null;
            }
            int xmlbegin = content.IndexOf("<?xml", StringComparison.Ordinal);
            string fixedcontent = content.Substring(xmlbegin);

            XmlDocument xd = new XmlDocument();
            try
            {
                xd.LoadXml(fixedcontent);
            }
            catch (XmlException xe)
            {
                Tracer.Write("XML decode failed: " + xe.Message);
                return null;
            }

            foreach (IExtension ext in Parent.ExtensionPlugIns)
            {
                IXrdsConsumer ixc = ext as IXrdsConsumer;
                if (ixc != null)
                {
                    ixc.ProcessXrds(xd);
                }
            }

            XmlNamespaceManager nsmanager = new XmlNamespaceManager(xd.NameTable);
            nsmanager.AddNamespace("openid", "http://openid.net/xmlns/1.0");
            nsmanager.AddNamespace("xrds", "xri://$xrds");
            nsmanager.AddNamespace("xrd", "xri://$xrd*($v*2.0)");

            XmlNode rootnode = xd.DocumentElement;

            List<DiscoveryResult> entries = new List<DiscoveryResult>();

            XmlNodeList xmlServices = rootnode.SelectNodes("/xrds:XRDS/xrd:XRD/xrd:Service", nsmanager);
            foreach (XmlNode node in xmlServices)
            {
                DiscoveryResult entry = new DiscoveryResult();
                entry.AuthVersion = ProtocolVersion.Invalid;

                foreach (XmlAttribute attr in node.Attributes)
                {
                    switch (attr.Name.ToUpperInvariant())
                    {
                        case "PRIORITY":
                            entry.Priority = Convert.ToInt32(attr.Value, CultureInfo.InvariantCulture);
                            break;
                        default:
                            break;
                    }
                }

                foreach (XmlNode servicenode in node.ChildNodes)
                {
                    string val = null;
                    if (servicenode.HasChildNodes)
                    {
                        val = servicenode.ChildNodes[0].Value;
                    }
                    if (val != null)
                    {
                        switch (servicenode.Name)
                        {
                            case "Type":
                                if (val.Contains("http://specs.openid.net/auth/2."))
                                {
                                    entry.AuthVersion = ProtocolVersion.V2Dot0;
                                }
                                else if (val.Contains("http://openid.net/signon/1."))
                                {
                                    if (entry.AuthVersion != ProtocolVersion.V2Dot0)
                                    {
                                        entry.AuthVersion = ProtocolVersion.V1Dot1;
                                    }
                                }
                                break;
                            case "URI":
                                entry.ServerUrl = new Uri(val);
                                break;
                            case "openid:Delegate":
                                entry.LocalId = val;
                                break;
                            case "LocalID":
                                entry.LocalId = val;
                                break;
                            default:
                                break;
                        }
                    }
                }
                if (entry.ServerUrl != null && entry.AuthVersion != ProtocolVersion.Invalid)
                {
                    entries.Add(entry);
                }
            }

            if (entries.Count > 0)
            {
                DiscoveryResultComparer comp = new DiscoveryResultComparer();
                entries.Sort(comp);
                _PV = entries[0].AuthVersion;
                return entries.ToArray();
            }
            return null;
        }
        /// <summary>
        /// Gets the newest OpenID protocol version supported
        /// by the Identity Provider.
        /// </summary>
        public ProtocolVersion Version
        {
            get { return _PV; }
        }
        /// <summary>
        /// Creates a new instance of Xrds.
        /// </summary>
        /// <param name="state">Parent StateContainer object</param>
        public Xrds(StateContainer state)
        {
            Parent = state;
            state.RegisterPlugIn(this);
        }

        /// <summary>
        /// Creates a new instance of Xrds.
        /// </summary>
        /// <param name="client">Parent <see cref="ClientCore"/> object</param>
        public Xrds(ClientCore client)
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
