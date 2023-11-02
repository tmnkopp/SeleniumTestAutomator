using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CyberScope.Automator
{
    internal class MultiHeader : BaseAutomator, IAutomator
    {
        #region PROPS
        private IWebElement ele;
        private WebDriverWait wait;
        private SessionContext sessionContext;
        private ChromeDriver driver;
        private List<string> rowIds;
        #endregion
        #region CTOR
        public MultiHeader()
        {
        }
        #endregion
        public virtual void Automate(SessionContext sessionContext)
        {
            this.sessionContext = sessionContext;
            this.driver = sessionContext.Driver;
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            IReadOnlyCollection<IWebElement> elist =
                driver.FindElementsByXPath(@"//table[contains(@class,'rgMasterTable')]/tbody//tr[contains(@class,'Row')]");
            rowIds = (from e in elist select e.GetAttribute("id")).ToList();
            foreach (string id in rowIds)
            { 
                try
                {
                    var esublist = driver.FindElementsByXPath($"//tr[contains(@id, '{id}')]//input[contains(@id, '_EditButton')]");
                    if (esublist.Count > 0)
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", esublist[0]);
                        NaiveAutomator na = new NaiveAutomator(this.ValueSetters);
                        na.ContainerSelector = $"#{id}";
                        Thread.Sleep(1000);
                        na.Automate(sessionContext);
                    }
                    var elements = driver.FindElements(By.CssSelector($"#{id} input[id*=_UpdateButton]"));
                    if (elements.Count > 0)
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", elements[0]);
                }
                catch (StaleElementReferenceException ex)
                {
                    sessionContext.Logger.Warning($"StaleElementReferenceException {id} {ex.Message} {ex.InnerException}");
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("element not interactable"))
                        sessionContext.Logger.Error($"element not interactable {id} ");
                    else
                        throw new Exception($"{id} {ex.Message} {ex.InnerException}");
                }

                var e = new AutomatorEventArgs(sessionContext);
                 
                if (driver.FindElementsByXPath(SettingsProvider.appSettings[$"ErrorXPath"]).Count > 0)
                    FormError(e);
                FormSubmitted(e);

                if (this.AutomatorState == AutomatorState.AutomationComplete)
                    break;
            } 
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.01);
        }
    }
}
