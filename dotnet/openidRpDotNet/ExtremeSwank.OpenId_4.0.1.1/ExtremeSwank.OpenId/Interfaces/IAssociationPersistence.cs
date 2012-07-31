using ExtremeSwank.OpenId.Persistence;

namespace ExtremeSwank.OpenId
{
    /// <summary>
    /// Interface used for Association persistence management objects.
    /// </summary>
    /// <example>
    /// <code>
    /// public sealed class ApplicationAssociationManager : IAssociationPersistence
    /// {
    ///    const string nextAssocCleanup = "OpenID_NextAssocCleanup";
    ///    const string associations = "OpenID_Associations";
    ///
    ///    private DateTime NextCleanup
    ///    {
    ///        get 
    ///        {
    ///            if (HttpContext.Current.Application[nextAssocCleanup] == null) { return DateTime.MaxValue; }
    ///            return (DateTime)HttpContext.Current.Application[nextAssocCleanup]; 
    ///        }
    ///        set { HttpContext.Current.Application[nextAssocCleanup] = value; }
    ///    }
    ///
    ///    private DataTable Associations
    ///    {
    ///        get
    ///        {
    ///            if (HttpContext.Current.Application[associations] == null)
    ///            {
    ///                Init();
    ///            }
    ///            return (DataTable)HttpContext.Current.Application[associations];
    ///        }
    ///        set
    ///        {
    ///            if (HttpContext.Current.Application[associations] == null)
    ///            {
    ///                Init();
    ///            }
    ///            HttpContext.Current.Application[associations] = value;
    ///        }
    ///    }
    ///
    ///    public void Init()
    ///    {
    ///        DataTable dt = new DataTable();
    ///        dt.Columns.Add("protocol", typeof(ProtocolVersion));
    ///        dt.Columns.Add("server", typeof(string));
    ///        dt.Columns.Add("handle", typeof(string));
    ///        dt.Columns.Add("assoc_type", typeof(string));
    ///        dt.Columns.Add("session_type", typeof(string));
    ///        dt.Columns.Add("secret", typeof(byte[]));
    ///        dt.Columns.Add("expiration", typeof(DateTime));
    ///        dt.AcceptChanges();
    ///
    ///        HttpContext.Current.Application[associations] = dt;
    ///    }
    ///
    ///    public void Remove(Association assoc)
    ///    {
    ///        DataRow[] rows = Associations.Select("handle = '" + assoc.Handle + "'");
    ///        foreach (DataRow dr in rows) 
    ///        {
    ///            Associations.Rows.Remove(dr);
    ///        }
    ///        Associations.AcceptChanges();
    ///    }
    ///
    ///    public void Add(Association association)
    ///    {
    ///        // Check for existing association
    ///        DataRow[] result = Associations.Select("server = '" + association.Server + "'");
    ///        if (result.Length > 0)
    ///        {
    ///            for (int i = 0; i &lt; result.Length; i++)
    ///            {
    ///                Associations.Rows.Remove(result[i]);
    ///            }
    ///        }
    ///
    ///        // Add new row
    ///        DataRow dr = Associations.NewRow();
    ///        dr["protocol"] = association.ProtocolVersion;
    ///        dr["server"] = association.Server;
    ///        dr["handle"] = association.Handle;
    ///        dr["assoc_type"] = association.AssociationType;
    ///        dr["session_type"] = association.SessionType;
    ///        dr["secret"] = association.Secret;
    ///        dr["expiration"] = association.Expiration;
    ///        Associations.Rows.Add(dr);
    ///        Associations.AcceptChanges();
    ///    }
    ///
    ///    private Association ToAssociation(DataRow dr)
    ///    {
    ///        Association ar = new Association();
    ///        ar.ProtocolVersion = (ProtocolVersion)dr["protocol"];
    ///        ar.Server = (string)dr["server"];
    ///        ar.Handle = (string)dr["handle"];
    ///        ar.AssociationType = (string)dr["assoc_type"];
    ///        ar.SessionType = (string)dr["session_type"];
    ///        ar.Secret = (byte[])dr["secret"];
    ///        ar.Expiration = (DateTime)dr["expiration"];
    ///        return ar;
    ///    }
    ///
    ///    public Association FindByHandle(string handle)
    ///    {
    ///        DataRow[] result = Associations.Select("handle = '" + handle + "'");
    ///        if (result.Length > 0)
    ///        {
    ///            return ToAssociation(result[0]);
    ///        }
    ///        return null;
    ///    }
    ///
    ///    public Association FindByServer(string server)
    ///    {
    ///        DataRow[] result = Associations.Select("server = '" + server + "'");
    ///        if (result.Length > 0)
    ///        {
    ///            return ToAssociation(result[0]);
    ///        }
    ///        return null;
    ///    }
    ///
    ///    public void Cleanup()
    ///    {
    ///        if (NextCleanup == null || NextCleanup &lt; DateTime.UtcNow)
    ///        {
    ///            foreach (DataRow dr in Associations.Rows)
    ///            {
    ///                if ((DateTime)dr["expiration"] &lt; DateTime.UtcNow)
    ///                {
    ///                    dr.Delete();
    ///                }
    ///            }
    ///            Associations.AcceptChanges();
    ///            NextCleanup = DateTime.UtcNow.AddMinutes(10);
    ///        }
    ///    }
    /// }
    /// </code>
    /// </example>
    public interface IAssociationPersistence
    {
        /// <summary>
        /// Removes an association entry from persistence.
        /// </summary>
        /// <param name="assoc">The association entry to remove.</param>
        void Remove(Association assoc);
        /// <summary>
        /// Adds an assocation entry to persistence.
        /// </summary>
        /// <param name="association">The Association to store.</param>
        void Add(Association association);
        /// <summary>
        /// Retrieve an association entry by its handle.
        /// </summary>
        /// <param name="handle">The association handle.</param>
        /// <returns>An Association object representing the stored association.</returns>
        Association FindByHandle(string handle);
        /// <summary>
        /// Retrieve an association entry by the OpenID Provider Server URL
        /// </summary>
        /// <param name="server">The server URL.</param>
        /// <returns>An Association object representing the stored association.</returns>
        Association FindByServer(string server);
        /// <summary>
        /// Remove expired association entries from persistence.
        /// </summary>
        void Cleanup();
    }
}
