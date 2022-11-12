using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VolumeMixer.Models
{
    public class AudioSliderModel : BindableBase
    {
        private string _displayName = "";

        public string DisplayName
        {
            get { return string.IsNullOrEmpty(_displayName) ? ProgramName : _displayName; }
            set { SetProperty(ref _displayName, value); }
        }

        private string _programName = "";

        public string ProgramName
        {
            get { return _programName; }
            set { SetProperty(ref _programName, value); RaisePropertyChanged(DisplayName); }
        }

        private float _value = 0.5f;

        public float Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        private bool _muted;

        public bool Muted
        {
            get { return _muted; }
            set { SetProperty(ref _muted, value); }
        }

        private bool _enabled = true;

        public bool Enabled
        {
            get { return _enabled; }
            set { SetProperty(ref _enabled, value); }
        }

    }
}
