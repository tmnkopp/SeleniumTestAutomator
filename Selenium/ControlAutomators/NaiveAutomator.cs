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
   
            var args = new AutomatorEventArgs(driver);
            PreAutomate(args);
             
            int posts = 0;
            while (true) { 
                int precnt = GetDisplayedElements().Count();
                foreach (var setter in ValueSetters)
                {
                    var meta = (ValueSetterMeta)Attribute.GetCustomAttribute(setter.GetType(), typeof(ValueSetterMeta));
                    var selector = $"{this.ContainerSelector} {meta.Selector}";
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.01);
                    if (driver.FindElementsByCssSelector($"{selector}").Count < 1)
                        continue; 

                    ElementIdIterator(selector, (ElementId) =>
                    {
                        IValueSetter valueSetter = setter;
                        valueSetter.Overwrite = posts==0;
                        valueSetter.Defaults = sessionContext.Defaults;
                        try
                        {
                            valueSetter.SetValue(driver, ElementId);
                        }
                        catch (StaleElementReferenceException ex)
                        {
                            sessionContext.Logger.Warning($"StaleElementReferenceException {ElementId} {ex.Message} {ex.InnerException}"); 
                        }
                        catch (Exception ex)
                        {
                            if (!ex.Message.Contains("element not interactable"))
                            {
                                throw new Exception($"{ElementId} {ex.Message} {ex.InnerException}");
                            } 
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
         
        private IReadOnlyCollection<IWebElement> GetDisplayedElements()
        {
            IReadOnlyCollection<IWebElement> eCollection =
                (from e in driver.FindElement(By.CssSelector($"{this.ContainerSelector}")).FindElements(By.XPath($"//input|//select|//textarea"))
                 where e.Displayed
                 select e).ToList();
            return eCollection;
        }

        #endregion

        #endregion
    }
}
