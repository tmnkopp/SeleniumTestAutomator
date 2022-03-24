using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CyberScope.Tests.Selenium
{
    [ValueSetterMeta( Selector="input[type='text']:not([readonly='readonly'])")]
    public class TextInputValueSetter : BaseValueSetter, IValueSetter
    { 
        public void SetValue(SessionContext sessionContext, string ElementId) {

            var driver = sessionContext.Driver;
            string id = ElementId;
            IWebElement Element = new WebDriverWait(driver, TimeSpan.FromSeconds(1))
                .Until(drv => drv.FindElement(By.CssSelector($"input[id='{id}']")));
  
            Element.Clear();
            Element = new WebDriverWait(driver, TimeSpan.FromSeconds(1))
                .Until(drv => drv.FindElement(By.CssSelector($"input[id='{id}']")));

            string matchAttr = this.GetMatchAttribute(Element); 
            foreach (var item in sessionContext.Defaults.EmptyIfNull())
            {
                string MatchKey = item.Key;
                if (MatchKey.StartsWith("//"))
                {
                    var element = new WebDriverWait(driver, TimeSpan.FromSeconds(.25))
                        .Until(drv => drv.FindElements(By.XPath(MatchKey))).FirstOrDefault();
                    MatchKey = element?.GetAttribute("id") ?? "^$";
                } 
                if (Regex.Match(matchAttr, MatchKey, RegexOptions.IgnoreCase).Success) {
                    sessionContext.Logger.Information($"TextInputValueSetter MatchKey: {MatchKey}  matchAttr:{matchAttr} ");
                    Element.Clear();
                    Element.SendKeys(item.Value);
                }     
            }
            Element = new WebDriverWait(driver, TimeSpan.FromSeconds(1))
                .Until(drv => drv.FindElement(By.CssSelector($"input[id='{id}']")));
            var val = Element.GetAttribute("value");
            if (val == "^$") {
                Element.Clear();
            } else if (val == "") {
                Element.SendKeys("0");
            }
        } 
    }
}
