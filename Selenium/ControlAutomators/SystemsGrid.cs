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
            IReadOnlyCollection<IWebElement> edits = wait.Until(drv =>
                drv.FindElements(By.CssSelector($"{ this.ContainerSelector } *[id$='_EditButton']")));

            var ids = (from e in edits select e.GetAttribute("id")).ToList<string>();
            
            NaiveAutomator na = new NaiveAutomator(); 
            na.ValueSetters = this.ValueSetters; 
            foreach (string id in ids)
            {   
                driver.FindElementByXPath($"//*[contains(@id, '{id}')]").Click();
                string container_id = driver.FindElementsByXPath(
                    $"//*[contains(@id, '_UpdateButton')]/../../../*[@class='rgEditRow']"
                    ).FirstOrDefault()?.GetAttribute("id");

                na.ContainerSelector = (string.IsNullOrEmpty(container_id)) ? this.ContainerSelector : $"#{container_id} ";
                na.Automate(sessionContext);  
                driver.FindElementByXPath($"//*[contains(@id, '_UpdateButton')]").Click();
                var args = new AutomatorEventArgs(driver);
                this.FormSubmitted(args); 
            }
        }
        #endregion
    }
}
