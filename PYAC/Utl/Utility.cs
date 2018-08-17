using System;
using System.Collections.Generic;
using System.Text;

namespace Utl
{
    public class Utility
    {
        public static String getAppSetting(String key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
        }

        public static List<String> getMultiAppSetting(String key)
        {
            
            int count = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings[key + "Count"]);

            List<String> param = new List<string>();
            for (int i = 0; i < count; i++)
            {
                param.Add(System.Configuration.ConfigurationManager.AppSettings[key + i]);
            }

            return param;
        }

    }
}
