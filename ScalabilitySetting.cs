using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureGroupsConfigurator
{
    public class ScalabilitySetting
    {
        public string Command { get; set; }
        public string DefaultValue { get; set; }
        public string CurrentValue { get; set; }

        public ScalabilitySetting(string command, string defaultValue, string currentValue)
        {
            Command = command;
            DefaultValue = defaultValue;
            CurrentValue = currentValue;
        }
    }
}
