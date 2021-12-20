using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
 
namespace CyberScope.Tests.Selenium
{
    internal class NaiveFismaFormAutomator : NaiveAutomator, IAutomator 
    {
        #region CTOR  
        public NaiveFismaFormAutomator() : base()
        {
            this.ContainerSelector = "div[id*='ctl00_ContentPlaceHolder1_Panel'] table ";
            this.OnPreAutomate += (sender, e) =>
            { 
                var eles = new WebDriverWait(driver, TimeSpan.FromSeconds(1))
                  .Until(drv => drv.FindElements(By.CssSelector($"*[id$='_btnEdit']")));
                if (eles.Count > 0) 
                    eles.FirstOrDefault().Click(); 
            };
            this.OnPostAutomate += (sender, e) =>
            {
                string errorSelector = "span[id*='_lblError']";
                var error = e.Driver.FindElements(By.CssSelector(errorSelector))?.FirstOrDefault(); 
                if (error?.GetType() == typeof(IWebElement))
                {
                    e.Driver.FindElements(By.CssSelector("*[id$='_btnEdit']"))?.FirstOrDefault().Click();
                    throw new Exception($"Form Contains Errors {error.Text}"); 
                } else {
                    var eles = new WebDriverWait(driver, TimeSpan.FromSeconds(1))
                        .Until(drv => drv.FindElements(By.CssSelector($"*[id$='_btnSave']")));
                    if (eles.Count > 0)
                        eles.FirstOrDefault().Click();
                      
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
                    error = e.Driver.FindElements(By.CssSelector(errorSelector))?.FirstOrDefault(); 
                    if (error != null)
                    {
                        string errtxt = error.Text; 
                        throw new Exception($"Form Contains Errors {errtxt}");
                    }
                } 
            }; 
        } 
        #endregion
    }
    internal class NaiveAutomator : BaseAutomator, IAutomator 
    { 
        #region PROPS  
        private Random _random = new Random();
        private SessionContext sessionContext;
        #endregion

        #region CTOR
        public NaiveAutomator():base()
        {  
        }
        public NaiveAutomator(List<IValueSetter> valueSetters) : base(valueSetters)
        { 
        }
        #endregion

        #region METHODS

        public virtual void Automate(SessionContext sessionContext)
        {
            this.driver = sessionContext.Driver;
            this.sessionContext = sessionContext;
            var eContainer = driver.FindElements(By.CssSelector($"{this.container}")); 

            var args = new AutomatorEventArgs(driver);
            PreAutomate(args);

            var defaults = GetInputDefaults();
            int posts = 0;
            while (true) { 
                int precnt = GetDisplayedElements().Count();
                foreach (var setter in ValueSetters)
                {
                    var meta = (ValueSetterMeta)Attribute.GetCustomAttribute(setter.GetType(), typeof(ValueSetterMeta));
                    var selector = $"{this.container} {meta.Selector}";
                    
                    if (driver.FindElements(By.CssSelector($"{selector}")).Count < 1)
                        continue; 

                    ElementIdIterator(selector, (ElementId) =>
                    {
                        IValueSetter valueSetter = setter;
                        valueSetter.Overwrite = posts==0;
                        valueSetter.Defaults = defaults;
                        try
                        {
                            sessionContext.Logger.Warning($"SetValue ElementId: {ElementId}");
                            valueSetter.SetValue(driver, ElementId);
                        }
                        catch (StaleElementReferenceException ex)
                        {
                            sessionContext.Logger.Warning($"StaleElementReferenceException {ElementId} {ex.Message} {ex.InnerException}"); 
                        }
                        catch (Exception ex)
                        { 
                            throw new Exception($"{ElementId} {ex.Message} {ex.InnerException}");
                        } 
                    });
                } 
                int postcnt = GetDisplayedElements().Count();
                ((IJavaScriptExecutor)driver).ExecuteScript("document.title=arguments[0];", $"{precnt}:{postcnt}");
                posts++;
                if (precnt >= postcnt || posts > 5) 
                    break; 
            }  
            PostAutomate(args);
        }

        #region PRIV

        private void ElementIdIterator(string Selector, Action<string> InputAction)
        {
            inputs = driver.FindElements(By.CssSelector($"{Selector}"));
            var elmts = (from i in inputs
                         where i.Enabled && i.Displayed
                         select new { id = i.GetAttribute("id") }).ToList();
            while (elmts.Count > 0)
            {
                InputAction(elmts[0].id);
                elmts.RemoveAt(0);
            }
        }

        private Dictionary<string, string> GetInputDefaults()
        { 
            Dictionary<string, string> DefaultValues = SettingsProvider.InputDefaults[$"Global"];
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
                            if (DefaultValues.ContainsKey(kv.Key))
                                DefaultValues[kv.Key] = kv.Value;
                            else
                                DefaultValues.Add(kv.Key, kv.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    sessionContext.Logger.Error($"GetInputDefaults: {ex.Message} {ex.InnerException}");
                } 
            } 
            return DefaultValues;
        }

        private IReadOnlyCollection<IWebElement> GetDisplayedElements()
        {
            IReadOnlyCollection<IWebElement> eCollection =
                (from e in driver.FindElement(By.CssSelector($"{this.container}")).FindElements(By.XPath($"//input|//select|//textarea"))
                 where e.Displayed
                 select e).ToList();
            return eCollection;
        }

        #endregion

        #endregion
    }
}
