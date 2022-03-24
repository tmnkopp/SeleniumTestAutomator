using OpenQA.Selenium;
using System;
using System.Configuration;
using System.Threading;

namespace CyberScope.Tests.Selenium
{
    [ConnectorMeta(@"stag.*gov")]
    public class StageConnector : IConnect
    {
        public void Connect(SessionContext context)
        {
            var driver = context.Driver;
            driver.Navigate().GoToUrl(ConfigurationManager.AppSettings.Get("CSTargerUrl"));
            driver.FindElement(By.XPath($"//a[contains(@href, 'cyberscope-staging')]")).Click();
            Thread.Sleep(10 * 1000);
            driver.FindElement(By.XPath($"//input[contains(@id, 'btn_Accept')]")).Click(); 
            driver.FindElement(By.XPath($"//img[contains(@class, 'navbar-brand')]")).Click();
        }
    } 
}
