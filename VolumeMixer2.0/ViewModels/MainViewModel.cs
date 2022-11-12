using JosLibrary.WPF.Command;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using VolumeMixer.Models;
using VolumeMixer.Views;
using JosLibrary.Audio;
using JosLibrary.Collections.Generic;
using System.Collections.Generic;
using System;
using System.Windows.Threading;
using JosLibrary.Communication.Database;
using System.Threading.Tasks;
using System.Data.Common;

namespace VolumeMixer.ViewModels
{
    public class MainViewModel : BindableBase
    {
        #region Commands

        private ICommand _launchGithubProperty;
        public ICommand LaunchGithubProperty
        {
            get
            {
                if (_launchGithubProperty == null)
                {
                    _launchGithubProperty = new RelayCommand(param => OpenUrl("https://github.com/joswigchert"));
                }
                return _launchGithubProperty;
            }
            set { SetProperty(ref _launchGithubProperty, value); }
        }

        private ICommand _updateSliderProperty;
        public ICommand UpdateSliderProperty
        {
            get
            {
                if (_updateSliderProperty == null)
                {
                    _updateSliderProperty = new RelayCommand(param => OnSliderUpdate((string)param));
                }
                return _updateSliderProperty;
            }
            set { SetProperty(ref _updateSliderProperty, value); }
        }

        #endregion

        #region Properties

        private MainView _view;

        public MainView View
        {
            get { return _view; }
            set { _view = value; }
        }

        private DispatcherTimer _programSlidersUpdateTimer;

        private AudioSliderModel _masterSlider;

        public AudioSliderModel MasterSlider
        {
            get { return _masterSlider; }
            set { SetProperty(ref _masterSlider, value); }
        }

        private ObservableCollection<AudioSliderModel> _programSliders;

        public ObservableCollection<AudioSliderModel> ProgramSliders
        {
            get { return _programSliders; }
            set { _programSliders = value; }
        }

        private ObservableCollection<AudioSliderModel> _categorySliders;

        public ObservableCollection<AudioSliderModel> CategorySliders
        {
            get { return _categorySliders; }
            set { _categorySliders = value; }
        }

        private MultiDict<string, AudioSession> _audioSessions;

        public MultiDict<string, AudioSession> AudioSessions
        {
            get { return _audioSessions; }
            set { _audioSessions = value; }
        }

        private Dictionary<int, string> _audioCategories;


        #endregion

        #region Constructors
        public MainViewModel(MainView view)
        {
            View = view;

            InitializeSliders();
            InitializeBackgroundTasks();
        }

        private void InitializeBackgroundTasks()
        {
            InitializeProgramSlidersBackgroundUpdate(1000);
        }

        private void InitializeProgramSlidersBackgroundUpdate(int seconds)
        {
            DispatcherTimer _programSlidersUpdateTimer = new DispatcherTimer();

            _programSlidersUpdateTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            _programSlidersUpdateTimer.Interval = TimeSpan.FromMilliseconds(seconds);
            _programSlidersUpdateTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            UpdateProgramSliders();

            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        private void InitializeSliders()
        {
            InitializeMasterSlider();
            InitializeProgramSliders();
            InitializeCategorySliders();
        }

        private void InitializeMasterSlider()
        {
            MasterSlider = new AudioSliderModel();
            MasterSlider.ProgramName = "Master Volume";
            MasterSlider.Value = (float)Math.Floor(AudioUtilities.GetMasterVolume());
            MasterSlider.Muted = AudioUtilities.GetMasterVolumeMute();
        }

        private void InitializeProgramSliders()
        {
            InitializeAudioSessions();

            if (ProgramSliders == null)
            {
                ProgramSliders = new ObservableCollection<AudioSliderModel>();
            }

            ProgramSliders.Clear();

            foreach (string name in _audioSessions.Keys)
            {
                AudioSliderModel sliderModel = new AudioSliderModel { ProgramName = name, Value = (float)Math.Floor(_audioSessions[name][0].GetVolume() * 100), Muted = _audioSessions[name][0].GetMute() };
                GetProgramDisplayName(sliderModel);
                ProgramSliders.Add(sliderModel);
            }
        }
        private void InitializeCategorySliders()
        {
            List<MsSqlDatabaseQueryArgument> arguments = new List<MsSqlDatabaseQueryArgument>();

            if (_audioCategories == null)
            {
                _audioCategories = new Dictionary<int, string>();
            }
            _audioCategories.Clear();

            GlobalHandler.Instance.Database.ExecuteStoredProcedure("GetCategories", arguments, (DbDataReader rdr) => {
                _audioCategories.Add((int)rdr[0], (string)rdr[1]);
            });

            if (CategorySliders == null)
            {
                CategorySliders = new ObservableCollection<AudioSliderModel>();
            }

            CategorySliders.Clear();

            foreach (string name in _audioCategories.Values)
            {
                AudioSliderModel sliderModel = new AudioSliderModel { ProgramName = name, Value = 50, Muted = false };
                CategorySliders.Add(sliderModel);
            }
        }

        private async void GetProgramDisplayName(AudioSliderModel sliderModel)
        {
            if (string.IsNullOrEmpty(sliderModel.ProgramName));

            List<MsSqlDatabaseQueryArgument> arguments = new List<MsSqlDatabaseQueryArgument>();
            arguments.Add(new MsSqlDatabaseQueryArgument() { Name = "ProgramName", Type = System.Data.SqlDbType.VarChar, Value = sliderModel.ProgramName });

            GlobalHandler.Instance.Database.ExecuteStoredProcedure("GetProgramDisplayName", arguments, out object result);

            string DisplayName = (string)result;

            if (!string.IsNullOrEmpty(DisplayName))
            {
                sliderModel.DisplayName = DisplayName;
            }
        }

        private void UpdateProgramSliders()
        {
            if (ProgramSliders == null) return;

            UpdateAudioSessions();

            List<AudioSliderModel> toRemove = new List<AudioSliderModel>();

            foreach (AudioSliderModel item in ProgramSliders)
            {
                if (!AudioSessions.ContainsKey(item.ProgramName))
                {
                    toRemove.Add(item);
                }
            }

            toRemove.ForEach(item => { ProgramSliders.Remove(item); });

            foreach (string name in AudioSessions.Keys)
            {
                bool contains = false;
                foreach (AudioSliderModel item in ProgramSliders)
                {
                    if (item.ProgramName == name)
                    {
                        contains = true;
                    }
                }

                if (!contains)
                {
                    AudioSliderModel sliderModel = new AudioSliderModel { ProgramName = name, Value = (float)Math.Floor(_audioSessions[name][0].GetVolume() * 100), Muted = _audioSessions[name][0].GetMute() };
                    GetProgramDisplayName(sliderModel);
                    ProgramSliders.Add(sliderModel);
                }
            }
        }

        private void InitializeAudioSessions()
        {
            if (AudioSessions == null)
            {
                AudioSessions = new MultiDict<string, AudioSession>();
            }

            AudioSessions.Clear();

            foreach (AudioSession s in AudioUtilities.GetAllSessions())
            {
                if (s.Process != null && s.State != AudioSessionState.Expired)
                {
                    AudioSessions.Add(s.Process.ProcessName, s);

                    if (!GlobalHandler.Instance.ProgramBacklog.Contains(s.Process.ProcessName))
                    { 
                        GlobalHandler.Instance.ProgramBacklog.Add(s.Process.ProcessName);
                    }
                }
            }
        }

        private void UpdateAudioSessions()
        {
            if (AudioSessions == null)
            {
                InitializeAudioSessions();
            }
            else
            {

                // TODO: WIP, check all audiosessions if they are present in the applications that are running, otherise remove them. 
                // (for the moment just initialize again)
                // dont forget to add new things to the all programs list

                InitializeAudioSessions();
                /*var sessions = AudioUtilities.GetAllSessions();

                foreach (AudioSession s in sessions)
                {
                    if (s.Process == null) continue;

                    if (!AudioSessions.Contains(s.Process.ProcessName, s))
                    {
                        AudioSessions.Add(s.Process.ProcessName, s);
                    }
                }

                foreach (string item in AudioSessions.Keys)
                {
                
                    List<AudioSession> currentSessions = AudioSessions[item];
                    if (!sessions.Contains())
                    {

                    }
                }*/
            }
        }

        private void OnSliderUpdate(string program)
        {
            if (MasterSlider.ProgramName == program)
            {
                AudioUtilities.SetMasterVolume(MasterSlider.Value);
                AudioUtilities.SetMasterVolumeMute(MasterSlider.Muted);
            }

            foreach (var item in ProgramSliders)
            {
                if (item.DisplayName == program)
                {
                    AudioSessions[item.ProgramName].ForEach(s => { s.SetVolume(item.Value); s.SetMute(item.Muted); });
                }
            }
        }
    }
}
