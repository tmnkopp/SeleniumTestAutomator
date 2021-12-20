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
        SessionContext _sessionContext;
        public DefaultInputProvider(SessionContext sessionContext)
        {
            this._sessionContext = sessionContext;
        }
        #region PROPS
        private Dictionary<string, string> _DefaultValues = new Dictionary<string, string>();
        public Dictionary<string, string> DefaultValues
        {
            get {
                if (_DefaultValues.Count < 1) 
                    LoadItems(); 
                return _DefaultValues; 
            }
        }
        #endregion

        #region Methods
          
        private void LoadItems()
        {
            var driver = _sessionContext.Driver;
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
                    _sessionContext.Logger.Error($"GetInputDefaults: {ex.Message} {ex.InnerException}");
                }
            } 
        }
        #endregion
    }
}
