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

        public NaiveFismaFormAutomator()
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


        #endregion

        #region CTOR
        public NaiveAutomator()
        { 
        }
        public NaiveAutomator(
        EventHandler<AutomatorEventArgs> PreAutomate
        , EventHandler<AutomatorEventArgs> PostAutomate)
        {
            this.OnPreAutomate += PreAutomate;
            this.OnPostAutomate += PostAutomate;
        } 
        public NaiveAutomator(
        EventHandler<AutomatorEventArgs> PreAutomate
        , EventHandler<AutomatorEventArgs> PostAutomate 
        , string ContainerCssSelector
        ) : this(PreAutomate, PostAutomate)
        {
            this.ContainerSelector = ContainerCssSelector;
        }
        public NaiveAutomator(
        EventHandler<AutomatorEventArgs> PreAutomate
        , EventHandler<AutomatorEventArgs> PostAutomate 
        , string ContainerCssSelector
        , string PK_FORM
            ) : this(PreAutomate, PostAutomate, ContainerCssSelector)
        {
            this.PK_FORM = PK_FORM;
        } 
        public NaiveAutomator(string PK_FORM)
        {
            this.PK_FORM = PK_FORM;
        }
        #endregion 

        #region METHODS

        private void ElementIdIterator(string Selector, Action<string> InputAction)
        {
            inputs = driver.FindElements(By.CssSelector($"{Selector}"));
            var elmts = (from i in inputs
                        where i.Enabled && i.Displayed
                        select new { id=i.GetAttribute("id") }).ToList();
            while (elmts.Count > 0)
            {
                InputAction(elmts[0].id); 
                elmts.RemoveAt(0);
            }
        }

        public virtual void Automate(ChromeDriver driver)
        {
            this.driver = driver;
            IWebElement eContainer = driver.FindElement(By.CssSelector($"{this.container}")); 

            var args = new AutomatorEventArgs(driver);
            PreAutomate(args);
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

            Dictionary<string, IValueSetter> valueSetters = new Dictionary<string, IValueSetter>();
            valueSetters.Add($"{this.container} input[type='radio']", new RadioValueSetter());
            valueSetters.Add($"{this.container} select:not([id*='_ddl_Sections'])", new SelectValueSetter());
            valueSetters.Add($"{this.container} *[class*='RadDropDownList']", new RadDropDownListValueSetter());
            valueSetters.Add($"{this.container} div[class*='RadComboBox']", new RadComboBoxValueSetter());
            valueSetters.Add($"{this.container} textarea", new TextAreaValueSetter());
            valueSetters.Add($"{this.container} input[type='text']:not([readonly='readonly'])", new TextInputValueSetter());

            int posts = 0;
            while (true) { 
                int precnt = GetDisplayedElements().Count();
                foreach (var item in valueSetters)
                {
                    ElementIdIterator(item.Key, (ElementId) =>
                    {
                        IValueSetter valueSetter = item.Value;
                        valueSetter.Overwrite = posts==0;
                        valueSetter.Defaults = InputDefaults;
                        try
                        {
                            valueSetter.SetValue(driver, ElementId);
                        }
                        catch (StaleElementReferenceException ex)
                        { 
                            throw new Exception($"{ElementId} {ex.Message}");
                        }
                        catch (Exception ex)
                        { 
                            throw new Exception($"{ElementId} {ex.Message}");
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
        private IReadOnlyCollection<IWebElement> GetDisplayedElements() {
            IReadOnlyCollection<IWebElement> eCollection = 
                (from e in driver.FindElement(By.CssSelector($"{this.container}")).FindElements(By.XPath($"//input|//select|//textarea"))
                where e.Displayed
                select e).ToList();
            return eCollection;
        }
        private string GetRand()
        { 
            return string.Format("{0}:{1}"
                , DateTime.Now.TimeOfDay.Hours.ToString()
                , DateTime.Now.TimeOfDay.Minutes.ToString());
        } 
        #endregion 
    }
}
