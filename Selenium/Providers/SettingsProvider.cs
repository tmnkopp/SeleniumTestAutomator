 
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
using Newtonsoft.Json;

namespace CyberScope.Tests.Selenium
{
    public class ControlLocator{
        public string Locator { get; set; }
        public string Type { get; set; }
        public string Selector { get; set; }
    }
    public static class SettingsProvider
    { 
        #region PROPS 
        private static string RawConfig() {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);
            return File.ReadAllText($"{dirPath}\\Selenium\\config.json"); 
        } 
        public static Dictionary<string, Dictionary<string, string>> InputDefaults
        {
            get {
                var local_settings = RawConfig();
                dynamic json = JsonConvert.DeserializeObject(local_settings);
                Dictionary<string, Dictionary<string, string>> config
                    = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>
                    (JsonConvert.SerializeObject(json.InputDefaults));
                return config;
            }
        }
        public static List<ControlLocator> ControlLocators
        { 
            get {
                var local_settings = RawConfig();
                List<ControlLocator> locators = new List<ControlLocator>();
                dynamic json = JsonConvert.DeserializeObject(local_settings); 
                foreach (var item in json.ControlLocators)
                {
                    ControlLocator i = JsonConvert.DeserializeObject<ControlLocator>(JsonConvert.SerializeObject(item));
                    locators.Add(i); 
                } 
                return locators;
            }
        }
        #endregion 
    }
}
