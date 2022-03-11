using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CyberScope.Tests.Selenium
{
    internal class ImportableDataGrid : BaseAutomator, IAutomator
    {
        #region PROPS
        private IWebElement ele;
        private WebDriverWait wait;
        #endregion 

        #region CTOR
        public ImportableDataGrid()
        {
             
        }
        #endregion

        #region METHODS
        public virtual void Automate(SessionContext sessionContext)
        {
            this.driver = sessionContext.Driver;
            
            IWebElement ele;
            IAlert alert;

            ele = driver.FindElements(By.XPath("//input[contains(@id, 'import')]")).FirstOrDefault();
            if (ele != null)
            {
                ele.Click();
                alert = driver.SwitchTo().Alert();
                alert.Accept();
            }

            Thread.Sleep(6000); 
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.01);

            ele = driver.FindElements(By.XPath("//*[contains(@id, '_rbtnDownload')]")).FirstOrDefault();
            if (ele != null) ele.Click();
             
            var path = ConfigurationManager.AppSettings.Get($"DownloadsDir");
            if (path != null)
            {
                Thread.Sleep(2000);
                var file = new DirectoryInfo(path)
                            .GetFiles()
                            .Where(f => !f.FullName.EndsWith(".tmp"))
                            .OrderByDescending(f => f.LastWriteTime)
                            .FirstOrDefault()
                            .FullName; 
                ele = driver.FindElements(By.XPath("//*[contains(@id, '_fileUpload')]")).FirstOrDefault();
                ele.SendKeys(file);
                ele = driver.FindElements(By.XPath("//*[contains(@id, '_cmdUpload')]")).FirstOrDefault();
                if (ele != null) ele.Click();
                Thread.Sleep(5000);
            }
             
            try
            { 
                ele = driver.FindElements(By.XPath("//*[contains(@id, '_cb_NA')]")).FirstOrDefault();
                IWebElement elespan = ele.FindElements(By.XPath("//*[contains(@class, 'rbToggleCheckbox')]")).FirstOrDefault();
                var chkd = elespan.GetAttribute("class").Contains("rbToggleCheckboxChecked"); 
                if (!chkd) ele?.Click(); 
            }
            catch (StaleElementReferenceException ex)
            {
                sessionContext.Logger.Warning($"StaleElementReferenceException {ex.Message} {ex.InnerException}");
            }

            ele = driver.FindElements(By.XPath("//*[contains(@id, '_ctl04_EditButton')]")).FirstOrDefault();
            if (ele != null) ele.Click();
            NaiveAutomator naiveFormFill = new NaiveAutomator(this.ValueSetters) ;
            naiveFormFill.ContainerSelector = ".rgEditRow"; 
            naiveFormFill.Automate(sessionContext); 
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            ele = wait.Until(drv => drv.FindElement(By.CssSelector("*[id$='_UpdateButton']")));
            ele.Click();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.01);
        }
        #endregion
    }
}
