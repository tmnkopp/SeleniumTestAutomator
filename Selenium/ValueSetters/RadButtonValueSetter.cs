using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace CyberScope.Tests.Selenium
{
    [ValueSetterMeta(Selector = "button[class*='RadRadioButton']")]
    public class RadButtonValueSetter : BaseValueSetter, IValueSetter
    { 
        public void SetValue(SessionContext sessionContext, string ElementId)
        {
            var driver = sessionContext.Driver;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            IWebElement Element = wait.Until(drv => drv.FindElement(By.CssSelector($"#{ElementId}"))); 
            bool matched = false;
            foreach (var item in sessionContext.Defaults.EmptyIfNull())
            {
                if (Regex.Match(this.GetMatchAttribute(Element), item.Key, RegexOptions.IgnoreCase).Success)
                {
                    matched = true;
                    if (Element.GetAttribute("value").ToUpper() == item.Value.ToUpper())
                    {
                        Element.Click();
                        break;
                    }
                }
            }
            if (!matched) Element.Click();
        }
    }
}