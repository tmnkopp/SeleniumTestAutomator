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
    public class BOD1802
    {
        #region FIELDS 
        ILogger _logger;
        private readonly ITestOutputHelper output;
        WebDriverWait wait;
        #endregion

        #region CTOR 
        public BOD1802(ITestOutputHelper output)
        {
            this.output = output;
            _logger = new LoggerConfiguration()
            .WriteTo.TestOutput(output, LogEventLevel.Verbose)
            .WriteTo.File(@"d:\logs\log.txt",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true)
            .CreateLogger();


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
        public void Logger_Resolves()
        {
            string raw = File.ReadAllText(@"d:\logs\log202201191240.txt");
            MatchCollection matches = Regex.Matches(raw, @"\[WRN\].*(\{.+InvalidElementStateException.+)");
            raw = $"[{ string.Join(",", (from Match m in matches select m.Groups[1].Value).ToList()) }]"; 

            string xpath = "//*[@id='ctl00_ContentPlaceHolder1_CBButtPanel1_btnEdit']";
            dynamic json = JsonConvert.DeserializeObject(raw); 
            dynamic query =  (from j in ((JArray)json)
                              where j["xpath"].ToString() == xpath
                             select j).FirstOrDefault() ;
            var item =  query.method.Value ;
            // return ((JArray)json).Select(i => (string)i).ToList();
        }
        #region UNITTESTS  
        [Fact]
        public void HvaAnnual_Resolves(){
            var ds = new Selenium.DriverService(_logger);
            ds.CsConnect(UserContext.Agency);
            ds.ToTab("BOD 18-02 Annual 2021");
            
            wait = new WebDriverWait(ds.Driver, TimeSpan.FromSeconds(2));
            var xpaths = new string[] {
                  "//*[@id='ctl00_ContentPlaceHolder1_CBButtPanel1_btnEdit']"
                , "//*[contains(@id, 'ctl00_ContentPlaceHolder1_CBYesNo_10_SelectionList_1')]"
                , "//*[@id='ctl00_ContentPlaceHolder1_CBtext7_WebTextEdit1']"
            };
            var mtx = new List<dynamic>(); 
            mtx.Add(new {xpath = "//*[contains(@id, 'ctl00_ContentPlaceHolder1_CBYesNo_10_SelectionList_1')]", param="Y" });
            mtx.Add(new { xpath = "//*[@id='ctl00_ContentPlaceHolder1_CBButtPanel1_btnEdit']" });
            mtx.Add(new {xpath = "//*[@id='ctl00_ContentPlaceHolder1_CBtext7_WebTextEdit1']", param = "test" });

            foreach (dynamic item in mtx)
            {
                var xp = item.xpath;
                var p = item.param;
            }
            foreach (var xpath in xpaths)
            {
                IWebElement elm = wait.Until(drv => drv.FindElement(By.XPath($"{xpath}"))); 
                foreach (var item in new string[] { "Clear", "SendKeys", "Click" })
                {
                    try
                    { 
                        MethodInfo mi = elm.GetType().GetMethod(item);
                        if (mi.GetParameters().Count() > 0) {
                            mi.Invoke(elm, new object[]{ xpath });
                        }else{
                            mi.Invoke(elm, null);
                        }   
                    }
                    catch (TargetInvocationException ex)
                    {
                        _logger.Warning($"{{@model}}", new { element_tag= elm.TagName, element_id = elm.GetAttribute("id"), method = item, xpath = xpath, exception = ex.InnerException.Message});
                    }
                    catch (Exception )
                    {
                        throw;
                    }
                }  
            }

        }

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
        #endregion
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
