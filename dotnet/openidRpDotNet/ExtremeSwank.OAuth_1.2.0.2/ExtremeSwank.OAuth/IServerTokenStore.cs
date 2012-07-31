// Copyright (c) 2009 John Ehn, ExtremeSwank.com
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Web;

namespace ExtremeSwank.OAuth
{
    /// <summary>
    /// Interface for server-side token and Consumer registration storage.
    /// </summary>
    public interface IServerTokenStore
    {
        /// <summary>
        /// Get a Consumer Registration entry from storage.
        /// </summary>
        /// <param name="key">The key for the entry.</param>
        /// <returns>A populated ConsumerRegistration object, or null if it cannot be found.</returns>
        ConsumerRegistration FindConsumerRegistration(string key);
        /// <summary>
        /// Get an Access Token from storage.
        /// </summary>
        /// <param name="key">The key for the entry.</param>
        /// <returns>A populated ServerAccessToken object, or null if it cannot be found.</returns>
        ServerAccessToken FindAccessToken(string key);
        /// <summary>
        /// Get a Request Token from storage.
        /// </summary>
        /// <param name="key">The key for the entry.</param>
        /// <returns>A populated ServerRequestToken object, or null if it cannot be found.</returns>
        ServerRequestToken FindRequestToken(string key);
        /// <summary>
        /// Delete a RequestToken from storage.
        /// </summary>
        /// <param name="key">The key for the entry.</param>
        void DeleteRequestToken(string key);
        /// <summary>
        /// Delete a AccessToken from storage.
        /// </summary>
        /// <param name="key">The key for the entry.</param>
        void DeleteAccessToken(string key);
        /// <summary>
        /// Delete a Consumer Registration entry from storage.
        /// </summary>
        /// <param name="key">The key for the entry.</param>
        void DeleteConsumerRegistration(string key);
        /// <summary>
        /// Save a Consumer Registration entry to storage.
        /// </summary>
        /// <param name="item">The item to store.</param>
        void StoreConsumerRegistration(ConsumerRegistration item);
        /// <summary>
        /// Save an AccessToken to storage.
        /// </summary>
        /// <param name="item">The item to store.</param>
        void StoreAccessToken(ServerAccessToken item);
        /// <summary>
        /// Save a RequestToken to storage.
        /// </summary>
        /// <param name="item">The item to store.</param>
        void StoreRequestToken(ServerRequestToken item);
    }

    /// <summary>
    /// Stores tokens in the current HttpApplication object.
    /// Good for testing, not recommended for production use.
    /// </summary>
    public class ApplicationServerTokenStore : IServerTokenStore
    {
        const string Prefix = "OAuth_";

        static T Retrieve<T>(string key) where T : class
        {
            string str = HttpContext.Current.Application[Prefix + key] as string;
            T item = OAuthUtility.Deserialize<T>(str) as T;
            if (item != null) return item;
            return null;
        }

        static void Save<T>(string key, T data) where T : class
        {
            HttpContext.Current.Application[Prefix + key] = OAuthUtility.Serialize<T>(data);
        }

        static void Delete(string key)
        {
            HttpContext.Current.Application.Remove(Prefix + key);
        }

        #region IServerTokenStore Members

        /// <summary>
        /// Retrieve a stored ConsumerRegistration entry.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <returns>ConsumerRegistration object representing the stored entry.</returns>
        public ConsumerRegistration FindConsumerRegistration(string key)
        {
            return Retrieve<ConsumerRegistration>(key);
        }
        /// <summary>
        /// Retrieve a stored AccessToken entry.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <returns>AccessToken object representing the stored entry.</returns>
        public ServerAccessToken FindAccessToken(string key)
        {
            HttpApplicationState app = HttpContext.Current.Application;
            return Retrieve<ServerAccessToken>(key);
        }
        /// <summary>
        /// Retrieve a stored RequestToken entry.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <returns>RequestToken object representing the stored entry.</returns>
        public ServerRequestToken FindRequestToken(string key)
        {
            return Retrieve<ServerRequestToken>(key);
        }

        /// <summary>
        /// Delete a stored RequestToken entry.
        /// </summary>
        /// <param name="key">Entry key.</param>
        public void DeleteRequestToken(string key)
        {
            Delete(key);
        }

        /// <summary>
        /// Delete a stored AccessToken entry.
        /// </summary>
        /// <param name="key">Entry key.</param>
        public void DeleteAccessToken(string key)
        {
            Delete(key);
        }

        /// <summary>
        /// Delete a stored ConsumerRegistration entry.
        /// </summary>
        /// <param name="key">Entry key.</param>
        public void DeleteConsumerRegistration(string key)
        {
            Delete(key);
        }

        /// <summary>
        /// Save a modified or new AccessToken entry.
        /// </summary>
        /// <param name="item">Item to save.</param>
        public void StoreAccessToken(ServerAccessToken item)
        {
            Save(item.Key, item);
        }

        /// <summary>
        /// Save a modified or new RequestToken entry.
        /// </summary>
        /// <param name="item">Item to save.</param>
        public void StoreRequestToken(ServerRequestToken item)
        {
            Save(item.Key, item);
        }

        /// <summary>
        /// Save a modified or new ConsumerRegistration entry.
        /// </summary>
        /// <param name="item">Item to save.</param>        
        public void StoreConsumerRegistration(ConsumerRegistration item)
        {
            Save(item.ConsumerKey, item);
        }

        #endregion
    }

    /// <summary>
    /// Stores tokens in a .NET compatible SQL database.
    /// </summary>
    /// <remarks>
    /// Requires a single table with three varchar fields: Key (255), Type (255), and ObjectData (4000 or TEXT).
    /// Indexes should be present on the Key and Type columns.
    /// </remarks>
    public class DBServerTokenStore : IServerTokenStore, IDisposable
    {
        IDbConnection Connection;
        string TableName;

        /// <summary>
        /// Create the table in the supplied database connection, using the supplied
        /// table name.
        /// </summary>
        /// <param name="connection">Database connection to use.</param>
        /// <param name="tableName">Name of the table to create.</param>
        public static void SetupDB(IDbConnection connection, string tableName)
        {
            connection.Open();
            IDbCommand comm = connection.CreateCommand();
            comm.CommandText = String.Format(CultureInfo.InvariantCulture, "CREATE TABLE {0} (OAuth_Key VARCHAR(255), OAuth_Type VARCHAR(255), OAuth_ObjectData TEXT);", tableName);
            comm.ExecuteNonQuery();
            comm.CommandText = String.Format(CultureInfo.InvariantCulture, "CREATE UNIQUE INDEX OAuth_Key_IDX ON {0} (OAuth_Key);", tableName);
            comm.ExecuteNonQuery();
            comm.CommandText = String.Format(CultureInfo.InvariantCulture, "CREATE INDEX OAuth_Type_IDX ON {0} (OAuth_Type);", tableName);
            comm.ExecuteNonQuery();
            connection.Close();
        }

        /// <summary>
        /// Read the configuration out of the web.config file and create the table
        /// in the database.
        /// </summary>
        public static void SetupDB()
        {
            SetupDB(GetConfiguredConnection(), GetConfiguredTableName());
        }

        /// <summary>
        /// Create the table in the current database connection, using the current
        /// table name.
        /// </summary>
        public void ConfigureDB()
        {
            SetupDB(Connection, TableName);
        }

        /// <summary>
        /// Creates a new instance of DBServerTokenStore.
        /// </summary>
        /// <param name="connection">The IDbConnection object representing a closed database connection.</param>
        /// <param name="tableName">The name the table where token data is stored.</param>
        public DBServerTokenStore(IDbConnection connection, string tableName)
        {
            Connection = connection;
            connection.Open();
            TableName = tableName;
        }

        /// <summary>
        /// Creates a new instance of DBServerTokenStore, getting configuration
        /// from the ASP.NET web.config file.
        /// </summary>
        public DBServerTokenStore()
        {
            IDbConnection conn = GetConfiguredConnection();
            TableName = GetConfiguredTableName();
            conn.Open();
            Connection = conn;
        }

        /// <summary>
        /// Get the configured database connection data from the web.config file.
        /// </summary>
        /// <returns>A configured IDbConnection object.</returns>
        static IDbConnection GetConfiguredConnection()
        {
            string connname = ConfigurationSettings.AppSettings[Strings.DbStorageConnection];
            ConnectionStringSettingsCollection connstrcol = System.Web.Configuration.WebConfigurationManager.ConnectionStrings;
            ConnectionStringSettings setting = connstrcol[connname];
            DbProviderFactory factory = DbProviderFactories.GetFactory(setting.ProviderName);
            DbConnection conn = factory.CreateConnection();
            conn.ConnectionString = setting.ConnectionString;
            return conn;
        }

        /// <summary>
        /// Get the configured database table name from the web.config file.
        /// </summary>
        /// <returns>The configured table name.</returns>
        static string GetConfiguredTableName()
        {
            return ConfigurationSettings.AppSettings[Strings.DbStorageTableName];
        }

        /// <summary>
        /// Generically get an item from the database based on its key and its type.
        /// </summary>
        /// <typeparam name="T">Type of the object to retrieve.</typeparam>
        /// <param name="key">Key of the object.</param>
        /// <returns>Retrieved object, if found.  Null if not found.</returns>
        T GetItem<T>(string key) where T : class
        {
            T item = null;
            using (IDbCommand comm = Connection.CreateCommand())
            {
                comm.CommandText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0} WHERE OAuth_Key = '{1}' AND OAuth_Type = '{2}';", TableName, ToSql(key), ToSql(typeof(T).ToString()));
                using (IDataReader idr = comm.ExecuteReader())
                {
                    if (idr.Read())
                    {
                        item = OAuthUtility.Deserialize<T>(FromSql(idr["OAuth_ObjectData"].ToString()));
                    }
                }
            }
            return item;
        }

        /// <summary>
        /// Save an item to the database based on its key and its type.
        /// </summary>
        /// <typeparam name="T">Type of object to save.</typeparam>
        /// <param name="key">Key of the object.</param>
        /// <param name="item">The object to save.</param>
        void SaveItem<T>(string key, T item) where T : class
        {
            T storedItem = GetItem<T>(key);
            using (IDbCommand comm = Connection.CreateCommand())
            {
                if (storedItem == null)
                {
                    comm.CommandText = string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0} (OAuth_Key, OAuth_Type, OAuth_ObjectData) VALUES ('{1}', '{2}', '{3}');", TableName, ToSql(key), ToSql(typeof(T).ToString()), ToSql(OAuthUtility.Serialize(item)));
                }
                else
                {
                    comm.CommandText = string.Format(CultureInfo.InvariantCulture, "UPDATE {0} SET OAuth_Type = '{1}', OAuth_ObjectData = '{2}' WHERE OAuth_Key = '{3}';", TableName, ToSql(typeof(T).ToString()), ToSql(OAuthUtility.Serialize(item)), ToSql(key));
                }
                comm.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Delete an object from the database using its key.
        /// </summary>
        /// <param name="key">Key of the object.</param>
        void DeleteItem(string key)
        {
            using (IDbCommand comm = Connection.CreateCommand())
            {
                comm.CommandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE OAuth_Key = '{1}';", TableName, ToSql(key));
                comm.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Replace any potentially dangerous characters with safer equivalents.
        /// </summary>
        /// <param name="sql">The input string.</param>
        /// <returns>Sanitized output string.</returns>
        static string ToSql(string sql)
        {
            return HttpUtility.HtmlEncode(sql);
        }

        /// <summary>
        /// Convert a safe string back to its original value.
        /// </summary>
        /// <param name="sql">The input string.</param>
        /// <returns>Restored output string.</returns>
        static string FromSql(string sql)
        {
            return HttpUtility.HtmlDecode(sql);
        }

        #region IServerTokenStore Members

        /// <summary>
        /// Retrieve a stored ConsumerRegistration entry.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <returns>ConsumerRegistration object representing the stored entry.</returns>
        public ConsumerRegistration FindConsumerRegistration(string key)
        {
            ConsumerRegistration cr = GetItem<ConsumerRegistration>(key);
            return cr;
        }

        /// <summary>
        /// Retrieve a stored AccessToken entry.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <returns>AccessToken object representing the stored entry.</returns>
        public ServerAccessToken FindAccessToken(string key)
        {
            ServerAccessToken accessToken = GetItem<ServerAccessToken>(key);
            return accessToken;
        }

        /// <summary>
        /// Retrieve a stored RequestToken entry.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <returns>RequestToken object representing the stored entry.</returns>
        public ServerRequestToken FindRequestToken(string key)
        {
            ServerRequestToken requestToken = GetItem<ServerRequestToken>(key);
            return requestToken;
        }

        /// <summary>
        /// Delete a stored RequestToken entry.
        /// </summary>
        /// <param name="key">Entry key.</param>
        public void DeleteRequestToken(string key)
        {
            DeleteItem(key);
        }

        /// <summary>
        /// Delete a stored AccessToken entry.
        /// </summary>
        /// <param name="key">Entry key.</param>
        public void DeleteAccessToken(string key)
        {
            DeleteItem(key);
        }

        /// <summary>
        /// Delete a stored ConsumerRegistration entry.
        /// </summary>
        /// <param name="key">Entry key.</param>
        public void DeleteConsumerRegistration(string key)
        {
            DeleteItem(key);
        }

        /// <summary>
        /// Save a modified or new AccessToken entry.
        /// </summary>
        /// <param name="item">Item to save.</param>
        public void StoreAccessToken(ServerAccessToken item)
        {
            SaveItem(item.Key, item);
        }

        /// <summary>
        /// Save a modified or new RequestToken entry.
        /// </summary>
        /// <param name="item">Item to save.</param>
        public void StoreRequestToken(ServerRequestToken item)
        {
            SaveItem(item.Key, item);
        }

        /// <summary>
        /// Save a modified or new ConsumerRegistration entry.
        /// </summary>
        /// <param name="item">Item to save.</param>
        public void StoreConsumerRegistration(ConsumerRegistration item)
        {
            SaveItem(item.ConsumerKey, item);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Clean up all used unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implements proper Dispose pattern.
        /// </summary>
        /// <param name="disposing">True if ran by Dispose(), false if by the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Connection.Dispose();
                Connection = null;
            }
        }

        #endregion
    }
}
