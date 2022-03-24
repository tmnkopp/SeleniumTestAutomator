using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CyberScope.Tests.Selenium
{
    [ValueSetterMeta(Selector = "select:not([id*='_ddl_Sections'])")]
    public class SelectValueSetter : BaseValueSetter, IValueSetter
    {
        public void SetValue(SessionContext sessionContext, string ElementId)
        {
            var driver = sessionContext.Driver;
            IWebElement Element = driver.FindElement(By.CssSelector($"#{ElementId}"));
            SelectElement sel = new SelectElement(Element);
            sel.SelectByIndex(sel.Options.Count - 1);
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
                if (Regex.Match(matchAttr, MatchKey, RegexOptions.IgnoreCase).Success && !string.IsNullOrEmpty(MatchKey))
                {
                    sel.SelectByText(item.Value);
                    break;
                }
            }  
        }
    }
}