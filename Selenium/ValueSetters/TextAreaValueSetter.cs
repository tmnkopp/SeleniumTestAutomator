using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CyberScope.Tests.Selenium
{
    [ValueSetterMeta(Selector = "textarea")]
    public class TextAreaValueSetter : BaseValueSetter, IValueSetter
    {  
        public void SetValue(ChromeDriver driver, string ElementId)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            IWebElement Element = wait.Until(drv => drv.FindElement(By.CssSelector($"#{ElementId}")));
            if (!Overwrite && Element.Text != "")
                return; 
            Element.Clear();
            foreach (var item in Defaults.EmptyIfNull())
            { 
                if (Regex.Match(this.GetMatchAttribute(Element), item.Key, RegexOptions.IgnoreCase).Success)
                    Element.SendKeys(item.Value);
            }
            if (Element.GetAttribute("value") == "")
                Element.SendKeys(Element.GetAttribute("id")); // "" element.GetAttribute("id")
        }
    }
}