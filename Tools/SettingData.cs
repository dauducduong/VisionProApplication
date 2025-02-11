using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionProApplication.Tools
{
    public class SettingData
    {
        public int SavingOption { get; set; }
        public string SavingDir { get; set; }

        public SettingData(int intValue, string stringValue)
        {
            SavingOption = intValue;
            SavingDir = stringValue;
        }
    }
}
