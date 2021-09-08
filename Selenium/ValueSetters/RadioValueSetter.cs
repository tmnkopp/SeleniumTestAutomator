using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace CyberScope.Tests.Selenium
{
    internal class RadioValueSetter : BaseValueSetter, IValueSetter
    {
        public void SetValue(ChromeDriver driver, string ElementId)
        {
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(.25));
            Element = wait.Until(drv => drv.FindElement(By.CssSelector($"#{ElementId}")));
            var target = this.GetMatchAttribute(); 
            bool matched = false;
            foreach (var item in Defaults.EmptyIfNull())
            {
                if (Regex.Match(target, item.Key, RegexOptions.IgnoreCase).Success)
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