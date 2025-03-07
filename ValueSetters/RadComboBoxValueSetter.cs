using AngleSharp.Dom;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
namespace CyberScope.Automator
{
    [ValueSetterMeta(Selector = "div[class*='RadComboBox']")]
    public class RadComboBoxValueSetter : BaseValueSetter, IValueSetter
    {
        private ChromeDriver driver;
        public void SetValue(SessionContext sessionContext, string ElementId)
        {
            this.driver = sessionContext.Driver;
            driver.FindElement(By.TagName($"body"))?.Click();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            IWebElement Element = wait.Until(drv => drv.FindElement(By.CssSelector($"#{ElementId}"))); 
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", Element);
            var inputs = wait.Until(drv => drv.FindElements(By.CssSelector($".rcbSlide *[id*='{ElementId}'] ul li input:not(:checked)")));
            if (inputs.Count < 1) return;

            var itemschecked = false;  
            try
            {
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
                inputs = wait.Until(drv => drv.FindElements(By.CssSelector($"*[id*='{ElementId}'] input[class*='rcbCheckAllItemsCheckBox']")));
                if (inputs.Count > 0)
                {
                    var element = inputs[0]; 
                    IWebElement e = new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(
                          dvr => (element.Displayed) ? element : null); 
                    if ((element?.GetAttribute("checked") ?? "") != "true")
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
                        itemschecked = true;
                    }
                } 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); 
            }

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            driver.FindElement(By.TagName($"body"));
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.001);

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2)); 
            inputs = wait.Until(drv => drv.FindElements(By.CssSelector($".rcbSlide *[id*='{ElementId}'] ul li input"))); 
            if (inputs.Count() > 0) // && !itemschecked)
            { 
                var attempts = 0;
                while ( attempts < 4) {
                    var elements = (from i in inputs where i.Enabled && i.Displayed select i).ToList();
                    this.SelectFromDropDown(elements);
                    itemschecked = true; 
                    attempts++; 
                } 
            }
            driver.FindElement(By.TagName($"body")).Click();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.01);
            base.Log(sessionContext, ElementId);
        } 
        private void SelectFromDropDown(IList<IWebElement> inputs)
        { 
            var index = inputs.Count()-1;
            while (index >= 0 ) { 
                var element = inputs[index];
                IWebElement parent = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].parentNode;", element);
                var att_disabled = element.GetAttribute("disabled") ?? "";
                if (string.IsNullOrEmpty(att_disabled)) {
                    IWebElement e = new WebDriverWait(this.driver, TimeSpan.FromSeconds(5)).Until(
                          dvr => (element.Displayed && element.Enabled) ? element : null);
                    if (element.Enabled)
                    {
                        var att_checked = element.GetAttribute("checked") ?? "";
                        var att_txt = element.Text.Trim();
                        if (att_checked != "true" && !att_txt.StartsWith("Select") && parent?.Text != "None")
                        {
                            ((IJavaScriptExecutor)this.driver).ExecuteScript("arguments[0].click();", element);
                            //break;
                        }
                    }
                } 
                index--;
            } 
        } 
    }
}