using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Configuration;
namespace CyberScope.Automator
{
    [ConnectorMeta("localhost|CyberScopeBranch")]
    public class DefaultCsConnector : IConnect
    {
        public void Connect(SessionContext context)
        {
            var driver = context.Driver;
            var userContext = context.userContext;
            var user = SettingsProvider.appSettings[$"{userContext.ToString()}User"]; 
            var pass = SettingsProvider.appSettings[$"{userContext.ToString()}Pass"];  
            driver.Url = SettingsProvider.appSettings[$"CSTargerUrl"]; 
            driver.FindElementByCssSelector("input[id$=Login1_UserName]").SendKeys(user);
            driver.FindElementByCssSelector("input[id$=Login1_Password]").SendKeys(pass);
            driver.FindElementByCssSelector("input[id$=Login1_LoginButton]").Click();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            IWebElement ele = wait.Until(drv => drv.FindElement(By.CssSelector($"input[id$=ctl00_ContentPlaceHolder1_btn_Accept]")));
            ele.Click();
            if (userContext == UserContext.Agency)
            {
                ele = wait.Until(drv => drv.FindElement(By.CssSelector($"img[id*=ctl00_AgencyNav]")));
                ele.Click();
            } 
        }
    } 
}