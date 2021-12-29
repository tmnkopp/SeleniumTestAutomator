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
using System.Diagnostics;
using NCalc;
using Xunit.Sdk;
using System.Reflection;
using System.IO;
using CyberScope.Tests.Selenium.Providers;

namespace CyberScope.Tests.Selenium.Datacall.Tests
{ 
    public class DataCall
    {
        #region FIELDS 
        ILogger _logger;
        private readonly ITestOutputHelper output;
        #endregion

        #region CTOR 
        public DataCall(ITestOutputHelper output)
        {
            this.output = output;
            _logger = new LoggerConfiguration()
            .WriteTo.TestOutput(output, LogEventLevel.Verbose)
            .CreateLogger(); 
        }
        #endregion
  
        #region UNITTESTS 
        [Fact] 
        public void Service_Resolves()
        {
            var ds = new Selenium.DriverService(_logger);
            ds.CsConnect(UserContext.Agency).ToTab("CIO 2022 Q1"); 
            ds.TestSections(qg => Regex.IsMatch(qg.SectionText, $"7D")); 
        }
        [Theory] 
        [InlineData("CIO 2022 Q1", "S2")] 
        public void DataCall_Resolves(string TabText, string SectionPattern)
        { 
            var ds = new Selenium.DriverService(_logger);
            ds.CsConnect(UserContext.Agency).ToTab(TabText); 
            ds.TestSections(qg => Regex.IsMatch(qg.SectionText, $"{SectionPattern}")); 

        } 
        [Theory] 
        [CsvData(@"")]
        public void Validate(string Section, string metricXpath, string attempt, string expected )
        {
            DriverService ds = new DriverService(_logger);
            ds.CsConnect(UserContext.Agency).ToTab("CIO 2022 Q1").ToSection((g => g.SectionText.Contains($"{Section}")));
            var metrics = new CIOMetricProvider();
            metrics.Populate(ds); 
            attempt = metrics.Eval<string>(attempt); 

            ds.FismaFormEnable();
            ds.SetFieldValue(By.XPath(metricXpath), attempt);
            ds.FismaFormSave();

            var actual = ds.GetFieldValue(By.XPath("//span[contains(@id, '_lblError')]")) ?? "";

            if (string.IsNullOrEmpty(expected))  {
                Assert.Equal(expected, actual);
            }
            else {
                Assert.Contains(expected, actual);
            }
             
            ds.FismaFormCancel(); 
            ds.Driver.Quit();
        } 
        #endregion 

        #region PRIVS  
        [Theory]
        [CsvData(@"C:\temp\DataCall_Validate.csv")]
        public void TestCSV(string a, string b, string c, string d)
        {
            var cda = new CsvDataAttribute(@"C:\temp\DataCall_Validate.csv");
            var a1 = a; 
        } 
        #endregion 
    }
    public class CIOMetricProvider : MetricProvider
    {
        public override void Populate(DriverService ds)
        {
            var url = ds.Driver.Url;
            ((IJavaScriptExecutor)ds.Driver).ExecuteScript("window.open();");
            var handles = ds.Driver.WindowHandles;
            ds.Driver.SwitchTo().Window(handles[ds.Driver.WindowHandles.Count - 1]);
            ds.Driver.Navigate().GoToUrl($"{url}");
            ds.ToSection((g => g.SectionText.Contains("S1")));

            string m111 = ds.GetFieldValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblfirst_Total')]")) ?? "0";
            string m112 = ds.GetFieldValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblSecond_Total')]")) ?? "0";
            string m115 = ds.GetFieldValue(By.XPath("//td/span[contains(text(), '1.1.5')]/../..//*[contains(@class, 'CustomControlValue')]")) ?? "0";
            SetMetric("qid_111_111", m111);
            SetMetric("qid_111_112", m112);
            SetMetric("qid_1_1_5", m115);
            int sum111_112 = this.GetMetric<int>("qid_111_111") + this.GetMetric<int>("qid_111_112"); 
            SetMetric("sum_111_112", sum111_112.ToString());

            ((IJavaScriptExecutor)ds.Driver).ExecuteScript("window.close();");
            ds.Driver.SwitchTo().Window(handles[ds.Driver.WindowHandles.Count - 1]);

            base.Populate(ds);
        }
    }
}
