using Microsoft.Extensions.FileSystemGlobbing;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
namespace CyberScope.Automator
{
    [ValueSetterMeta( Selector= "input[type='text']:not([type='readonly']):not([type='hidden']):not([data-role='datepicker'])")]
    public class TextInputValueSetter : BaseValueSetter, IValueSetter
    { 
        public void SetValue(SessionContext sessionContext, string ElementId) {

            var driver = sessionContext.Driver;
            string id = ElementId;
            IWebElement Element = new WebDriverWait(driver, TimeSpan.FromSeconds(.5))
                .Until(drv => drv.FindElement(By.CssSelector($"input[id='{id}']")));
            Element.Clear();
            Element = new WebDriverWait(driver, TimeSpan.FromSeconds(.5))
                .Until(drv => drv.FindElement(By.CssSelector($"input[id='{id}']")));
            string matchAttr = this.GetMatchAttribute(Element);
            sessionContext.Logger.Verbose("{o}", new { source = "TextInputValueSetter", ElementId = ElementId, matchAttr = matchAttr  });
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
                    Element.Clear();
                    var value = Utils.ReverseRegex(item.Value);
                    Element.SendKeys(value);
                }
                
            }

            Element = new WebDriverWait(driver, TimeSpan.FromSeconds(.5))
                .Until(drv => drv.FindElement(By.CssSelector($"input[id='{id}']")));
            var val = Element.GetAttribute("value");
            if (val == "^$") {
                Element.Clear();
            } else if (val == "") {
                Element.SendKeys("0");
            }
            base.Log(sessionContext, ElementId);
        }  
    }
}