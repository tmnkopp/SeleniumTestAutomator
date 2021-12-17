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

            IAlert alert;
            try
            {
                IWebElement ele = driver.FindElements(By.CssSelector("input[type='checkbox']")).FirstOrDefault();  
                if (ele?.GetAttribute("checked") == "checked") 
                {
                    ele?.Click();
                    driver.SwitchTo()?.Alert()?.Accept();
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
            try
            {
                IWebElement ele = driver.FindElements(By.CssSelector("button[class='rbCheckBox']")).FirstOrDefault();
                if (ele?.GetAttribute("checked") == "checked")
                {
                    ele?.Click();
                    driver.SwitchTo()?.Alert()?.Accept();
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
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            ele = wait.Until(drv => drv.FindElement(By.CssSelector("*[id$='_DeleteButton']")));
            ele.Click();
            alert = driver.SwitchTo().Alert();
            alert.Accept();

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            ele = wait.Until(drv => drv.FindElement(By.CssSelector("*[id$='_AddNewRecordButton_input']")));
            ele.Click();

            List<IValueSetter> valueSetters = new List<IValueSetter>();
                valueSetters.Add(new TextInputValueSetter());
                valueSetters.Add(new RadDropDownListValueSetter());
                valueSetters.Add(new RadioValueSetter());
                valueSetters.Add(new RadButtonValueSetter());
                valueSetters.Add(new SelectValueSetter()); 
            NaiveAutomator naiveFormFill = new NaiveAutomator(valueSetters) ;
            naiveFormFill.ContainerSelector = ".rgEditRow"; 
            naiveFormFill.Automate(sessionContext);

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            ele = wait.Until(drv => drv.FindElement(By.CssSelector("*[id$='_PerformInsertButton']")));
            ele.Click(); 
        }
        #endregion
    }
}
