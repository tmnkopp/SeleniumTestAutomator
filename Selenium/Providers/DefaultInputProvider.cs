using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberScope.Tests.Selenium
{
    public class DefaultInputProvider
    { 
        ChromeDriver driver;
        public DefaultInputProvider(ChromeDriver driver)
        {
            this.driver = driver;
        }
        #region PROPS
        private Dictionary<string, string> _DefaultValues = new Dictionary<string, string>();
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
            _DefaultValues = new Dictionary<string, string>();
            _DefaultValues = SettingsProvider.InputDefaults[$"Global"];
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
                            if (_DefaultValues.ContainsKey(kv.Key))
                                _DefaultValues[kv.Key] = kv.Value;
                            else
                                _DefaultValues.Add(kv.Key, kv.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetInputDefaults: {ex.Message} {ex.InnerException}");
                }
            } 
        }
        #endregion
    }
}
