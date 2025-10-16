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
        public string Original_CurrentValue { get; set; }

        public ScalabilitySetting(string command, string defaultValue, string currentValue)
        {
            Command = command;
            DefaultValue = defaultValue;
            CurrentValue = currentValue;

            Original_CurrentValue = currentValue;
        }

        public void ResetValues()
        {
            CurrentValue = Original_CurrentValue;
        }
    }
}
