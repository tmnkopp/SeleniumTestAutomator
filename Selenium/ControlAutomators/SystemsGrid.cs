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
    internal class SystemsGrid : BaseAutomator, IAutomator 
    {
        #region PROPS
        private IWebElement ele;
        private WebDriverWait wait;
        #endregion
         
        #region CTOR
        public SystemsGrid()
        {
        }
        #endregion

        #region METHODS
        public virtual void Automate(SessionContext sessionContext)
        {
            this.driver = sessionContext.Driver;

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            IReadOnlyCollection<IWebElement> elist = wait.Until(drv =>
                drv.FindElements(By.CssSelector(".rgMasterTable tr[class$='Row']")));
            var ids = (from e in elist select e.GetAttribute("id")).ToList();

            foreach (string id in ids)
            {
                try 
                { 
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
                    var esublist = driver.FindElementsByXPath($"//tr[contains(@id, '{id}')]//*[contains(@id, '_EditButton')]");
                    if (esublist.Count > 0)
                    {
                        esublist[0].Click();
                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
                        var inputs = driver.FindElementsByXPath($"//tr[contains(@id, '{id}')]//input[contains(@type, 'text')]");
                        foreach (var input in inputs)
                        {
                            input.Clear();
                            input.SendKeys("0");
                        }
                        driver.FindElementByXPath($"//tr[contains(@id, '{id}')]//*[contains(@id, '_UpdateButton')]").Click();
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
            }
        }
        #endregion
    }
}
