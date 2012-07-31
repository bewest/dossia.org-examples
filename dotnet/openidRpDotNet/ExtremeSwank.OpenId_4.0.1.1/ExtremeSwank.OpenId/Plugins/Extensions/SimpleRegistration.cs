using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using ExtremeSwank.OpenId.PlugIns.Discovery;

namespace ExtremeSwank.OpenId.PlugIns.Extensions
{
    /// <summary>
    /// Provides support for the Simple Registration extension.
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
    /// SimpleRegistration sr = new AttributeExchange(client);
    /// 
    /// // Add required items
    /// sr.AddRequiredFields(SimpleRegistrationFields.Email,
    ///                      SimpleRegistrationFields.FullName);
    /// 
    /// // Add optional items
    /// sr.AddOptionalFields(SimpleRegistrationFields.Gender,
    ///                      SimpleRegistrationFields.DateOfBirth);
    /// 
    /// client.CreateRequest();
    /// </code>
    /// When receiving the response from the OpenID Provider:
    /// <code>
    /// OpenIdUser user = client.RetrieveUser();
    /// string email = user.GetValue(SimpleRegistrationFields.Email);
    /// string fullName = user.GetValue(SimpleRegistrationFields.FullName);
    /// string gender = user.GetValue(SimpleRegistrationFields.Gender);
    /// string dateOfBirth = user.GetValue(SimpleRegistrationFields.DateOfBirth);
    /// </code>
    /// </example>
    [Serializable]
    public class SimpleRegistration : IExtension
    {
        private const string _Name = "OpenID Simple Registration Extension 1.1";
        private static readonly Uri _Namespace = ProtocolUri.SimpleRegistration1Dot1;
        private const string _Prefix = "openid.sreg.";
        private List<string> _OptionalFields;
        private List<string> _RequiredFields;
        private Uri _PolicyUrl;
        private StateContainer _Parent;

        /// <summary>
        /// Gets the name of the extension.
        /// </summary>
        public string Name
        {
            get
            {
                return _Name;
            }
        }
        /// <summary>
        /// Gets the extension's registered namespace.
        /// </summary>
        public Uri NamespaceUri
        {
            get { return _Namespace; }
        }
        /// <summary>
        /// Gets or sets the parent <see cref="StateContainer"/> object.
        /// </summary>
        public StateContainer Parent
        {
            get { return _Parent; }
        }

        /// <summary>
        /// Comma-delimited list of optional fields to retrieve from Identity Provider.
        /// Valid values are: nickname, email, fullname, dob, gender, postcode,
        /// country, language, timezone
        /// </summary>
        /// <example>
        /// <code>
        /// OpenIdClient openid;
        /// SimpleRegistration sr = new SimpleRegistration(openid);
        /// sr.OptionalFields = "nickname,fullname,postcode";
        /// </code>
        /// </example>
        public string OptionalFields
        {
            get
            {
                return String.Join(",", _OptionalFields.ToArray());
            }
            set 
            {
                _OptionalFields = new List<string>(value.Split(','));
            }
        }
        /// <summary>
        /// Comma-delimited list of required fields to retrieve from Identity Provider.
        /// Valid values are: nickname, email, fullname, dob, gender, postcode,
        /// country, language, timezone
        /// </summary>
        /// <example>
        /// <code>
        /// OpenIdClient openid;
        /// SimpleRegistration sr = new SimpleRegistration(openid);
        /// sr.RequiredFields = "nickname,fullname,postcode";
        /// </code>
        /// </example>
        public string RequiredFields
        {
            get
            {
                return String.Join(",", _RequiredFields.ToArray());
            }
            set 
            {
                _RequiredFields = new List<string>(value.Split(','));
            }
        }
        
        /// <summary>
        /// Gets or sets the full URL to the privacy policy that will be sent to the
        /// OpenID Provider in the request.
        /// </summary>
        public Uri PolicyUrl 
        {
            get
            {
                return _PolicyUrl;
            }
            set
            {
                _PolicyUrl = value;
            }
        }

        /// <summary>
        /// Dictionary&lt;string, string&gt; containing key-value pairs that will be passed
        /// during initial authentication request to Identity Provider.
        /// </summary>
        /// <param name="discResult">The DiscoveryResult object to use.</param>
        public NameValueCollection BuildAuthorizationData(DiscoveryResult discResult)
        {
            NameValueCollection pms = new NameValueCollection();
            int fieldcount = _OptionalFields.Count + _RequiredFields.Count;
            if (PolicyUrl != null)
            {
                fieldcount++;
            }
            if (fieldcount > 0)
            {
                if (discResult.AuthVersion == ProtocolVersion.V2Dot0)
                {
                    pms["openid.ns.sreg"] = _Namespace.AbsoluteUri;
                }
                if (RequiredFields != null)
                {
                    pms[_Prefix + "required"] = RequiredFields;
                }
                if (OptionalFields != null)
                {
                    pms[_Prefix + "optional"] = OptionalFields;
                }
                if (PolicyUrl != null)
                {
                    pms[_Prefix + "policy_url"] = PolicyUrl.AbsoluteUri;
                }
            }
            return pms;
        }
        /// <summary>
        /// Performs extension-specific validation functions once authentication response has been received.
        /// </summary>
        /// <returns>Returns boolean value, true if validation is successful, false if not.</returns>
        public bool Validation()
        {
            return true;
        }
        /// <summary>
        /// Gets the user object data needed to populate an <see cref="OpenIdUser"/> object.
        /// </summary>
        /// <remarks>
        /// Populates the OpenIdUser.ExtensionData property.
        /// </remarks>
        /// <param name="userObject">The OpenIdUser object to populate.</param>
        public void PopulateUserObject(OpenIdUser userObject)
        {
            NameValueCollection Request = Parent.RequestArguments;
            foreach (string key in Request.Keys)
            {
                if (key != null && key.StartsWith(_Prefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (Request[key] != null)
                    {
                        userObject.ExtensionData[key] = Request[key];
                    }
                }
            }
        }

        /// <summary>
        /// Add optional fields using members of the <see cref="SimpleRegistrationFields"/> class.
        /// </summary>
        /// <example>
        /// <code>
        /// OpenIdClient openid;
        /// SimpleRegistration sr = new SimpleRegistration(openid);
        /// sr.AddOptionalFields(Fields.Nickname, Fields.Email, Fields.PostalCode);
        /// </code>
        /// </example>
        /// <param name="fields">A list of parameters from the Fields class.</param>
        public void AddOptionalFields(params string[] fields)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = fields[i].Replace("openid.sreg.", "");
                if (!_OptionalFields.Contains(fields[i]))
                {
                    _OptionalFields.Add(fields[i]);
                }
            }
        }

        /// <summary>
        /// Add required fields using members of the <see cref="SimpleRegistrationFields"/> class.
        /// </summary>
        /// <example>
        /// <code>
        /// OpenIdClient openid;
        /// SimpleRegistration sr = new SimpleRegistration(openid);
        /// sr.AddRequiredFields(Fields.Nickname, Fields.Email, Fields.PostalCode);
        /// </code>
        /// </example>
        /// <param name="fields">A list of parameters from the Fields class.</param>
        public void AddRequiredFields(params string[] fields)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = fields[i].Replace("openid.sreg.", "");
                if (!_RequiredFields.Contains(fields[i]))
                {
                    _RequiredFields.Add(fields[i]);
                }
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="SimpleRegistration"/>.
        /// </summary>
        /// <param name="state"><see cref="StateContainer"/> object to attach.</param>
        public SimpleRegistration(StateContainer state)
        {
            _RequiredFields = new List<string>();
            _OptionalFields = new List<string>();
            _Parent = state;
            _Parent.RegisterPlugIn(this);
        }

        /// <summary>
        /// Creates a new SimpleRegistration plugin and registers it with an <see cref="ClientCore"/> object.
        /// </summary>
        /// <param name="client">The <see cref="ClientCore"/> object to attach.</param>
        public SimpleRegistration(ClientCore client)
        {
            _RequiredFields = new List<string>();
            _OptionalFields = new List<string>();
            _Parent = client.StateContainer;
            _Parent.RegisterPlugIn(this);
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
