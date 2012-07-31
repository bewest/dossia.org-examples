
namespace ExtremeSwank.OpenId.Persistence
{
    /// <summary>
    /// A volatile in-memory SessionManager.
    /// </summary>
    /// <remarks>
    /// Only holds data for a single user session.  Data is not persisted.
    /// </remarks>
    public sealed class SingularSessionManager : ISessionPersistence
    {
        private int _Cnonce;

        #region ISessionPersistence Members

        /// <summary>
        /// Gets or sets the nonce value used to protect against replay attacks.
        /// </summary>
        public int Nonce
        {
            get
            {
                if (_Cnonce == default(int)) { return -1; }
                return _Cnonce;
            }
            set
            {
                _Cnonce = value;
            }
        }

        #endregion

    }
}
