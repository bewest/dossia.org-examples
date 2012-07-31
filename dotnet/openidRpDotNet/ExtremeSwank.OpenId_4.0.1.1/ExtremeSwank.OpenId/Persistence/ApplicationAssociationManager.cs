using System;
using System.Data;
using System.Web;
using System.Globalization;

namespace ExtremeSwank.OpenId.Persistence
{
    /// <summary>
    /// Manages associations with OpenID Providers, using the ASP.NET Application object for persistence.
    /// </summary>
    public sealed class ApplicationAssociationManager : IAssociationPersistence
    {
        const string nextAssocCleanup = "OpenID_NextAssocCleanup";
        const string associations = "OpenID_Associations";

        private static DateTime NextCleanup
        {
            get 
            {
                if (HttpContext.Current.Application[nextAssocCleanup] == null) { return DateTime.MaxValue; }
                return (DateTime)HttpContext.Current.Application[nextAssocCleanup]; 
            }
            set { HttpContext.Current.Application[nextAssocCleanup] = value; }
        }

        private static DataTable Associations
        {
            get
            {
                if (HttpContext.Current.Application[associations] == null)
                {
                    Init();
                }
                return (DataTable)HttpContext.Current.Application[associations];
            }
        }
        /// <summary>
        /// Initializes the in-memory associations table.
        /// </summary>
        public static void Init()
        {
            DataTable dt = new DataTable();
            dt.Locale = CultureInfo.InvariantCulture;
            dt.Columns.Add("protocol", typeof(ProtocolVersion));
            dt.Columns.Add("server", typeof(string));
            dt.Columns.Add("handle", typeof(string));
            dt.Columns.Add("assoc_type", typeof(string));
            dt.Columns.Add("session_type", typeof(string));
            dt.Columns.Add("secret", typeof(byte[]));
            dt.Columns.Add("expiration", typeof(DateTime));
            dt.AcceptChanges();

            HttpContext.Current.Application[associations] = dt;
        }
        /// <summary>
        /// Removes an association from the associations table.
        /// </summary>
        /// <param name="assoc">Association to remove from persistence.</param>
        public void Remove(Association assoc)
        {
            DataRow[] rows = Associations.Select("handle = '" + assoc.Handle + "'");
            foreach (DataRow dr in rows) 
            {
                Associations.Rows.Remove(dr);
            }
            Associations.AcceptChanges();
        }
        /// <summary>
        /// Adds a new association to the associations table.
        /// </summary>
        /// <param name="association">The association entry to store.</param>
        public void Add(Association association)
        {
            // Check for existing association
            DataRow[] result = Associations.Select("server = '" + association.Server + "'");
            if (result.Length > 0)
            {
                for (int i = 0; i < result.Length; i++)
                {
                    Associations.Rows.Remove(result[i]);
                }
            }

            // Add new row
            DataRow dr = Associations.NewRow();
            dr["protocol"] = association.ProtocolVersion;
            dr["server"] = association.Server;
            dr["handle"] = association.Handle;
            dr["assoc_type"] = association.AssociationType;
            dr["session_type"] = association.SessionType;
            dr["secret"] = association.Secret;
            dr["expiration"] = association.Expiration;
            Associations.Rows.Add(dr);
            Associations.AcceptChanges();
        }
        /// <summary>
        /// Translates a DataRow matching the correct schema
        /// into an Association object.
        /// </summary>
        /// <param name="dr">DataRow containing source data.</param>
        /// <returns>Populated Association object.</returns>
        private static Association ToAssociation(DataRow dr)
        {
            Association ar = new Association();
            ar.ProtocolVersion = (ProtocolVersion)dr["protocol"];
            ar.Server = (string)dr["server"];
            ar.Handle = (string)dr["handle"];
            ar.AssociationType = (string)dr["assoc_type"];
            ar.SessionType = (string)dr["session_type"];
            ar.Secret = (byte[])dr["secret"];
            ar.Expiration = (DateTime)dr["expiration"];
            return ar;
        }

        /// <summary>
        /// Find an association by its handle.
        /// </summary>
        /// <param name="handle">The association handle.</param>
        /// <returns>A DataRow containing the association entry.</returns>
        public Association FindByHandle(string handle)
        {
            DataRow[] result = Associations.Select("handle = '" + handle + "'");
            if (result.Length > 0)
            {
                return ToAssociation(result[0]);
            }
            return null;
        }
        /// <summary>
        /// Find an association by server name.
        /// </summary>
        /// <param name="server">The OpenID server endpoint URL</param>
        /// <returns>An Association object containing the association data</returns>
        public Association FindByServer(string server)
        {
            DataRow[] result = Associations.Select("server = '" + server + "'");
            if (result.Length > 0)
            {
                return ToAssociation(result[0]);
            }
            return null;
        }
        /// <summary>
        /// Removes expired associations from the associations table
        /// </summary>
        public void Cleanup()
        {
            if (NextCleanup == null || NextCleanup < DateTime.UtcNow)
            {
                foreach (DataRow dr in Associations.Rows)
                {
                    if ((DateTime)dr["expiration"] < DateTime.UtcNow)
                    {
                        dr.Delete();
                    }
                }
                Associations.AcceptChanges();
                NextCleanup = DateTime.UtcNow.AddMinutes(10);
            }
        }
    }
}
