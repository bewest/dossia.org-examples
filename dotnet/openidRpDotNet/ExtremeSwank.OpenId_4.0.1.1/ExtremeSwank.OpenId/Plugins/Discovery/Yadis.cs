using System;
using System.Text.RegularExpressions;

namespace ExtremeSwank.OpenId.PlugIns.Discovery
{
    /// <summary>
    /// Yadis Discovery Plugin.
    /// Provides everything needed to perform Yadis discovery.
    /// Depends on <see cref="Xrds" /> to decode resulting XRDS document.
    /// </summary>
    [Serializable]
    public class Yadis : IDiscovery
    {
        const string _Name = "Yadis Discovery Plugin";
        private StateContainer _Parent;

        /// <summary>
        /// Gets the human-readable name of the discovery plugin.
        /// </summary>
        /// <remarks>Always returns "Yadis Discovery Plugin".</remarks>
        public string Name
        {
            get
            {
                return _Name;
            }
        }
        /// <summary>
        /// Parent OpenID object.
        /// </summary>
        public StateContainer Parent
        {
            get { return _Parent; }
            set { _Parent = value; }
        }
        ProtocolVersion _PV;
        /// <summary>
        /// Not used.  Always returns null.
        /// </summary>
        /// <param name="openid">Claimed identifier.</param>
        /// <returns>Null</returns>
        public NormalizationEntry ProcessId(string openid)
        {
            return null;
        }
        /// <summary>
        /// Perform Yadis discovery on a HTTP response, automatically retrieving and processing
        /// discovered documents.
        /// </summary>
        /// <param name="content">HTTP response content to parse.</param>
        /// <returns>An array of DiscoveryResult objects.</returns>
        public DiscoveryResult[] Discover(string content)
        {
            string xrds_url = GetURL(content);
            if (!String.IsNullOrEmpty(xrds_url))
            {
                Xrds x = new Xrds(Parent);
                string actualLocation = null;
                string xrdsdoc = Utility.MakeRequest(new Uri(xrds_url), out actualLocation);
                if (xrdsdoc != null)
                {
                    DiscoveryResult[] ret = x.Discover(xrdsdoc);
                    _PV = x.Version;
                    return ret;
                }
            }
            return null;
        }
        private static string GetURL(string content)
        {
            string[] arrcon = content.Split('\n');
            int i = 0;
            while (i < arrcon.Length)
            {
                if (arrcon[i].StartsWith("X-XRDS-Location:", StringComparison.OrdinalIgnoreCase))
                {
                    char[] delimiter = { ':' };
                    string[] keyval = arrcon[i].Split(delimiter, 2);
                    string newurl = keyval[1].Trim();
                    return newurl;
                }
                i++;
            }

            string newcontent = Utility.RemoveHtmlComments(content);

            Regex xrds1 = new Regex("<meta[^>]*http-equiv=\"X-XRDS-Location\"[^>]*content=\"([^\"]+)\"[^>]*");
            Regex xrds2 = new Regex("<meta[^>]*content=\"([^\"]+)\"[^>]*http-equiv=\"X-XRDS-Location\"[^>]*");
            Match matches1 = xrds1.Match(newcontent);
            Match matches2 = xrds2.Match(newcontent);

            string url = "";

            if (!String.IsNullOrEmpty(matches1.Groups[1].Value)) { url = matches1.Groups[1].Value; }
            else if (!String.IsNullOrEmpty(matches2.Groups[1].Value)) { url = matches2.Groups[1].Value; }

            if (!String.IsNullOrEmpty(url))
            {
                return url;
            }
            return "";
        }
        /// <summary>
        /// Highest version of OpenID protocol supported by the discovered Identity Provider.
        /// </summary>
        public ProtocolVersion Version
        {
            get
            {
                return _PV;
            }
        }
        /// <summary>
        /// Creates a new instance of Yadis.
        /// </summary>
        /// <param name="state">Parent <see cref="StateContainer" /> object.</param>
        public Yadis(StateContainer state)
        {
            Parent = state;
            state.RegisterPlugIn(this);
        }

        /// <summary>
        /// Create a new instance of Yadis.
        /// </summary>
        /// <param name="client">Parent <see cref="ClientCore" /> object.</param>
        public Yadis(ClientCore client)
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
