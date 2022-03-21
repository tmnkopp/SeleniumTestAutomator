using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SeleniumExtras.WaitHelpers;
using System.Text.RegularExpressions;
using CyberScope.Tests.Selenium;
using System.Drawing.Imaging;
using static CyberScope.Tests.Selenium.DriverService;
using Serilog;
using Serilog.Events;
using Xunit.Abstractions;
using System.IO; 
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace CyberScope.Tests.Selenium.Datacall.Tests
{ 
    public class BOD1802 : BaseDataCallTest
    {
        #region FIELDS  
        WebDriverWait wait;
        #endregion

        #region CTOR 
        public BOD1802(ITestOutputHelper output): base(output)
        { 
        }
        #endregion

        #region PRIV 

        private void AppErrorHandler(DriverServiceEventArgs e)
        {
            string fnam = Regex.Replace(e.Section.SectionText + e.Driver.Title, $"[^A-Za-z0-9]", "");
            if (fnam.Length > 245) fnam = fnam.Substring(0, 245);
            var path = Environment.GetEnvironmentVariable("TMP", EnvironmentVariableTarget.User);
            e.Driver.GetScreenshot().SaveAsFile($"{path}/{fnam}.png", ScreenshotImageFormat.Png);
        }
        #endregion
        
        
        [Fact] 
        public void Assessment_Resolves()
        { 
            var ds = new Selenium.DriverService(_logger);
            ds.CsConnect(UserContext.Agency); 
            ds.OnApplicationError += (s, e) => {  
                AppErrorHandler(e); 
                Assert.True(false); 
            };
             
            wait = new WebDriverWait(ds.Driver, TimeSpan.FromSeconds(2));
            wait.Until(drv => drv.FindElement(By.XPath($"//ul[@class='rtsUL']//li//span[contains(text(), 'BOD 18-02')]"))).Click();

            NewTab(ds);
            ds.Driver.FindElement(By.XPath($"//*[contains(@id, '_lnkAddAssm')]")).Click();
             
            NewTab(ds);  
            wait.Until(drv => drv.FindElement(By.XPath($"//td[contains(@class, 'rgExpandCol')]"))).Click(); 

            NewTab(ds);  
            wait = new WebDriverWait(ds.Driver, TimeSpan.FromSeconds(2));
            wait.Until(drv => drv.FindElement(By.XPath($"//input[contains(@id, 'EditButton')]"))).Click();  
        }
     
        private void NewTab(DriverService ds)
        { 
            var url = ds.Driver.Url;
            ((IJavaScriptExecutor)ds.Driver).ExecuteScript("window.open();");
            var handles = ds.Driver.WindowHandles;
            ds.Driver.SwitchTo().Window(handles[ds.Driver.WindowHandles.Count - 1]);
            ds.Driver.Navigate().GoToUrl($"{url}");
        }
    } 
}
