using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
namespace CyberScope.Automator
{
    internal class ServicesGrid : BaseAutomator, IAutomator
    {
        #region PROPS
        private IWebElement ele;
        private WebDriverWait wait;
        #endregion 
        #region CTOR
        public ServicesGrid()
        {
        }
        #endregion
        #region METHODS
        public virtual void Automate(SessionContext sessionContext)
        {
            this.driver = sessionContext.Driver;
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1.5);
            IAlert alert;
            try
            {
                IWebElement ele = driver.FindElements(By.CssSelector("input[type='checkbox']")).FirstOrDefault();
                var chk = ele?.GetAttribute("checked");
                if (chk == "true" || chk == "checked")
                {
                    ele?.Click();
                }
            }
            catch (StaleElementReferenceException ex)
            {
                sessionContext.Logger.Warning($"StaleElementReferenceException {ex.Message} {ex.InnerException}");
            }
            try
            {
                IWebElement ele = driver.FindElements(By.CssSelector("button[class='rbCheckBox']")).FirstOrDefault();
                var chk = ele?.GetAttribute("checked");
                if (chk == "true" || chk == "checked")
                {
                    ele?.Click();
                }
            }
            catch (StaleElementReferenceException ex)
            {
                sessionContext.Logger.Warning($"StaleElementReferenceException {ex.Message} {ex.InnerException}");
            }
              
            ele = driver.FindElements(By.XPath("//input[@type='file']")).FirstOrDefault();
            if (ele != null)
            {
                foreach (var item in sessionContext.Defaults.EmptyIfNull())
                {
                    if (Regex.Match(ele.GetAttribute("id"), item.Key, RegexOptions.IgnoreCase).Success)
                    {
                        ele.SendKeys(item.Value);
                        ele = driver.FindElements(By.XPath("//input[contains(@value,'Upload')]")).FirstOrDefault();
                        ele?.Click();
                        break;
                    }
                }
            }
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
 
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.01);
             
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            ele = wait.Until(drv => drv.FindElement(By.XPath("//*[contains(@class, 'rgCommandRow')]//*[contains(@value, 'Add New') or contains(text(), 'Add New')]")));
         
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", ele);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.01); 
            NaiveAutomator naiveFormFill = new NaiveAutomator();
            naiveFormFill.ContainerSelector = ".rgEditRow";
            naiveFormFill.Automate(sessionContext);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            Thread.Sleep(500);
            ele = wait.Until(drv => drv.FindElement(By.CssSelector("*[id$='_PerformInsertButton']")));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", ele);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.01);

        } 
        #endregion
    }
}