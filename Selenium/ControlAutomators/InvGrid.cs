using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberScope.Tests.Selenium
{ 
    internal class InvGrid : BaseAutomator, IAutomator 
    {
        #region PROPS
        private IWebElement ele;
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
        public virtual void Automate(SessionContext sessionContext)
        {
            this.sessionContext = sessionContext;
            this.driver = sessionContext.Driver;

            IReadOnlyCollection<IWebElement> elist =
                driver.FindElementsByXPath(@"//table[contains(@class,'rgMasterTable')]/tbody//tr[contains(@class,'Row')]");
            rowIds = (from e in elist select e.GetAttribute("id")).ToList();
             
            RowCommandPrepare("Reset");

            foreach (string id in rowIds)
            {
                try
                { 
                    var esublist = driver.FindElementsByXPath($"//tr[contains(@id, '{id}')]//input[contains(@id, '_EditButton')]");
                    if (esublist.Count > 0)
                    {
                        esublist[0].Click();  
                        NaiveAutomator na = new NaiveAutomator(this.ValueSetters); 
                        na.ContainerSelector = $"#{id}";
                        na.Automate(sessionContext);

                        // var inputs = driver.FindElementsByXPath($"//tr[contains(@id, '{id}')]//input[contains(@type, 'text')]");
                        // foreach (var input in inputs)
                        // { 
                        //     input.Clear();
                        //     input.SendKeys("1"); 
                        // }
                    }
                    var elements = driver.FindElements(By.CssSelector($"#{id} input[id*=_UpdateButton]"));
                    if (elements.Count > 0)
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", elements[0]);
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
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

            }
            RowCommandPrepare("Submit"); 
        }

        private void RowCommandPrepare(string RowCommand) {
            bool hasRowCommands = driver.FindElementsByXPath($"//tr[contains(@class,'Row')]//a[contains(text(), '{RowCommand}')]").Count > 0;
            int ittr = 0;
            while (hasRowCommands)
            {
                try
                {
                    var elements = driver.FindElementsByXPath($"//tr[contains(@class,'Row')]//a[contains(text(), '{RowCommand}')]");
                    if (elements.Count > 0)
                    {
                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", elements[0]);
                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
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
                hasRowCommands = driver.FindElementsByXPath($"//tr[contains(@class,'Row')]//a[contains(text(), '{RowCommand}')]").Count > 0;
                ittr++;
                if (ittr > rowIds.Count + 5)
                    break;
            }
        }
        #endregion 
    }
}
