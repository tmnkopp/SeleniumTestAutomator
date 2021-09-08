 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Configuration;

namespace CyberScope.Tests.Selenium 
{ 
    public static class SettingsProvider
    {

        #region PROPS
         
        public static Dictionary<string, string> InputDefaults
        {
            get => ProvideConfig()["InputDefaults"];
        }
        public static Dictionary<string, Dictionary<string, string>> Config
        {
            get => ProvideConfig();
        }
        public static Dictionary<string, string> CompositeControls
        {
            get => ProvideConfig()["CompositeControls"];
        }
        #endregion

        #region METHODS
         
        private static Dictionary<string, Dictionary<string, string>> ProvideConfig()
        {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath); 
            var local_settings = File.ReadAllText($"{dirPath}\\Selenium\\config.json");
            var json = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(local_settings);
            return json;
        }   

        #endregion
    }
}
