using System;
using System.Data;
using System.Globalization;

namespace ExtremeSwank.OpenId.Persistence
{
    /// <summary>
    /// Manages associations with OpenID Providers, can use any database driver
    /// implementing the IDbConnection interface.
    /// </summary>
    public class DBAssociationManager : IAssociationPersistence
    {
        private IDbConnection _conn;
        private string _tableName;
        private string _globalTableName;
        private string _tablePrefix;

        /// <summary>
        /// The database connection object.
        /// </summary>
        protected IDbConnection DatabaseConnection
        {
            get { return _conn; }
            set { _conn = value; }
        }

        /// <summary>
        /// The name of the associations table.  Set automatically by the _tablePrefix property.
        /// </summary>
        protected string TableName
        {
            get { return _tableName; }
        }

        /// <summary>
        /// Sets the prefix for the table names.
        /// </summary>
        protected string TablePrefix
        {
            get { return _tablePrefix; }
            set 
            {
                _tablePrefix = value;
                _tableName = value + "ConsumerAssociations";
                _globalTableName = value + "ConsumerGlobals";
            }
        }

        /// <summary>
        /// Creates a new OdbcAssociationManager instance.
        /// </summary>
        /// <param name="connection">IDbConnection object providing connection to the database.</param>
        /// <param name="tableNamePrefix">Table name prefix.</param>
        public DBAssociationManager(IDbConnection connection, string tableNamePrefix)
        {
            DatabaseConnection = connection;
            TablePrefix = tableNamePrefix;
        }

        /// <summary>
        /// Creates a new OdbcAssociationManager instance.
        /// Should only be used by inheriting classes.
        /// </summary>
        public DBAssociationManager()
        {
        }

        #region IAssociationPersistence Members

        /// <summary>
        /// Removes an association from the database.
        /// </summary>
        /// <param name="assoc">Association to remove.</param>
        public void Remove(Association assoc)
        {
            string sql = "DELETE FROM " + _tableName + " WHERE Handle = '" + FixSqlIn(assoc.Handle) + "'";
            _conn.Open();
            IDbCommand comm = _conn.CreateCommand();
            comm.CommandText = sql;
            comm.ExecuteNonQuery();
            _conn.Close();
        }

        /// <summary>
        /// Add an assocation to the database.
        /// </summary>
        /// <param name="association">Association to add.</param>
        public void Add(Association association)
        {
            string protoversion = "";
            switch (association.ProtocolVersion)
            {
                case ProtocolVersion.V1Dot1:
                    protoversion = "V1_1";
                    break;
                case ProtocolVersion.V2Dot0:
                    protoversion = "V2_0";
                    break;
            }

            string sql = "INSERT INTO " + _tableName + " (Handle, Server, AssociationType, "
                + "SessionType, ProtocolVersion, Secret, Expiration) "
                + "VALUES ('" + FixSqlIn(association.Handle) + "', "
                + "'" + association.Server + "', "
                + "'" + association.AssociationType + "', "
                + "'" + association.SessionType + "', "
                + "'" + protoversion + "', "
                + "'" + Convert.ToBase64String(association.Secret) + "', " 
                + "'" + association.Expiration.ToString("s", CultureInfo.InvariantCulture) + "') ";
            _conn.Open();
            IDbCommand comm = _conn.CreateCommand();
            comm.CommandText = sql;
            comm.ExecuteNonQuery();
            _conn.Close();
        }

        /// <summary>
        /// Locate an association by its handle.
        /// </summary>
        /// <param name="handle">The association's handle.</param>
        /// <returns>A populated Association object, or null if not found.</returns>
        public Association FindByHandle(string handle)
        {
            string sql = "SELECT Handle, Server, AssociationType, "
                + "SessionType, ProtocolVersion, Secret, Expiration "
                + "FROM " + _tableName + " "
                + "WHERE Handle = '" + FixSqlIn(handle) + "' ";
            _conn.Open();
            IDbCommand comm = _conn.CreateCommand();
            comm.CommandText = sql;
            IDataReader dr = comm.ExecuteReader();
            if (dr.Read() == false) 
            {
                _conn.Close();
                return null;
            }

            bool expiredFound = false;

            while (((DateTime)dr["Expiration"]) < DateTime.UtcNow)
            {
                dr.Read();
                expiredFound = true;
            }

            if (expiredFound)
            {
                Cleanup();
            }

            Association ar = ToAssociation(dr);
            _conn.Close();
            return ar;
        }

        /// <summary>
        /// Locate an association by its OpenID Provider URL.
        /// </summary>
        /// <param name="server">The OpenID Provider URL.</param>
        /// <returns>A populated Association object, or null if not found.</returns>
        public Association FindByServer(string server)
        {
            string sql = "SELECT Handle, Server, AssociationType, "
                + "SessionType, ProtocolVersion, Secret, Expiration "
                + "FROM " + _tableName + " "
                + "WHERE Server = '" + server + "' ";
            _conn.Open();
            IDbCommand comm = _conn.CreateCommand();
            comm.CommandText = sql;
            IDataReader dr = comm.ExecuteReader();
            if (dr.Read() == false) 
            {
                _conn.Close();
                return null; 
            }

            bool expiredFound = false;

            while (((DateTime)dr["Expiration"]) < DateTime.UtcNow)
            {
                dr.Read();
                expiredFound = true;
            }

            if (expiredFound)
            {
                Cleanup();
            }

            Association ar = ToAssociation(dr);

            _conn.Close();
            return ar;
        }

        /// <summary>
        /// Transforms the current record in a DataReader object into an Association object.
        /// </summary>
        /// <param name="dr">The DataReader object use.</param>
        /// <returns>A populated Association object.</returns>
        protected static Association ToAssociation(IDataRecord dr)
        {
            Association ar = new Association();
            switch ((string)dr["ProtocolVersion"])
            {
                case "V2_0":
                    ar.ProtocolVersion = ProtocolVersion.V2Dot0;
                    break;
                case "V1_1":
                    ar.ProtocolVersion = ProtocolVersion.V1Dot1;
                    break;
            }
            ar.Server = (string)dr["Server"];
            ar.Handle = FixSqlOut((string)dr["Handle"]);
            ar.AssociationType = (string)dr["AssociationType"];
            ar.SessionType = (string)dr["SessionType"];
            ar.Secret = Convert.FromBase64String((string)dr["Secret"]);
            ar.Expiration = ((DateTime)dr["Expiration"]);
            return ar;
        }

        /// <summary>
        /// Remove all expired associations from the database.
        /// </summary>
        public void Cleanup()
        {
            string sql = "SELECT LastAssociationCleanup FROM " + _globalTableName + " ";
            IDbCommand comm = _conn.CreateCommand();
            comm.CommandText = sql;
            _conn.Open();
            IDataReader dr = comm.ExecuteReader();
            if (dr.Read())
            {
                if (((DateTime)dr["LastAssociationCleanup"]) < DateTime.UtcNow.AddMinutes(-10))
                {
                    sql = "DELETE FROM " + _tableName + " WHERE "
                        + "Expiration < '" + DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture) + "' ";
                    dr.Close();
                    comm = _conn.CreateCommand();
                    comm.CommandText = sql;
                    comm.ExecuteNonQuery();
                }
            }
            else
            {
                sql = "INSERT INTO " + _globalTableName + " (LastAssociationCleanup) "
                    + "VALUES ('" + DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture) + "') ";
                dr.Close();
                comm = _conn.CreateCommand();
                comm.CommandText = sql;
                comm.ExecuteNonQuery();
            }
            _conn.Close();
        }

        #endregion

        /// <summary>
        /// Create the association table in the database.
        /// </summary>
        public void BuildDB()
        {
            string sql = "CREATE TABLE " + _tableName + " ("
                + "Handle NVARCHAR(255), "
                + "Server NVARCHAR(255), "
                + "ProtocolVersion NVARCHAR(5), "
                + "AssociationType NVARCHAR(255), "
                + "SessionType NVARCHAR(255), "
                + "Secret NVARCHAR(255), "
                + "Expiration DATETIME)";
            string globals = "CREATE TABLE " + _globalTableName + " ("
                + "LastAssociationCleanup DATETIME)";
            string indexstr = "CREATE UNIQUE INDEX " + _tableName + "_Handle "
                + "ON " + _tableName + " (Handle) ";
            string indexstr2 = "CREATE INDEX " + _tableName + "_Server "
                + "ON " + _tableName + " (Server) ";
            string[] sqls = { sql, globals, indexstr, indexstr2 };
            _conn.Open();
            foreach (string s in sqls)
            {
                IDbCommand comm = _conn.CreateCommand();
                comm.CommandText = s;
                comm.ExecuteNonQuery();
            }
            _conn.Close();
        }

        /// <summary>
        /// Ensures a string does not contain characters that could
        /// prematurely terminate an SQL statement.
        /// </summary>
        /// <param name="sql">The string to fix.</param>
        /// <returns>A SQL-compatible string.</returns>
        protected static string FixSqlIn(string sql)
        {
            return sql.Replace("'", "&apos;");
        }
        /// <summary>
        /// Converts a SQL-compatible value string into its original
        /// value.
        /// </summary>
        /// <param name="sql">The value string to fix.</param>
        /// <returns>The original string value.</returns>
        protected static string FixSqlOut(string sql)
        {
            return sql.Replace("&apos;", "'");
        }
    }
}
