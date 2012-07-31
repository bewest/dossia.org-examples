using System;
using System.Data;
using System.Globalization;

namespace ExtremeSwank.OpenId.Persistence
{
    /// <summary>
    /// Manages data needed on a per-user basis for Stateful authentication.
    /// Can use any database driver implementing the IDbConnection interface.
    /// </summary>
    public class DBSessionManager : ISessionPersistence
    {
        private string _sessionID;
        private IDbConnection _conn;
        private string _tableName;
        private string _tablePrefix;

        /// <summary>
        /// The unique identifier for this session.
        /// </summary>
        protected string SessionId
        {
            get { return _sessionID; }
            set { _sessionID = value; }
        }

        /// <summary>
        /// The database connection object.
        /// </summary>
        protected IDbConnection DatabaseConnection
        {
            get { return _conn; }
            set { _conn = value; }
        }

        /// <summary>
        /// The name of the sessions table.  Set automatically by the _tablePrefix property.
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
                _tableName = value + "ConsumerSessionData";
                _tablePrefix = value;
            }
        }

        /// <summary>
        /// Creates a new DbSessionManager instance.
        /// </summary>
        /// <param name="connection">IDbConnection object providing connection to the database.</param>
        /// <param name="tablePrefix">Table name prefix.</param>
        /// <param name="sessionId">Associated session ID.</param>
        public DBSessionManager(IDbConnection connection, string tablePrefix, string sessionId)
        {
            TablePrefix = tablePrefix;
            DatabaseConnection = connection;
            SessionId = sessionId;
        }

        /// <summary>
        /// Creates a new DbSessionManager instance.
        /// Should only be used by inheriting classes.
        /// </summary>
        public DBSessionManager()
        {
        }

        /// <summary>
        /// Create the session table in the database.
        /// </summary>
        public void BuildDB()
        {
            string sqlstr = "CREATE TABLE " + _tableName + " ("
                + "SessionID NVARCHAR(255), "
                + "Cnonce NVARCHAR(255), "
                + "LastUpdate DATETIME) ";
            string indexstr = "CREATE UNIQUE INDEX " + _tableName + "_SessionID "
                + "ON " + _tableName + " (SessionID) ";
            string[] sqls = { sqlstr, indexstr };
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

        #region ISessionPersistence Members

        /// <summary>
        /// Gets or sets the nonce value used to protect against replay attacks.
        /// </summary>
        public int Nonce
        {
            get
            {
                _conn.Open();
                string setting = GetCurrentSetting();
                _conn.Close();
                if (setting == "NOTPRESENT" || setting == null)
                {
                    return -1;
                }
                return Convert.ToInt32(setting, CultureInfo.InvariantCulture);
            }
            set
            {
                _conn.Open();
                string setstr = "";

                if (value == -1)
                {
                    setstr = "DELETE FROM " + _tableName + " WHERE SessionID = '" + _sessionID + "'";
                }
                else
                {
                    string setting = GetCurrentSetting();

                    if (setting == "NOTPRESENT")
                    {
                        setstr = "INSERT INTO " + _tableName + " (SessionID, Cnonce, LastUpdate) VALUES ('" + _sessionID + "', '" + value + "', '" + DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture) + "')";
                    }
                    else
                    {
                        setstr = "UPDATE " + _tableName + " SET Cnonce = '" + value + "' WHERE SessionID = '" + _sessionID + "'";
                    }
                }
                IDbCommand comm = _conn.CreateCommand();
                comm.CommandText = setstr;
                comm.ExecuteNonQuery();
                _conn.Close();
            }
        }

        #endregion

        string GetCurrentSetting()
        {
            string delstr = "DELETE FROM " + _tableName + " WHERE LastUpdate < '" + DateTime.UtcNow.AddMinutes(-5).ToString("s", CultureInfo.InvariantCulture) + "'";
            IDbCommand comm = _conn.CreateCommand();
            comm.CommandText = delstr;
            comm.ExecuteNonQuery();

            string getstr = "SELECT Cnonce FROM " + _tableName + " WHERE SessionID = '" + _sessionID + "'";
            comm = _conn.CreateCommand();
            comm.CommandText = getstr;
            IDataReader dr = comm.ExecuteReader();

            string result = null;
            if (dr.Read())
            {
                if (dr["Cnonce"] == DBNull.Value) { result = null; }
                else { result = (string)dr["Cnonce"]; }
            }
            else
            {
                result = "NOTPRESENT";
            }
            dr.Close();
            return result;
        }
    }
}
