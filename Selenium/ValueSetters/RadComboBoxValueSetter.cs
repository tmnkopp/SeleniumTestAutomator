using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberScope.Tests.Selenium
{
    [ValueSetterMeta(Selector = "div[class*='RadComboBox']")]
    public class RadComboBoxValueSetter : BaseValueSetter, IValueSetter
    {
        private ChromeDriver Driver;
        public void SetValue(ChromeDriver driver, string ElementId)
        {
            this.Driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            IWebElement Element = wait.Until(drv => drv.FindElement(By.CssSelector($"#{ElementId}")));
            Element.Click();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);

            var itemschecked = false;

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            inputs = wait.Until(drv => drv.FindElements(By.CssSelector($"*[id*='{ElementId}'] *[class='rcbCheckAllItemsCheckBox']")));

            if (inputs.Count() > 0 && !itemschecked)
            {
                var element = inputs[0];
                var ischecked = element.GetAttribute("checked") ?? "";
                if (ischecked  != "true" && element.Displayed) {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
                    itemschecked = true;
                } 
            }

            var rcbSlides = new string[] { "input", "" };

            foreach (var item in rcbSlides)
            {
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
                inputs = wait.Until(drv => drv.FindElements(By.CssSelector($".rcbSlide *[id*='{ElementId}'] ul li {item}")));

                if (inputs.Count() > 0 && !itemschecked)
                {
                    var elements = (from i in inputs
                                    where i.Enabled && i.Displayed
                                    select i).ToList();
                    this.SelectFromDropDown(elements);
                    itemschecked = true;
                }
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
                    var att_checked = element.GetAttribute("checked");
                    var att_txt = element.Text.Trim();
                    if ((att_checked ?? "") != "true" && !att_txt.StartsWith("Select"))
                    {
                        ((IJavaScriptExecutor)this.Driver).ExecuteScript("arguments[0].click();", element);
                        break;
                    }
                }
            } 
        }
    }
}