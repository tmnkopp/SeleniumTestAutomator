using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO; 
using System.Text.RegularExpressions;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace CyberScope.Automator
{
    public class ControlLocator{
        public string Locator { get; set; }
        public List<string> Exclude { get; set; } = new List<string>();
        public string Type { get; set; }
        public string Selector { get; set; }
        private string _ValueSetterTypes;
        public string ValueSetterTypes { get; set; } = ".*";
        public string ControlPrep { get; set; } = "";
        public bool Overwrite { get; set; } = true; 
    }     
    public static class SettingsProvider
    {
        public static Dictionary<string, string> appSettings
        {
            get
            {
                dynamic json = JsonConvert.DeserializeObject(RawConfig());
                string json_section = JsonConvert.SerializeObject(json.appSettings);
                Dictionary<string, string> result = JsonConvert.DeserializeObject<Dictionary<string, string>>(json_section); 
                return result; 
            }
        }
        public static string RawConfig() {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);

            var config = File.ReadAllText($@"{dirPath}\AutomatorConfig.json");
            JObject oConfig = JObject.Parse(config);
            string localPath = $@"{dirPath}\AutomatorConfig.local.json";
           
            if (!File.Exists(localPath)){
                return  JsonConvert.SerializeObject(oConfig);
            }
    
            JObject oLocal; 
            try
            {
                var local = File.ReadAllText(localPath);
                oLocal = JObject.Parse(local); 
            } 
            catch(Exception ex){
                return JsonConvert.SerializeObject(oConfig);
            }

            JObject oMerged = new JObject();
            oMerged.Merge(oConfig);
            oMerged.Merge(oLocal); 
            return JsonConvert.SerializeObject(oMerged);
        } 
        public static Dictionary<string, Dictionary<string, string>> TestSettings
        {
            get
            {
                dynamic json = JsonConvert.DeserializeObject(RawConfig());
                string json_section = JsonConvert.SerializeObject(json.TestConfig);
                var appSettings = Automator.SettingsProvider.appSettings;
                foreach (var item in appSettings)
                {
                    var k = item.Key;
                    var v = item.Value; 
                    json_section = json_section.Replace("{AppSettings:" + k + "}", SettingsProvider.appSettings[k]); 
                } 
                Dictionary<string, Dictionary<string, string>> config;
                config = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>
                        (json_section);
                return config;
            }
        } 
        public static List<string> ChromeOptions
        {
            get
            { 
                dynamic json = JsonConvert.DeserializeObject(RawConfig());
                var options = json.ChromeOptions; 
                return ((JArray)options).Select(i => (string)i).ToList();
            }
        }
        public static Dictionary<string, Dictionary<string, string>> InputDefaults
        {
            get {
                var json_config = RawConfig();
                dynamic obj = JsonConvert.DeserializeObject(json_config);
                string json_input_defaults = JsonConvert.SerializeObject(obj.InputDefaults);
                Dictionary<string, Dictionary<string, string>> config; 
                config = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>
                    (json_input_defaults);
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
     
    }
}