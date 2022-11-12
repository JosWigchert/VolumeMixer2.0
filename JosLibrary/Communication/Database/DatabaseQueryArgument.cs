using MySql.Data.MySqlClient;
using System.Data;
using System.Data.OleDb;

namespace JosLibrary.Communication.Database
{
    public class DatabaseQueryArgument<T>
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private T _type;

        public T Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private object _value;

        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }

    public class MsSqlDatabaseQueryArgument : DatabaseQueryArgument<SqlDbType> { }
    public class MySqlDatabaseQueryArgument : DatabaseQueryArgument<MySqlDbType> { }
}
