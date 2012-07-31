using System;

namespace ExtremeSwank.OpenId.Persistence
{
    /// <summary>
    /// Contains information for a specific Association between the Client
    /// and an OpenID Provider.
    /// </summary>
    public class Association
    {
        ProtocolVersion _ProtocolVersion;
        string _Server;
        string _Handle;
        string _AssociationType;
        string _SessionType;
        byte[] _Secret;
        DateTime _Expiration;

        /// <summary>
        /// Gets or sets the version of the OpenID Protocol used to create
        /// the association.
        /// </summary>
        public ProtocolVersion ProtocolVersion
        {
            get { return _ProtocolVersion; }
            set { _ProtocolVersion = value; }
        }
        /// <summary>
        /// Gets or sets the OpenID Provider URL.
        /// </summary>
        public string Server
        {
            get { return _Server; }
            set { _Server = value; }
        }
        /// <summary>
        /// Gets or sets the association handle.
        /// </summary>
        public string Handle
        {
            get { return _Handle; }
            set { _Handle = value; }
        }
        /// <summary>
        /// Gets or sets the association type.
        /// </summary>
        public string AssociationType
        {
            get { return _AssociationType; }
            set { _AssociationType = value; }
        }
        /// <summary>
        /// Gets or sets the session type.
        /// </summary>
        public string SessionType
        {
            get { return _SessionType; }
            set { _SessionType = value; }
        }
        /// <summary>
        /// Gets or sets the negotiated secret.
        /// </summary>
        public byte[] Secret
        {
            get { return _Secret; }
            set { _Secret = value; }
        }
        /// <summary>
        /// Gets or sets the time when this association will be expired.
        /// </summary>
        public DateTime Expiration
        {
            get { return _Expiration; }
            set { _Expiration = value; }
        }
    }
}
