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
    internal class InvGrid : BaseAutomator, IAutomator 
    {
        #region PROPS
        private IWebElement ele;
        private IReadOnlyCollection<IWebElement> elist;
        private WebDriverWait wait;
        private SessionContext sessionContext;
        private ChromeDriver driver;
        private List<string> rowIds;
        #endregion
        #region CTOR
        public InvGrid()
        { 
        }
        #endregion
        #region METHODS
        private bool _hasData(){
            var query = driver.FindElementsByXPath(@"//tr[last()]/td/span[contains(@id, 'lblfirst_Total')]");
            if (query.Count > 0) 
                if (!string.IsNullOrWhiteSpace(query[0].Text) ) 
                    return true; 
            return false;
        }
        public virtual void Automate(SessionContext sessionContext)
        {
            this.sessionContext = sessionContext;
            this.driver = sessionContext.Driver;
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);

            if (this._hasData() && !this.Overwrite){
                this.AutomatorState = AutomatorState.AutomationComplete;
                return;
            }

            elist = driver.FindElementsByXPath(@"//table[contains(@class,'rgMasterTable')]/tbody//tr[contains(@class,'Row')]");
            rowIds = (from e in elist select e.GetAttribute("id")).ToList();
            RowCommandPrepare("Reset"); 
            foreach (string id in rowIds)
            {
                
                try
                {
                    var esublist = driver.FindElementsByXPath($"//tr[contains(@id, '{id}')]//input[contains(@id, '_EditButton')]");
                    if (esublist.Count > 0)
                    { 
                        ((IJavaScriptExecutor)driver).ExecuteScript($"document.getElementById('ctl00_lblStaging').innerHTML='<div style=\"color:#fff;\">{id}</div>';");
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", esublist[0]);
                        NaiveAutomator na = new NaiveAutomator(this.ValueSetters); 
                        na.ContainerSelector = $"#{id}";
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
                if (driver.FindElementsByXPath(@"//*[contains(@id, 'Error')]").Count > 0) 
                    FormError(e);
                FormSubmitted(e);

                if(this.AutomatorState == AutomatorState.AutomationComplete) 
                    break; 
            }
            RowCommandPrepare("Submit");
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.01);
        }
        private void RowCommandPrepare(string RowCommand) {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            bool hasRowCommands = driver.FindElementsByXPath($"//tr[contains(@class,'Row')]//a[contains(text(), '{RowCommand}')]").Count > 0;
            int ittr = 0;
            while (hasRowCommands)
            {
                try
                {
                    var elements = driver.FindElementsByXPath($"//tr[contains(@class,'Row')]//a[contains(text(), '{RowCommand}')]");
                    if (elements.Count > 0)
                    { 
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", elements[0]); 
                        IAlert alert = driver.SwitchTo().Alert();
                        alert.Accept();
                    }
                }
                catch (StaleElementReferenceException ex)
                {
                    sessionContext.Logger.Warning($"StaleElementReferenceException {ex.Message} {ex.InnerException}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"{ex.Message} {ex.InnerException}");
                }
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
                hasRowCommands = driver.FindElementsByXPath($"//tr[contains(@class,'Row')]//a[contains(text(), '{RowCommand}')]").Count > 0;
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                ittr++;
                if (ittr > rowIds.Count + 5)
                    break;
            }
        }
        #endregion 
    }
}