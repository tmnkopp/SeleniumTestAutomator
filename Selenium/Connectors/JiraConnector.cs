using OpenQA.Selenium;
using System;
using System.Configuration;
using System.Threading;

namespace CyberScope.Tests.Selenium
{
    [ConnectorMeta(@"jira.*jsp")]
    public class JiraConnector : IConnect
    {
        public void Connect(SessionContext context)
        {
            var driver = context.Driver;
            driver.Navigate().GoToUrl(ConfigurationManager.AppSettings.Get("CSTargerUrl"));
            var user = ConfigurationManager.AppSettings.Get("CBUser") ?? "";
            var pass = ConfigurationManager.AppSettings.Get("CBPass") ?? "";
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass)) 
            {
                driver.FindElement(By.XPath($"//input[contains(@id, 'username')]")).SendKeys(user);
                driver.FindElement(By.XPath($"//input[contains(@id, 'password')]")).SendKeys(pass);
                driver.FindElement(By.XPath($"//input[contains(@id, 'login-form-submit')]")).Click();
            } else { 
                var ele = driver.FindElement(By.XPath($"//*[contains(@class, 'header-main')]")); 
                ((IJavaScriptExecutor)driver).ExecuteScript("var ele=arguments[0]; ele.innerHTML = 'app.config for automated jira login:<br>&lt;add key=CBuser value=my_jira_username /&gt;<br> &lt;add key=CBpass value=my_secret_jira_password /&gt;';", ele);

                Thread.Sleep(15 * 1000);
            }
            
        }
    }

}
