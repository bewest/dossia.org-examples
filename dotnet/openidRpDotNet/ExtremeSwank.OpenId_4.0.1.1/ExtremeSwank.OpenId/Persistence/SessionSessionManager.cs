using System.Web;

namespace ExtremeSwank.OpenId.Persistence
{
    /// <summary>
    /// Provides session persistence to OpenID functions using the ASP.NET Session object.
    /// </summary>
    public class SessionSessionManager : ISessionPersistence
    {
        const string cnoncestr = "AllowLogin";

        #region ISessionPersistence Members

        /// <summary>
        /// Gets or sets the nonce value used to protect against replay attacks.
        /// </summary>
        public int Nonce
        {
            get
            {
                if (HttpContext.Current.Session[cnoncestr] == null)
                {
                    return -1;
                }
                return (int)HttpContext.Current.Session[cnoncestr];
            }
            set { HttpContext.Current.Session[cnoncestr] = value; }
        }

        #endregion
    }
}
