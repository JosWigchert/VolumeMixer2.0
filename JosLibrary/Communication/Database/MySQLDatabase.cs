using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace JosLibrary.Communication.Database
{
    public class MySQLDatabase : Database<MySqlDbType>
    {
        public MySQLDatabase()
            : base()
        {
        }

        public MySQLDatabase(string server, string databaseName, string username, string password) 
            : base(server, databaseName, username, password)
        {
        }

        public override bool ExecuteQuery(string query)
        {
            bool errorOccurred = false;

            _databaseLock.WaitOne();
            try
            {
                using (SqlCommand command = new SqlCommand(query, (SqlConnection)Connection))
                {
                    if (Connection.State != ConnectionState.Open)
                    {
                        Connection.Open();
                    }
                    command.ExecuteNonQuery();
                }
                if (Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
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

            _databaseLock.WaitOne();
            try
            {
                using (MySqlCommand command = new MySqlCommand(query, (MySqlConnection)Connection))
                {
                    if (Connection.State != ConnectionState.Open)
                    {
                        Connection.Open();
                    }
                    MySqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        dataCallback(rdr);
                    }
                    rdr.Close();
                }
                if (Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
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

            _databaseLock.WaitOne();
            try
            {
                using (MySqlCommand command = new MySqlCommand(query, (MySqlConnection)Connection))
                {
                    if (Connection.State != ConnectionState.Open)
                    {
                        Connection.Open();
                    }
                    returnVal = command.ExecuteScalar();
                }
                if (Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
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

        public override bool ExecuteStoredProcedure(string sp, IEnumerable<DatabaseQueryArgument<MySqlDbType>> arguments)
        {
            bool errorOccurred = false;

            _databaseLock.WaitOne();
            try
            {
                using (MySqlCommand command = new MySqlCommand(sp, (MySqlConnection)Connection) { CommandType = CommandType.StoredProcedure })
                {
                    foreach (var item in arguments)
                    {
                        command.Parameters.Add(item.Name, item.Type).Value = item.Value;
                    }

                    if (Connection.State != ConnectionState.Open)
                    {
                        Connection.Open();
                    }
                    command.ExecuteNonQuery();
                }
                if (Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
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
        public override bool ExecuteStoredProcedure(string sp, IEnumerable<DatabaseQueryArgument<MySqlDbType>> arguments, out object returnVal)
        {
            bool errorOccurred = false;
            returnVal = null;

            _databaseLock.WaitOne();
            try
            {
                using (MySqlCommand command = new MySqlCommand(sp, (MySqlConnection)Connection) { CommandType = CommandType.StoredProcedure })
                {
                    foreach (var item in arguments)
                    {
                        command.Parameters.Add(item.Name, item.Type).Value = item.Value;
                    }

                    if (Connection.State != ConnectionState.Open)
                    {
                        Connection.Open();
                    }
                    returnVal = command.ExecuteScalar();
                }
                if (Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
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

        public override bool ExecuteStoredProcedure(string sp, IEnumerable<DatabaseQueryArgument<MySqlDbType>> arguments, Action<DbDataReader> dataCallback)
        {
            bool errorOccurred = false;

            _databaseLock.WaitOne();
            try
            {
                using (MySqlCommand command = new MySqlCommand(sp, (MySqlConnection)Connection) { CommandType = CommandType.StoredProcedure })
                {
                    foreach (var item in arguments)
                    {
                        command.Parameters.Add(item.Name, item.Type).Value = item.Value;
                    }

                    if (Connection.State != ConnectionState.Open)
                    {
                        Connection.Open();
                    }
                    MySqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        dataCallback(rdr);
                    }
                    rdr.Close();
                }
                if (Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
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
            string connstring = string.Format("Server={0}; database={1}; UID={2}; password={3}; Connection Timeout={4}", Server, DatabaseName, Username, Password, timeout);
            return new MySqlConnection(connstring);
        }
    }
}
