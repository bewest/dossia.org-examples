using System.Data.Odbc;

namespace ExtremeSwank.OpenId.Persistence
{
    /// <summary>
    /// Manages associations with OpenID Providers, uses ODBC for persistence.
    /// </summary>
    public sealed class OdbcAssociationManager : DBAssociationManager
    {
        /// <summary>
        /// Creates a new OdbcAssociationManager instance.
        /// </summary>
        /// <param name="connectionString">ODBC connection string.</param>
        /// <param name="tableNamePrefix">Table name prefix.</param>
        public OdbcAssociationManager(string connectionString, string tableNamePrefix)
        {
            DatabaseConnection = new OdbcConnection(connectionString);
            TablePrefix = tableNamePrefix;
        }
    }
}
