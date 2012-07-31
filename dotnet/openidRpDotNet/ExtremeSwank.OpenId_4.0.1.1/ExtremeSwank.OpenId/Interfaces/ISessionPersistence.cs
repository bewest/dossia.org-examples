
namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Interface used by all objects implementing per-user session persistence.
    /// </summary>
    /// <example>
    /// <para>
    /// Implementing the ISessionPersistence interface just requires implementing
    /// the Cnonce property.  When the property is set, it must be persisted immediately.
    /// </para>
    /// <para>
    /// The following code persists to the ASP.NET Session object.
    /// </para>
    /// <code>
    /// public class SessionSessionManager : ISessionPersistence
    /// {
    ///     const string cnoncestr = "AllowLogin";
    ///     
    ///     #region ISessionPersistence Members
    ///     
    ///     public int Cnonce
    ///     {
    ///         get
    ///         {
    ///             if (HttpContext.Current.Session[cnoncestr] == null)
    ///             {
    ///                 return -1;
    ///             }
    ///             return (int)HttpContext.Current.Session[cnoncestr];
    ///         }
    ///         set { HttpContext.Current.Session[cnoncestr] = value; }
    ///     }
    ///     
    ///     #endregion
    /// }
    /// </code>
    /// </example>
    public interface ISessionPersistence
    {
        /// <summary>
        /// Gets or sets the nonce value used to protect against replay attacks.
        /// </summary>
        int Nonce { get; set; }
    }
}
