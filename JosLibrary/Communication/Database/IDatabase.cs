using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosLibrary.Communication.Database
{
    public interface IDatabase
    {
        public abstract bool Open();

        public abstract bool Open(int timeout);

        public abstract void Close();
    }
}
