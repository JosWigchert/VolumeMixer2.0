using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VolumeMixer.Config
{
    [Serializable]
    public class AudioProgramBacklog
    {
        private string _fileName = "Files/ProgramBacklog.json";


        private HashSet<string> _programBacklog;

        public HashSet<string> ProgramBacklog
        {
            get { return _programBacklog; }
            set { _programBacklog = value; }
        }

        public AudioProgramBacklog()
        {
            _programBacklog = new HashSet<string>();
        }

        public void Load()
        {
            if (File.Exists(_fileName))
            {
                string jsonString = File.ReadAllText(_fileName);
                var jsonObject = JsonSerializer.Deserialize<AudioProgramBacklog>(jsonString);

                ProgramBacklog.Clear();
                ProgramBacklog = null;

                if (jsonObject != null)
                {
                    ProgramBacklog = jsonObject.ProgramBacklog;
                }
            }
        }

        public void Save()
        {
            string jsonString = JsonSerializer.Serialize(this);
            File.WriteAllText(_fileName, jsonString);
        }

        public void Add(string program)
        {
            if (ProgramBacklog != null)
            {
                ProgramBacklog.Add(program);
            }

            Save();
        }

        public bool Contains(string program)
        {
            return ProgramBacklog.Contains(program);
        }
    }
}
