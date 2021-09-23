using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberScope.Tests.Selenium
{
    internal class RadGridForm : BaseAutomator, IAutomator
    {
        #region PROPS
        private IWebElement ele;
        private WebDriverWait wait;
        #endregion

        #region CTOR
        public RadGridForm()
        {
        }
        #endregion

        #region METHODS
        public virtual void Automate(SessionContext sessionContext)
        {
            this.driver = sessionContext.Driver;
             
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            ele = wait.Until(drv => drv.FindElement(By.CssSelector(".RadGrid .rgCommandRow [id*='AddNew']")));
            ele.Click();

            var eContainer = driver.FindElements(By.CssSelector($".rgEditForm"));

            if (eContainer.Count < 1)
            {
                this.driver.Navigate().Back();
            }
            else {
                NaiveAutomator naiveFormFill = new NaiveAutomator();
                naiveFormFill.ContainerSelector = ".rgEditForm";
                naiveFormFill.PK_FORM = base.PK_FORM;
                naiveFormFill.Automate(sessionContext);

                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
                ele = wait.Until(drv => drv.FindElement(By.CssSelector("*[id*='PerformInsert']")));
                ele.Click();

            } 
        }
        #endregion 
    }
}
