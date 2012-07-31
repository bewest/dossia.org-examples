using System.Data.SqlClient;

namespace ExtremeSwank.OpenId.Persistence
{
    /// <summary>
    /// Manages associations with OpenID Providers, uses SQL Server for persistence.
    /// </summary>
    public sealed class SqlAssociationManager : DBAssociationManager
    {
        /// <summary>
        /// Creates a new SqlAssociationManager instance.
        /// </summary>
        /// <param name="connectionString">SQL Server connection string.</param>
        /// <param name="tableNamePrefix">Table name prefix.</param>
        public SqlAssociationManager(string connectionString, string tableNamePrefix)
        {
            DatabaseConnection = new SqlConnection(connectionString);
            TablePrefix = tableNamePrefix;
        }
    }
}
