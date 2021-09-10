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
    public class NaiveFismaFormAutomator : NaiveAutomator, IAutomator 
    {
        #region CTOR 
        public NaiveFismaFormAutomator(string _PK_FORM) : this()
        {
            this.PK_FORM = _PK_FORM;
            this.ContainerSelector = "div[id*='ctl00_ContentPlaceHolder1_Panel'] table ";
        } 
        public NaiveFismaFormAutomator() : base()
        {
            this.OnPreAutomate += (sender, e) =>
            {
                new WebDriverWait(driver, TimeSpan.FromSeconds(1))
                  .Until(drv => drv.FindElement(By.CssSelector($"*[id$='_btnEdit']"))).Click();
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
                    new WebDriverWait(driver, TimeSpan.FromSeconds(1))
                        .Until(drv => drv.FindElement(By.CssSelector($"*[id$='_btnSave']"))).Click();

                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
                    error = e.Driver.FindElements(By.CssSelector(errorSelector))?.FirstOrDefault(); 
                    if (error != null)
                    {
                        string errtxt = error.Text;
                        //e.Driver.FindElements(By.CssSelector("*[id$='_btnEdit']"))?.FirstOrDefault().Click(); 
                        throw new Exception($"Form Contains Errors {errtxt}");
                    }
                } 
            }; 
        } 
        #endregion
    }
    public class NaiveAutomator : BaseAutomator, IAutomator 
    { 
        #region PROPS 
        
        private Random _random = new Random();
        private List<IValueSetter> valueSetters;
        public List<IValueSetter> ValueSetters { get => valueSetters; set=> valueSetters = value; }

        #endregion

        #region CTOR
        public NaiveAutomator()
        {
            // TODO: injet this dependancy 
            valueSetters = new List<IValueSetter>();
            var setters = (from assm in AppDomain.CurrentDomain.GetAssemblies()
                           where assm.FullName.Contains(AppDomain.CurrentDomain.FriendlyName)
                           from t in assm.GetTypes()
                           where typeof(IValueSetter).IsAssignableFrom(t)
                           && t.IsClass
                           select t).ToList(); 
            foreach (var type in setters) 
                valueSetters.Add((IValueSetter)Activator.CreateInstance(Type.GetType($"{type.FullName}"))); 

        }
        public NaiveAutomator(List<IValueSetter> valueSetters)
        {
            this.valueSetters = valueSetters; 
        }
        #endregion

        #region METHODS

        public virtual void Automate(ChromeDriver driver)
        {
            this.driver = driver;
            IWebElement eContainer = driver.FindElement(By.CssSelector($"{this.container}")); 

            var args = new AutomatorEventArgs(driver);
            PreAutomate(args);

            Dictionary<string, string> InputDefaults = GetInputDefaults();
                 
            int posts = 0;
            while (true) { 
                int precnt = GetDisplayedElements().Count();
                foreach (var setter in ValueSetters)
                {
                    var meta = (ValueSetterMeta)Attribute.GetCustomAttribute(setter.GetType(), typeof(ValueSetterMeta));
                    var selector = $"{this.container} {meta.Selector}";
                    ElementIdIterator(selector, (ElementId) =>
                    {
                        IValueSetter valueSetter = setter;
                        valueSetter.Overwrite = posts==0;
                        valueSetter.Defaults = InputDefaults;
                        try
                        {
                            valueSetter.SetValue(driver, ElementId);
                        }
                        catch (StaleElementReferenceException ex)
                        { 
                            throw new Exception($"{ElementId} {ex.Message} {ex.InnerException}");
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
            string dkey = this.DataCall;
            Dictionary<string, string> InputDefaults = SettingsProvider.InputDefaults[$"Global"];
            if (SettingsProvider.InputDefaults.ContainsKey($"{dkey}"))
            {
                foreach (var item in SettingsProvider.InputDefaults[$"{dkey}"])
                {
                    if (InputDefaults.ContainsKey(item.Key))
                        InputDefaults[item.Key] = item.Value;
                    else
                        InputDefaults.Add(item.Key, item.Value);
                }
            }
            return InputDefaults;
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
