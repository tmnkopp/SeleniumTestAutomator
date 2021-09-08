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
    public class SystemsGrid : BaseAutomator, IAutomator 
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
        public void Automate(ChromeDriver driver)
        {
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            IReadOnlyCollection<IWebElement> elist = wait.Until(drv =>
                drv.FindElements(By.CssSelector(".rgMasterTable tr[class$='Row']")));
            var ids = (from e in elist select e.GetAttribute("id")).ToList();

            foreach (string id in ids)
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
                var esublist = driver.FindElementsByXPath($"//tr[contains(@id, '{id}')]//input[contains(@id, '_EditButton')]");
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
                    driver.FindElementByXPath($"//tr[contains(@id, '{id}')]//input[contains(@id, '_UpdateButton')]").Click();
                }
            }
        }
        #endregion
    }
}
