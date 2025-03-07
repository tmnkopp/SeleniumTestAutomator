using AngleSharp.Dom;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CyberScope.Automator
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
            ((IJavaScriptExecutor)this.driver).ExecuteScript("arguments[0].click();", Element);
            double waitDefault = 2;
            if (SettingsProvider.appSettings.ContainsKey("RadDropDownListValueSetterWait")) {
                waitDefault= Convert.ToDouble(SettingsProvider.appSettings["RadDropDownListValueSetterWait"]??"2");
            }
            
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(waitDefault); 
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitDefault));
            inputs = wait.Until(drv => drv.FindElements(By.CssSelector($".rddlPopup_Default ul li")));
  
            inputs = (from i in inputs
                      where i.Enabled && i.Displayed && !i.Text.StartsWith("--")
                      select i).ToList();
            var elements = (from i in inputs select i).SkipWhile(o => o.Text == "").ToList();
             
            this.SelectFromDropDown(elements); 
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.01);
            base.Log(sessionContext, ElementId);
        }
        private void SelectFromDropDown(IList<IWebElement> inputs)
        {
            foreach (var element in inputs)
            {
                IWebElement e = new WebDriverWait(this.driver, TimeSpan.FromSeconds(5)).Until(
                      dvr => (element.Displayed) ? element : null);
                var checkedAttr = element.GetAttribute("checked");
                if (element.Enabled && (element.GetAttribute("checked") ?? "") != "true")
                {
                    ((IJavaScriptExecutor)this.driver).ExecuteScript("arguments[0].click();", element);
                    break;
                } 
            } 
        }
    }
}