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
        public void SetValue(SessionContext sessionContext, string ElementId)
        {
            var driver = sessionContext.Driver;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(8));
            IWebElement Element = null;
            try
            {
                Element = wait.Until(drv => drv.FindElement(By.CssSelector($"#{ElementId}")));
            }
            catch (NoSuchElementException ex)
            { 
                sessionContext.Logger.Error($"RadioValueSetter NoSuchElementException {ElementId} {ex.Message} {ex.InnerException}"); 
            }
            catch (Exception ex)
            { 
                sessionContext.Logger.Error($"RadioValueSetter Exception {ElementId} {ex.Message} {ex.InnerException}");
                throw;
            }

            if (Element != null)
            {
                bool matched = false;
                string onclick = Element.GetAttribute("onclick") ?? "";

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
                if (!matched)
                {
                    Element.Click();
                }
            } 
        }
    }
}