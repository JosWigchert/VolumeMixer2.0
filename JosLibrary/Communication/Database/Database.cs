using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;

namespace JosLibrary.Communication.Database
{
    public abstract class Database<T> : IDatabase
    {
        public Database()
        {

        }

        public Database(string server, string databaseName, string username, string password)
        {
            Server = server;
            DatabaseName = databaseName;
            Username = username;
            Password = password;
        }

        private string m_server = "";

        public string Server
        {
            get { return m_server; }
            set { m_server = value; }
        }

        private string m_databaseName = "";

        public string DatabaseName
        {
            get { return m_databaseName; }
            set { m_databaseName = value; }
        }

        private string m_username = "";

        public string Username
        {
            get { return m_username; }
            set { m_username = value; }
        }

        private string m_password = "";

        public string Password
        {
            get { return m_password; }
            set { m_password = value; }
        }

        protected DbConnection Connection = null;
        protected Mutex _databaseLock = new Mutex();
        protected int _timeout = 10;


        /// <summary>
        /// Opens the specified database with a timeout of default 10 seconds
        /// </summary>
        /// <returns>True if the database is opened succesfully, false if something went wrong</returns>
        public bool Open()
        {
            return Open(_timeout);
        }
        /// <summary>
        /// Opens the specified database with a timeout specified in the parameter
        /// </summary>
        /// <param name="timeout">How many seconds the database should try to connect</param>
        /// <returns>True if the database is opened succesfully, false if something went wrong</returns>
        public bool Open(int timeout)
        {
            _timeout = timeout;

            if (Connection == null)
            {
                if (String.IsNullOrEmpty(Server) || String.IsNullOrEmpty(DatabaseName))
                {
                    return false;
                }


                Connection = CreateConnection(Server, DatabaseName, Username, Password, timeout);
            }


            if (Connection.State != System.Data.ConnectionState.Open)
            {
                try
                {
                    Connection.Open();
                    if (Connection.State != System.Data.ConnectionState.Open)
                    {
                        Console.WriteLine("Error Connecting to Database: {0}", Connection);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Connecting to Database: {0}", ex.Message);
                    return false;
                }

            }

            return true;
        }

        /// <summary>
        /// Closes the database connection if possible
        /// </summary>
        public void Close()
        {
            if (Connection != null)
            {
                if (Connection.State != System.Data.ConnectionState.Closed)
                {
                    Connection.Close();
                }
            }
        }

        /// <summary>
        /// Creates database object with the folowing parameters
        /// </summary>
        /// <param name="server">Server URL or ipadress</param>
        /// <param name="databaseName">Name of the database to connect to</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="timeout">How many seconds the database should try to connect</param>
        /// <returns>The DBConnection that will be used to communicate with the database</returns>
        protected abstract DbConnection CreateConnection(string server, string databaseName, string username, string password, int timeout);

        public abstract bool ExecuteQuery(string query);
        public abstract bool ExecuteQuery(string query, Action<DbDataReader> dataCallback);
        public abstract bool ExecuteQuery(string query, out object returnVal);
        public abstract bool ExecuteStoredProcedure(string sp, IEnumerable<DatabaseQueryArgument<T>> arguments);
        public abstract bool ExecuteStoredProcedure(string sp, IEnumerable<DatabaseQueryArgument<T>> arguments, out object returnVal);
        public abstract bool ExecuteStoredProcedure(string sp, IEnumerable<DatabaseQueryArgument<T>> arguments, Action<DbDataReader> dataCallback);

        protected void ShowDatabaseError(Exception e, string sp)
        {
            Trace.WriteLine(String.Format("Error with database storedprocedure: {0}", sp));
            Trace.WriteLine(String.Format("Error message: {0}", e.Message));
            Trace.WriteLine(String.Format("Stack trace: {0}", e.StackTrace));
        }
    }
}
