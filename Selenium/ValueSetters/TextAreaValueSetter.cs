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
        public void SetValue(SessionContext sessionContext, string ElementId)
        {
            var driver = sessionContext.Driver;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            IWebElement Element = wait.Until(drv => drv.FindElement(By.CssSelector($"#{ElementId}")));
            if (!Overwrite && Element.Text != "")
                return; 
            Element.Clear();
            foreach (var item in sessionContext.Defaults.EmptyIfNull())
            { 
                if (Regex.Match(this.GetMatchAttribute(Element), item.Key, RegexOptions.IgnoreCase).Success)
                    Element.SendKeys(item.Value);
            }
            var val = Element.GetAttribute("value");
            if (val == "^$")
            {
                Element.Clear();
            }
            else if (val == "")
            {
                Element.SendKeys(this.GetMatchAttribute(Element).Replace("_"," "));
            }  
        }
    }
}