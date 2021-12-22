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
        #endregion
          
        #region CTOR
        public InvGrid()
        { 
        }
        #endregion

        #region METHODS
        public virtual void Automate(SessionContext sessionContext)
        {
            this.driver = sessionContext.Driver;

            IReadOnlyCollection<IWebElement> elist =
                driver.FindElementsByXPath(@"//table[contains(@class,'rgMasterTable')]/tbody//tr[contains(@class,'Row')]");
            var ids = (from e in elist select e.GetAttribute("id")).ToList();
            foreach (string id in ids)
            { 
                try
                { 
                    var esublist = driver.FindElementsByXPath($"//tr[contains(@id, '{id}')]//a[contains(text(), 'Reset')]");
                    if (esublist.Count > 0)
                    {
                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                        esublist[0].Click();
                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                        IAlert alert = driver.SwitchTo().Alert();
                        alert.Accept();
                    } 
                }
                catch (StaleElementReferenceException ex)
                {
                    sessionContext.Logger.Warning($"StaleElementReferenceException {id} {ex.Message} {ex.InnerException}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"{id} {ex.Message} {ex.InnerException}");
                }

            }
            foreach (string id in ids)
            {
                try
                { 
                    var esublist = driver.FindElementsByXPath($"//tr[contains(@id, '{id}')]//input[contains(@id, '_EditButton')]");
                    if (esublist.Count > 0)
                    {
                        esublist[0].Click();
                        var inputs = driver.FindElementsByXPath($"//tr[contains(@id, '{id}')]//input[contains(@type, 'text')]");
                        foreach (var input in inputs)
                        { 
                            input.Clear();
                            input.SendKeys("1"); 
                        }
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
                    throw new Exception($"{id} {ex.Message} {ex.InnerException}");
                }

            }
            foreach (string id in ids)
            {
                try 
                { 
                    var elements = driver.FindElementsByXPath($"//tr[contains(@id, '{id}')]//a[contains(text(), 'Submit')]"); 
                    if (elements.Count > 0)
                    {
                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", elements[0]);
                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                        driver.SwitchTo().Alert().Accept();
                    }
                }
                catch (StaleElementReferenceException ex)
                {
                    sessionContext.Logger.Warning($"StaleElementReferenceException {id} {ex.Message} {ex.InnerException}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"{id} {ex.Message} {ex.InnerException}");
                } 
            }
        }
        #endregion 
    }
}
