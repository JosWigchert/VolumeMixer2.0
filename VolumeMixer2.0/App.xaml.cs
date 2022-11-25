using JosLibrary.Communication.Database;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Forms = System.Windows.Forms;
using Hardcodet.Wpf.TaskbarNotification;
using VolumeMixer.Views;

namespace VolumeMixer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;

        public App()
        {
            GlobalHandler.Instance.Database = new MsSQLDatabase("DESKTOP-JOSWIGC\\SQLEXPRESS", "VolumeController");
            GlobalHandler.Instance.Database.Open(10);
            GlobalHandler.Instance.Database.Close();

            
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            notifyIcon = new TaskbarIcon()
            {
                Icon = new Icon("Resources/mixer-icon-96.ico"),
                ToolTipText = "Left-click to open",
                Visibility = Visibility.Visible,
                TrayPopup = new TrayView()
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            notifyIcon.Dispose();
        }

        private void Open(object sender, EventArgs e)
        {
            
        }

        private void Settings(object sender, EventArgs e)
        {

        }

        private void Exit(object sender, EventArgs e)
        {
            this.Shutdown();
        }
    }


}
