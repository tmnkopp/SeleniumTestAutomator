using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CyberScope.Tests.Selenium
{
    [ValueSetterMeta( Selector="input[type='text']:not([readonly='readonly'])")]
    public class TextInputValueSetter : BaseValueSetter, IValueSetter
    { 
        public void SetValue(ChromeDriver driver, string ElementId) {

            string id = ElementId;
            IWebElement Element = new WebDriverWait(driver, TimeSpan.FromSeconds(2))
                .Until(drv => drv.FindElement(By.CssSelector($"input[id='{id}']")));
  
            Element.Clear();
            Element = new WebDriverWait(driver, TimeSpan.FromSeconds(2))
                .Until(drv => drv.FindElement(By.CssSelector($"input[id='{id}']")));
 
            foreach (var item in Defaults.EmptyIfNull())
            {
                if (Regex.Match(this.GetMatchAttribute(Element), item.Key, RegexOptions.IgnoreCase).Success) {
                    Element.Clear();
                    Element.SendKeys(item.Value);
                }     
            }
            Element = new WebDriverWait(driver, TimeSpan.FromSeconds(1))
                .Until(drv => drv.FindElement(By.CssSelector($"input[id='{id}']")));
            if (Element.GetAttribute("value") == "")
            {
                Element.SendKeys("0");
            }
        }
    }
}
