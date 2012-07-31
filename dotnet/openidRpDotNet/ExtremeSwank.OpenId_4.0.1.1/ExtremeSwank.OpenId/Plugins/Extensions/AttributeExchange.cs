using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using ExtremeSwank.OpenId.PlugIns.Discovery;

namespace ExtremeSwank.OpenId.PlugIns.Extensions
{
    /// <summary>
    /// Extension plugin that provides support for the OpenID Attribute Exchange extension.
    /// </summary>
    /// <remarks>
    /// Not all OpenID Providers support all OpenID extensions.  If the expected data is
    /// not returned after a successful request, the OpenID Provider may not support this
    /// extension.
    /// </remarks>
    /// <example>
    /// Before using <see cref="ClientCore.CreateRequest"/>, create and configure
    /// this plug-in:
    /// <code>
    /// OpenIdClient client = new OpenIdClient();
    /// AttributeExchange ax = new AttributeExchange(client);
    /// 
    /// // Add required items
    /// ax.AddFetchItem(AttributeExchangeSchema.Email, 1, true);
    /// 
    /// // Add optional items
    /// ax.AddFetchItem(AttributeExchangeSchema.FullName, 1, false);
    /// ax.AddFetchItem(AttributeExchangeSchema.UserName, 1, false);
    /// 
    /// client.CreateRequest();
    /// </code>
    /// When receiving the response from the OpenID Provider:
    /// <code>
    /// OpenIdUser user = client.RetrieveUser();
    /// string userName = user.GetValue(AttributeExchangeSchema.UserName);
    /// string fullName = user.GetValue(AttributeExchangeSchema.FullName);
    /// string email = user.GetValue(AttributeExchangeSchema.Email);
    /// </code>
    /// </example>
    [Serializable]
    public class AttributeExchange : IExtension
    {
        private StateContainer _Parent;
        private const string prefix = "openid.ax.";
        private AttributeExchangeMode _Mode;
        private List<AttributeExchangeItem> _RequestData;
        private Uri _UpdateUrl;

        /// <summary>
        /// Creates a new instance of <see cref="AttributeExchange"/>.
        /// </summary>
        /// <param name="state">Parent <see cref="StateContainer"/> object to attach.</param>
        public AttributeExchange(StateContainer state)
        {
            _Parent = state;
            state.RegisterPlugIn(this);
            Mode = AttributeExchangeMode.Fetch;
            _RequestData = new List<AttributeExchangeItem>();
        }

        /// <summary>
        /// Creates a new instance of <see cref="AttributeExchange"/>.
        /// </summary>
        /// <param name="client">Parent <see cref="ClientCore"/> object to attach.</param>
        public AttributeExchange(ClientCore client)
        {
            _Parent = client.StateContainer;
            _Parent.RegisterPlugIn(this);
            Mode = AttributeExchangeMode.Fetch;
            _RequestData = new List<AttributeExchangeItem>();
        }

        /// <summary>
        /// The Update URL that the OpenID Provider will send updates to whenever the requested
        /// data is updated.
        /// </summary>
        /// <remarks><para>This is a feature that is supported by some OpenID Providers. Any time the
        /// data in this request is updated at the OpenID Provider, an update will be pushed to
        /// the URL supplied here.  If the OpenID Provider does not support this function, the 
        /// request is simply ignored.</para>
        /// <para>If you wish to receive these updates, use <see cref="AXReceiver"/>.</para>
        /// </remarks>
        public Uri UpdateUrl
        {
            get { return _UpdateUrl; }
            set { _UpdateUrl = value; }
        }

        /// <summary>
        /// Gets or sets the mode that will be used for this request.  Either a fetch or a store request.
        /// </summary>
        public AttributeExchangeMode Mode
        {
            get { return _Mode; }
            set { _Mode = value; }
        }

        /// <summary>
        /// Add an item to a Fetch request.
        /// </summary>
        /// <param name="idUri">The namespace URI for this item.</param>
        /// <param name="alias">A short alias to use for this item.  Use the same alias to retrieve data from the OpenIdUser object.</param>
        /// <param name="count">The number of values to return, if more than one value is recorded at the OpenID Provider.</param>
        /// <param name="required">Whether or not a response for this item is required.</param>
        /// <exception cref="InvalidOperationException">Attempted to add fetch item when it Store mode.</exception>
        public void AddFetchItem(string idUri, string alias, int count, bool required)
        {
            AddFetchItem(new Uri(idUri), alias, count, required);
        }

        /// <summary>
        /// Add an item to a Fetch request.
        /// </summary>
        /// <param name="idUri">The namespace URI for this item.</param>
        /// <param name="alias">A short alias to use for this item.  Use the same alias to retrieve data from the OpenIdUser object.</param>
        /// <param name="count">The number of values to return, if more than one value is recorded at the OpenID Provider.</param>
        /// <param name="required">Whether or not a response for this item is required.</param>
        /// <exception cref="InvalidOperationException">Attempted to add fetch item when it Store mode.</exception>
        public void AddFetchItem(Uri idUri, string alias, int count, bool required)
        {
            if (Mode == AttributeExchangeMode.Store) { throw new InvalidOperationException("Incorrect Mode. Must set Mode to AttributeExchangeMode.Fetch to add fetch items."); }
            AttributeExchangeItem aei = new AttributeExchangeItem();
            aei.IdUri = idUri;
            aei.Alias = alias;
            aei.Count = count;
            aei.Required = required;
            _RequestData.Add(aei);
        }

        /// <summary>
        /// Add an item to a Fetch request.
        /// </summary>
        /// <param name="schemaDef">The schema definition for this item.</param>
        /// <param name="count">The number of values to return, if more than one value is recorded at the OpenID Provider.</param>
        /// <param name="required">Whether or not a response for this item is required.</param>
        /// <exception cref="InvalidOperationException">Attempted to add fetch item when it Store mode.</exception>
        public void AddFetchItem(SchemaDef schemaDef, int count, bool required)
        {
            AddFetchItem(schemaDef.Uri, schemaDef.Alias, count, required);
        }

        /// <summary>
        /// Add an item to a Store request.
        /// </summary>
        /// <param name="idUri">The namespace URI for this item.</param>
        /// <param name="alias">A short alias for this item.</param>
        /// <param name="values">One or more values to record in this item.</param>
        /// <exception cref="InvalidOperationException">Attempted to add store item in Fetch mode.</exception>
        public void AddStoreItem(string idUri, string alias, params string[] values)
        {
            AddStoreItem(new Uri(idUri), alias, values);
        }

        /// <summary>
        /// Add an item to a Store request.
        /// </summary>
        /// <param name="idUri">The namespace URI for this item.</param>
        /// <param name="alias">A short alias for this item.</param>
        /// <param name="values">One or more values to record in this item.</param>
        /// <exception cref="InvalidOperationException">Attempted to add store item in Fetch mode.</exception>
        public void AddStoreItem(Uri idUri, string alias, params string[] values)
        {
            if (Mode == AttributeExchangeMode.Fetch) { throw new InvalidOperationException("Incorrect Mode. Must set Mode to AttributeExchangeMode.Store to add store items."); }
            AttributeExchangeItem aei = new AttributeExchangeItem();
            aei.IdUri = idUri;
            aei.Alias = alias;
            aei.Count = values.Length;
            aei.Values = values;
            _RequestData.Add(aei);
        }

        /// <summary>
        /// Add an item to a Store request.
        /// </summary>
        /// <param name="schemaDef">SchemaDef representing the value(s) to store.</param>
        /// <param name="values">One or more values to record in this item.</param>
        /// <exception cref="InvalidOperationException">Attempted to add store item in Fetch mode.</exception>
        public void AddStoreItem(SchemaDef schemaDef, params string[] values)
        {
            AddStoreItem(schemaDef.Uri, schemaDef.Alias, values);
        }

        #region IExtension Members

        /// <summary>
        /// Gets the human-readable name of this extension.
        /// </summary>
        /// <remarks>Always returns "OpenID Attribute Exchange 1.0".</remarks>
        public string Name
        {
            get { return "OpenID Attribute Exchange 1.0"; }
        }

        /// <summary>
        /// Gets the parent <see cref="StateContainer"/> object.
        /// </summary>
        public StateContainer Parent
        {
            get { return _Parent; }
        }

        /// <summary>
        /// Gets the namespace URI for this extension.
        /// </summary>
        /// <remarks>See <see cref="ProtocolUri"/> for more information.</remarks>
        public Uri NamespaceUri
        {
            get { return ProtocolUri.AttributeExchange1Dot0; }
        }

        /// <summary>
        /// Gets the key-value data to be sent to Identity Provider during
        /// authentication request.
        /// </summary>
        /// <param name="discResult">DiscoveryResult object to use.</param>
        public NameValueCollection BuildAuthorizationData(DiscoveryResult discResult)
        {
            NameValueCollection pms = new NameValueCollection();
            pms["openid.ns.ax"] = NamespaceUri.AbsoluteUri;
            switch (Mode)
            {
                case AttributeExchangeMode.Fetch:
                    pms[prefix + "mode"] = "fetch_request";
                    List<string> required = new List<string>();
                    List<string> if_available = new List<string>();
                    foreach (AttributeExchangeItem aei in _RequestData)
                    {
                        pms[prefix + "type." + aei.Alias] = aei.IdUri.AbsoluteUri;
                        if (aei.Count > 1)
                        {
                            pms[prefix + "count." + aei.Alias] = aei.Count.ToString(CultureInfo.InvariantCulture);
                        }
                        if (aei.Required == true)
                        {
                            required.Add(aei.Alias);
                        }
                        else
                        {
                            if_available.Add(aei.Alias);
                        }
                    }
                    if (required.Count > 0) { pms[prefix + "required"] = String.Join(",", required.ToArray()); }
                    if (if_available.Count > 0) { pms[prefix + "if_available"] = String.Join(",", if_available.ToArray()); }
                    if (UpdateUrl != null)
                    {
                        pms[prefix + "update_url"] = UpdateUrl.AbsoluteUri;
                    }
                    break;
                case AttributeExchangeMode.Store:
                    pms[prefix + "mode"] = "store_request";
                    foreach (AttributeExchangeItem aei in _RequestData)
                    {
                        pms[prefix + "type." + aei.Alias] = aei.IdUri.AbsoluteUri;
                        if (aei.Values != null)
                        {
                            if (aei.Count == 1)
                            {
                                pms[prefix + "value." + aei.Alias] = aei.Values[0];
                            }
                            else if (aei.Count > 1)
                            {
                                pms[prefix + "count." + aei.Alias] = aei.Count.ToString(CultureInfo.InvariantCulture);
                                for (int i = 0; i < aei.Count; i++)
                                {
                                    pms[prefix + "value." + aei.Alias + "." + i.ToString(CultureInfo.InvariantCulture)] = aei.Values[i];
                                }
                            }
                        }
                    }
                    break;
            }
            return pms;
        }

        /// <summary>
        /// Whether or not the validation completed per this extension.
        /// </summary>
        /// <returns>Always returns true.</returns>
        public bool Validation()
        {
            return true;
        }

        /// <summary>
        /// Gets data for use by OpenIdUser object.
        /// </summary>
        /// <param name="userObject">The OpenIdUser object to populate.</param>
        public void PopulateUserObject(OpenIdUser userObject)
        {
            NameValueCollection Request = Parent.RequestArguments;

            AttributeExchangeItem[] aeis = DecodeUserData(Request);
            foreach (AttributeExchangeItem aei in aeis)
            {
                userObject.ExtensionData.Add(aei.IdUri.AbsoluteUri, String.Join(", ", aei.Values));
            }
        }

        /// <summary>
        /// Decode data returned from a fetch request
        /// </summary>
        /// <param name="arguments">Request data that holds the data to decode.</param>
        /// <returns>An array of AttributeExchangeItem objects.</returns>
        internal static AttributeExchangeItem[] DecodeUserData(NameValueCollection arguments)
        {
            NameValueCollection ds = Utility.GetExtNamespaceAliases(arguments);
            if (ds[ProtocolUri.AttributeExchange1Dot0.AbsoluteUri] == null) { return null; }
            string p = ds[ProtocolUri.AttributeExchange1Dot0.AbsoluteUri];
            string prefix = "openid." + p + ".";

            List<AttributeExchangeItem> list = new List<AttributeExchangeItem>();

            List<string> keys = new List<string>();

            foreach (string k in arguments.Keys)
            {
                keys.Add(k);
            }

            foreach (string s in arguments.Keys)
            {
                if (s != null && s.StartsWith(prefix + "type.", StringComparison.OrdinalIgnoreCase))
                {
                    AttributeExchangeItem aei = new AttributeExchangeItem();

                    // Determine alias
                    aei.Alias = s.Substring((prefix + "type.").Length);

                    // Get URI
                    aei.IdUri = new Uri(arguments[s]);

                    bool useCount = false;

                    // Determine value count
                    if (keys.Contains(prefix + "count." + aei.Alias))
                    {
                        aei.Count = Convert.ToInt32(arguments[prefix + "count." + aei.Alias], CultureInfo.InvariantCulture);
                        useCount = true;
                    }
                    else
                    {
                        aei.Count = 1;
                        useCount = false;
                    }

                    // Get values
                    List<string> values = new List<string>();
                    for (int i = 1; i <= aei.Count; i++)
                    {
                        if (useCount)
                        {
                            values.Add(arguments[prefix + "value." + aei.Alias + "." + i]);
                        }
                        else
                        {
                            values.Add(arguments[prefix + "value." + aei.Alias]);
                        }
                    }
                    aei.Values = values.ToArray();

                    list.Add(aei);
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// Decode data returned from a fetch request
        /// </summary>
        /// <returns>An array of AttributeExchangeItem objects</returns>
        internal AttributeExchangeItem[] DecodeUserData()
        {
            return DecodeUserData(Parent.RequestArguments);
        }

        #endregion

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
