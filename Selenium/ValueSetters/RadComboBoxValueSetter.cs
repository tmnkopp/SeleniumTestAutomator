using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberScope.Tests.Selenium
{
    public class RadComboBoxValueSetter : BaseValueSetter, IValueSetter
    {
        private ChromeDriver Driver;
        public void SetValue(ChromeDriver driver, string ElementId)
        {
            this.Driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            Element = wait.Until(drv => drv.FindElement(By.CssSelector($"#{ElementId}")));
            Element.Click();

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
            inputs = wait.Until(drv => drv.FindElements(By.CssSelector($"input[class*='CheckAllItems']")));
            var checkedall = false;
            if (inputs.Count() > 0)
            {
                var element = inputs[0];
                var ischecked = element.GetAttribute("checked");
                if ((element.GetAttribute("checked") ?? "") != "true") 
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element); 
                checkedall = true;
            }
            if (!checkedall)
            {
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
                inputs = wait.Until(drv => drv.FindElements(By.CssSelector($".rcbSlide .RadComboBoxDropDown ul li input")));
                var elements = (from i in inputs
                                where i.Enabled && i.Displayed
                                select i).ToList();
                this.SelectFromDropDown(elements);
            }
            driver.FindElement(By.TagName($"body")).Click();
        }
        private void SelectFromDropDown(IList<IWebElement> inputs)
        {
            foreach (var element in inputs)
            {
                IWebElement e = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(5)).Until(
                      dvr => (element.Displayed) ? element : null);
                if (element.Enabled)
                {
                    if ((element.GetAttribute("checked") ?? "") != "true")
                    {
                        ((IJavaScriptExecutor)this.Driver).ExecuteScript("arguments[0].click();", element);
                        break;
                    }
                }
            } 
        }
    }
}