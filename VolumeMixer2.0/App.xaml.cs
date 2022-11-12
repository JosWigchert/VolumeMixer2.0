using JosLibrary.Communication.Database;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace VolumeMixer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            GlobalHandler.Instance.Database = new MsSQLDatabase("DESKTOP-JOSWIGC\\SQLEXPRESS", "VolumeController");
            GlobalHandler.Instance.Database.Open(10);
            GlobalHandler.Instance.Database.Close();
        }
    }
}
