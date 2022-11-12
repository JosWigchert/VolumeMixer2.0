using JosLibrary.Communication.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolumeMixer.Config;

namespace VolumeMixer
{
    public class GlobalHandler
    {
        #region Singleton
        private static readonly GlobalHandler instance = new GlobalHandler();
        static GlobalHandler()
        {
        }
        public static GlobalHandler Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion

        private GlobalHandler()
        {
            _programBacklog = new AudioProgramBacklog();
            _programBacklog.Load();
        }

        private AudioProgramBacklog _programBacklog;

        public AudioProgramBacklog ProgramBacklog
        {
            get { return _programBacklog; }
            set { _programBacklog = value; }
        }


        private MsSQLDatabase _database; // change type to change database type

        public MsSQLDatabase Database
        {
            get { return _database; }
            set { _database = value; }
        }

    }
}
