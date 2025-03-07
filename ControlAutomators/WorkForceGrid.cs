using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace CyberScope.Automator
{
    internal class WorkForceGrid : BaseAutomator, IAutomator
    {
        #region PROPS
        private IWebElement ele;
        private WebDriverWait wait;
        #endregion
        #region CTOR
        public WorkForceGrid()
        {
        }
        #endregion
        #region METHODS
        public virtual void Automate(SessionContext sessionContext)
        {
            this.driver = sessionContext.Driver;
            var secs = TimeSpan.FromSeconds(5); 

            wait = new WebDriverWait(driver, secs);

            var element = wait.Until(drv => drv.FindElement(By.CssSelector("*[id*='_btnEdit']"))); 
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            Thread.Sleep(700);
            element.Click(); 

            new WebDriverWait(driver, secs).Until((x) => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
             
            NaiveAutomator naiveFormFill = new NaiveAutomator();
            naiveFormFill.ContainerSelector = ".row1";
            naiveFormFill.Automate(sessionContext);
             
            element = wait.Until(drv => drv.FindElement(By.CssSelector("*[id*='btnSave']")));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            Thread.Sleep(500);
            element.Click();

            Thread.Sleep(500);
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 0);");
            Thread.Sleep(500);

        }
        #endregion 
    }
}