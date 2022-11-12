using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace JosLibrary.Communication.Database
{
    public class MsSQLDatabase : Database<SqlDbType>
    {
        public MsSQLDatabase()
            : base()
        {
        }

        public MsSQLDatabase(string server, string databaseName)
            : base(server, databaseName, null, null)
        {
        }

        public MsSQLDatabase(string server, string databaseName, string username, string password)
            : base(server, databaseName, username, password)
        {
        }

        public override bool ExecuteQuery(string query)
        {
            bool errorOccurred = false;

            if (Connection == null) return false;

            _databaseLock.WaitOne();
            try
            {
                using (SqlCommand command = new SqlCommand(query, (SqlConnection)Connection))
                {
                    if (Connection.State != ConnectionState.Open)
                    {
                        Open();
                    }
                    command.ExecuteNonQuery();
                }
                if (Connection.State != ConnectionState.Closed)
                {
                    Close();
                }
            }
            catch (Exception e)
            {
                ShowDatabaseError(e, query);

                errorOccurred = true;
            }
            finally
            {
                _databaseLock.ReleaseMutex();
            }

            return !errorOccurred;
        }

        public override bool ExecuteQuery(string query, Action<DbDataReader> dataCallback)
        {
            bool errorOccurred = false;

            if (Connection == null) return false;

            _databaseLock.WaitOne();
            try
            {
                using (SqlCommand command = new SqlCommand(query, (SqlConnection)Connection))
                {
                    if (Connection.State != ConnectionState.Open)
                    {
                        Open();
                    }
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        dataCallback(rdr);
                    }
                    rdr.Close();
                }
                if (Connection.State != ConnectionState.Closed)
                {
                    Close();
                }
            }
            catch (Exception e)
            {
                ShowDatabaseError(e, query);

                errorOccurred = true;
            }
            finally
            {
                _databaseLock.ReleaseMutex();
            }

            return !errorOccurred;
        }

        public override bool ExecuteQuery(string query, out object returnVal)
        {
            bool errorOccurred = false;
            returnVal = null;

            if (Connection == null) return false;

            _databaseLock.WaitOne();
            try
            {
                using (SqlCommand command = new SqlCommand(query, (SqlConnection)Connection))
                {
                    if (Connection.State != ConnectionState.Open)
                    {
                        Open();
                    }
                    returnVal = command.ExecuteScalar();
                }
                if (Connection.State != ConnectionState.Closed)
                {
                    Close();
                }
            }
            catch (Exception e)
            {
                ShowDatabaseError(e, query);

                errorOccurred = true;
            }
            finally
            {
                _databaseLock.ReleaseMutex();
            }

            return !errorOccurred;
        }

        public override bool ExecuteStoredProcedure(string sp, IEnumerable<DatabaseQueryArgument<SqlDbType>> arguments)
        {
            bool errorOccurred = false;

            if (Connection == null) return false;

            _databaseLock.WaitOne();
            try
            {
                using (SqlCommand command = new SqlCommand(sp, (SqlConnection)Connection) { CommandType = CommandType.StoredProcedure })
                {
                    foreach (var item in arguments)
                    {
                        command.Parameters.Add(item.Name, item.Type).Value = item.Value;
                    }

                    if (Connection.State != ConnectionState.Open)
                    {
                        Open();
                    }
                    command.ExecuteNonQuery();
                }
                if (Connection.State != ConnectionState.Closed)
                {
                    Close();
                }
            }
            catch (Exception e)
            {
                ShowDatabaseError(e, sp);

                errorOccurred = true;
            }
            finally
            {
                _databaseLock.ReleaseMutex();
            }

            return !errorOccurred;
        }
        public override bool ExecuteStoredProcedure(string sp, IEnumerable<DatabaseQueryArgument<SqlDbType>> arguments, out object returnVal)
        {
            bool errorOccurred = false;
            returnVal = null;

            if (Connection == null) return false;

            _databaseLock.WaitOne();
            try
            {
                using (SqlCommand command = new SqlCommand(sp, (SqlConnection)Connection) { CommandType = CommandType.StoredProcedure })
                {
                    foreach (var item in arguments)
                    {
                        command.Parameters.Add(item.Name, item.Type).Value = item.Value;
                    }

                    if (Connection.State != ConnectionState.Open)
                    {
                        Open();
                    }
                    returnVal = command.ExecuteScalar();
                }
                if (Connection.State != ConnectionState.Closed)
                {
                    Close();
                }
            }
            catch (Exception e)
            {
                ShowDatabaseError(e, sp);

                errorOccurred = true;
            }
            finally
            {
                _databaseLock.ReleaseMutex();
            }

            return !errorOccurred;
        }

        public override bool ExecuteStoredProcedure(string sp, IEnumerable<DatabaseQueryArgument<SqlDbType>> arguments, Action<DbDataReader> dataCallback)
        {
            bool errorOccurred = false;

            if (Connection == null) return false;

            _databaseLock.WaitOne();
            try
            {
                using (SqlCommand command = new SqlCommand(sp, (SqlConnection)Connection) { CommandType = CommandType.StoredProcedure })
                {
                    foreach (var item in arguments)
                    {
                        command.Parameters.Add(item.Name, item.Type).Value = item.Value;
                    }

                    if (Connection.State != ConnectionState.Open)
                    {
                        Open();
                    }
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        dataCallback(rdr);
                    }
                    rdr.Close();
                }
                if (Connection.State != ConnectionState.Closed)
                {
                    Close();
                }
            }
            catch (Exception e)
            {
                ShowDatabaseError(e, sp);

                errorOccurred = true;
            }
            finally
            {
                _databaseLock.ReleaseMutex();
            }

            return !errorOccurred;
        }

        protected override DbConnection CreateConnection(string server, string databaseName, string username, string password, int timeout)
        {
            // Data Source=Desktop-JosWigc\SQLEXPRESS;Initial Catalog=VolumeController;Integrated Security=True

            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(username))
            {
                string connstring = string.Format("Data Source={0}; Initial Catalog={1}; Integrated Security=True; Connection Timeout={2}", Server, DatabaseName, timeout);
                return new SqlConnection(connstring);
            }
            else
            {
                string connstring = string.Format("Data Source={0}; Initial Catalog={1}; Integrated Security=True; Connection Timeout={2}", Server, DatabaseName, timeout);
                return new SqlConnection(connstring);
            }
            
        }
    }
}
