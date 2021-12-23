using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace CyberScope.Tests.Selenium
{ 
    [ValueSetterMeta(Selector = "input[type='radio']")]
    public class RadioValueSetter : BaseValueSetter, IValueSetter
    { 
        public void SetValue(ChromeDriver driver, string ElementId)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            IWebElement Element = wait.Until(drv => drv.FindElement(By.CssSelector($"#{ElementId}"))); 
            bool matched = false;
            string onclick = Element.GetAttribute("onclick") ?? "";
            // if (onclick.Contains("__doPostBack"))
            // {
            //     Element.Click();
            //     return;
            // }
            foreach (var item in Defaults.EmptyIfNull())
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
            if (!matched) {
                Element.Click();
            } 
        }
    }
}