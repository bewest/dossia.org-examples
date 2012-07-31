using System;
using System.Collections.Generic;
using ExtremeSwank.OpenId.PlugIns.Discovery;
using System.Collections.Specialized;

namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Contains all information received about the authenticated user.
    /// </summary>
    [Serializable]
    public class OpenIdUser
    {
        DiscoveryResult _LastDiscoveryResult;

        /// <summary>
        /// Get the DiscoveryResult object generated during the last
        /// OpenID discovery.
        /// </summary>
        public DiscoveryResult LastDiscoveryResult
        {
            get { return _LastDiscoveryResult; }
        }

        private string _Identity;
        /// <summary>
        /// Gets or sets the claimed identifier.
        /// </summary>
        public string Identity
        {
            get { return _Identity; }
            set { _Identity = value; }
        }

        private string _BaseIdentity;
        /// <summary>
        /// Gets or sets the identifier validated by the Identity Provider.
        /// </summary>
        public string BaseIdentity
        {
            get { return _BaseIdentity; }
            set { _BaseIdentity = value; }
        }

        private NameValueCollection _ExtensionData = new NameValueCollection();

        /// <summary>
        /// Data returned by all loaded Extensions.
        /// </summary>
        public NameValueCollection ExtensionData
        {
            get { return _ExtensionData; }
        }

        /// <summary>
        /// Retrieves extension data.
        /// </summary>
        /// <param name="key">Key of value to get</param>
        /// <returns>String containing value, or null if the value is not present</returns>
        public string GetValue(string key)
        {
            if (ExtensionData[key] != null)
            {
                return ExtensionData[key];
            }
            return null;
        }

        /// <summary>
        /// Retrieves extension data
        /// </summary>
        /// <param name="key">Object, that when <see cref="Object.ToString()"/> is run against it, produces the key of the value to get</param>
        /// <returns>String containing requested value, or null if the value is not present</returns>
        public string GetValue(object key)
        {
            if (ExtensionData[key.ToString()] != null) 
            {
                return ExtensionData[key.ToString()];
            }
            return null;
        }

        /// <summary>
        /// Returns a new <see cref="OpenIdUser"/> object with a pre-set claimed identity.
        /// </summary>
        /// <param name="discResult">The DiscoveryResult object created from the previous discovery process.</param>
        public OpenIdUser(DiscoveryResult discResult)
        {
            _LastDiscoveryResult = discResult;
        }

        /// <summary>
        /// Fill the object with information from received response arguments.
        /// </summary>
        /// <param name="client"><see cref="ClientCore"/> object from which to retrieve the data.</param>
        public void Retrieve(ClientCore client)
        {
            foreach (IExtension e in client.StateContainer.ExtensionPlugIns)
            {
                e.PopulateUserObject(this);
            }
        }
    }

}
