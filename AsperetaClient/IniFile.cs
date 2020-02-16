using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AsperetaClient
{
    class IniFile
    {
        public Dictionary<string, Dictionary<string, string>> Sections { get; private set; }

        public IniFile(string filePath)
        {
            ReadFile(filePath);
        }

        private void ReadFile(string filePath)
        {
            Sections = new Dictionary<string, Dictionary<string, string>>();

            string currentSection = null;
            foreach (var line in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line[0] == ';') continue;

                if (line[0] == '[')
                {
                    currentSection = line.Substring(1, line.Length - 2);
                    Sections[currentSection] = new Dictionary<string, string>();
                    continue;
                }

                int equals = line.IndexOf('=');
                if (equals == -1) continue;

                string key = line.Substring(0, equals);
                string value = line.Substring(equals + 1, line.Length - equals - 1);
                Sections[currentSection][key] = value;

                //Console.WriteLine($"{currentSection} {key} = {value}");
            }
        }

        public int GetInt(string section, string key)
        {
            return Convert.ToInt32(Sections[section][key]);
        }

        public IEnumerable<int> GetCoords(string section, string key)
        {
            return Sections[section][key].Split(',').Select(o => Convert.ToInt32(o));
        }

        public Dictionary<string, string> this[string section]
        {
            get { return Sections[section]; }
        }
    }
}
