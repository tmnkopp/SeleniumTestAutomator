using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
namespace CyberScope.Automator
{
    public class NaiveAutomator : BaseAutomator, IAutomator 
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
        public NaiveAutomator(string ContainerSelector) : base()
        {
            this.ContainerSelector = ContainerSelector;
        }
        #endregion

        #region METHODS
        public async Task AutomateAsync(SessionContext sessionContext)
        {
            await Task.Run(() =>
            {
                this.Automate(sessionContext);
            });   
        }
        public virtual void Automate(SessionContext sessionContext)
        {
            this.driver = sessionContext.Driver;
            this.sessionContext = sessionContext;
            var args = new AutomatorEventArgs(sessionContext);
            PreAutomate(args);
            int posts = 0,
                postcnt = 0,
                precnt = 0,
                valuesSet = 0;
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.1); 
            while (true) {

                precnt = GetDisplayedElementsCount();
                
                foreach (var setter in ValueSetters)
                {
                    var meta = (ValueSetterMeta)Attribute.GetCustomAttribute(setter.GetType(), typeof(ValueSetterMeta)); 
                    var selector = $"{this.ContainerSelector} {meta.Selector}"; 
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.01);
                    if (driver.FindElementsByCssSelector($"{selector}").Count < 1)
                        continue; 
                    ElementIdIterator(selector, (ElementId) =>
                    {
                        driver.FindElement(By.TagName($"body"))?.Click();
                        new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until((x) =>
                        {
                            return ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete");
                        }); 

                        sessionContext.Logger.Verbose($"NaiveAutomator:ValueSetterMeta {selector} {ElementId}");
                         
                        var element = driver.FindElement(By.Id(ElementId));
                        var script = "arguments[0].scrollIntoView(true);";
                        ((IJavaScriptExecutor)driver).ExecuteScript(script, element);

                        IValueSetter valueSetter = setter;
                        valueSetter.Overwrite = posts==0; 
                        try
                        { 
                            valueSetter.SetValue(sessionContext, ElementId);
                            valuesSet++;
                        }
                        catch (StaleElementReferenceException ex)
                        {
                            sessionContext.Logger.Warning($"NaiveAutomator StaleElementReferenceException {ElementId} {ex.Message} {ex.InnerException}"); 
                        }
                        catch (Exception ex)
                        {
                            sessionContext.Logger.Error($"NaiveAutomator Exception {ElementId} {ex.Message} {ex.InnerException}");
                            if (!Regex.IsMatch(ex.Message, "interactable|element with text")) 
                                throw new Exception($"NaiveAutomator {ElementId} {ex.Message} {ex.InnerException}"); 
                        } 
                    });

                }
                postcnt = GetDisplayedElementsCount();
                ((IJavaScriptExecutor)driver).ExecuteScript("document.title=arguments[0];", $"{precnt}:{postcnt}:{valuesSet}");
                sessionContext.Logger.Information($"NaiveAutomator precnt:{precnt}  postcnt:{postcnt} valuesSet:{valuesSet}");
                posts++;
                if (precnt >= postcnt || posts > 8) 
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
        private int GetDisplayedElementsCount()
        { 
            IWebElement container = driver.FindElement(By.CssSelector($"{this.ContainerSelector}")); 
            IReadOnlyCollection<IWebElement> eCollection;
            try
            { 
                eCollection =
                (from e in container.FindElements(By.XPath($"//input|//select|//textarea"))
                 where e.Displayed && e.Enabled
                 select e).ToList();
                return eCollection.Count;
            }
            catch (Exception ex)
            {
                sessionContext.Logger.Error($"NaiveAutomator {ex}");
            } 
            return 0;
        }
        #endregion
        #endregion
    } 
}