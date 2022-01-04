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
              
            NaiveAutomator na = new NaiveAutomator(); 
            na.ValueSetters = this.ValueSetters; 
            foreach (string id in ids)
            { 
                elist = wait.Until(drv =>
                    drv.FindElements(By.XPath($"//tr[contains(@id, '{id}')]//*[contains(@id, '_EditButton')]"))); 
                if (elist.Count > 0)
                {
                    elist.FirstOrDefault().Click(); 
                    na.ContainerSelector = $"#{id}";
                    na.Automate(sessionContext);  
                    driver.FindElementByXPath($"//tr[contains(@id, '{id}')]//*[contains(@id, '_UpdateButton')]").Click();
                    var args = new AutomatorEventArgs(driver);
                    this.FormSubmitted(args);
                } 
            }
        }
        #endregion
    }
}
