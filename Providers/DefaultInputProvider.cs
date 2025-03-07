using CyberScope.Automator.Providers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyCsvParser;

namespace CyberScope.Automator
{
    public class DefaultInputProvider
    {

        #region CTOR

        ChromeDriver driver;
        Dictionary<string, string> _DefaultValues = new Dictionary<string, string>();
        public DefaultInputProvider(ChromeDriver driver)
        {
            this.driver = driver;
        }

        #endregion

        #region PROPS 
        public Dictionary<string, string> DefaultValues
        {
            get {
                if (_DefaultValues.Count < 1) 
                    Populate(); 
                return _DefaultValues; 
            }
        }
        #endregion
       
        #region Methods 
        private void Populate()
        {
            this._DefaultValues = SettingsProvider.InputDefaults[$"Global"];
            foreach (var item in SettingsProvider.InputDefaults)
            {
                try
                {
                    IReadOnlyCollection<IWebElement> elmts = driver.FindElements(By.XPath(item.Key));
                    if (elmts.Count > 0)
                    {
                        Dictionary<string, string> dct = SettingsProvider.InputDefaults[item.Key];
                        foreach (var kv in dct)
                        {
                            var value = kv.Value;
                            if(kv.Value.StartsWith("//"))
                            { 
                                var elms =  driver.FindElements(By.XPath(kv.Value));
                                if (elms.Count > 0) value = elms[0].Text; 
                            }
                            this.addKey(kv.Key, value); 
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetInputDefaults: {ex.Message} {ex.InnerException}");
                }
            }
            //  try
            //  {
            //      string _fileName = Automator.SettingsProvider.appSettings[$"TestDataDir"].Replace("{Type}_Validations", "ElementPath");
            //      var csvParser = new CsvParser<ElementValueMap>(new CsvParserOptions(true, ','), new ElementValueMapper());
            //      var result = csvParser.ReadFromFile(_fileName, Encoding.ASCII).ToList();
            //      foreach (var item in result)
            //      {
            //          var row = item.Result.GetAsRow;
            //          var elm = driver.FindElementByXPath((string)row[0]);
            //          this.addKey((string)row[1], elm.Text);
            //      }
            //  }
            //  catch (Exception ex)
            //  {
            //      Console.WriteLine($"GetInputDefaults: {ex.Message} {ex.InnerException}");
            //  }

            //sessionContext.Defaults = sessionContext.Defaults.MergeLeft(cvp.Answers);

        }
        private void addKey(string key, string value) {
            if (!string.IsNullOrEmpty(key))
                if (_DefaultValues.ContainsKey(key))
                    _DefaultValues[key] = value;
                else
                    _DefaultValues.Add(key, value);
        }
        #endregion
    }
}