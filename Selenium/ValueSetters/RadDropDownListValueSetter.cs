using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;


namespace CyberScope.Tests.Selenium
{
    [ValueSetterMeta(Selector = "*[class*='RadDropDownList']")]
    public class RadDropDownListValueSetter : BaseValueSetter, IValueSetter
    {
        private ChromeDriver driver;
        public void SetValue(SessionContext sessionContext, string ElementId)
        {
            this.driver = sessionContext.Driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            IWebElement Element = wait.Until(drv => drv.FindElement(By.CssSelector($"#{ElementId}")));
            Element.Click();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            inputs = wait.Until(drv => drv.FindElements(By.CssSelector($".rddlPopup_Default ul li")));
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            inputs = (from i in inputs
                      where i.Enabled && i.Displayed
                      select i).ToList();
            var elements = (from i in inputs select i).SkipWhile(o => o.Text == "").ToList();
            this.SelectFromDropDown(elements);
            driver.FindElement(By.TagName($"body")).Click();
        }
        private void SelectFromDropDown(IList<IWebElement> inputs)
        {
            foreach (var element in inputs)
            {
                IWebElement e = new WebDriverWait(this.driver, TimeSpan.FromSeconds(5)).Until(
                      dvr => (element.Displayed) ? element : null); 
                if (element.Enabled && (element.GetAttribute("checked") ?? "") != "true")
                {
                    ((IJavaScriptExecutor)this.driver).ExecuteScript("arguments[0].click();", element);
                    break;
                } 
            } 
        }
    }
}