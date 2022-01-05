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
        [Theory]
        [InlineData("CIO 2022 Q1", "S1A|S1C|4")] // S1A|S1C|
        public void Initialize(string TabText, string SectionPattern)
        {
            var ds = new Selenium.DriverService(_logger);
            ds.CsConnect(UserContext.Agency).ToTab(TabText);  
            ds.TestSections(qg => Regex.IsMatch(qg.SectionText, $"{SectionPattern}"));
            ds.Driver.Quit();
        }
        [Theory] 
        [CsvData(@"C:\temp\CIO_Validate.csv")]
        public void Validate(string Section, string metricXpath, string attempt, string expected )
        {
            var ds = new DriverService(_logger);
            ds.CsConnect(UserContext.Agency)
                .ToTab("CIO 2022 Q1") // CIO 2022 Q1
                .ToSection((g => g.SectionText.Contains($"{Section}"))); 

            var metrics = new CIOMetricProvider();
            metrics.Populate(ds);
            attempt = metrics.Eval<string>(attempt);

            var Defaults = new DefaultInputProvider(ds.Driver).DefaultValues;
            Defaults.Add(metricXpath, attempt);

            var sc = new SessionContext(ds.Logger, ds.Driver, Defaults); 
            var pcc = ds.PageControlCollection().EmptyIfNull();

            ds.FismaFormEnable(); 
            string id = Utils.ExtractContainerId(ds.Driver, metricXpath); 
            foreach (IAutomator control in pcc) {
                if (!string.IsNullOrEmpty(id))  
                    ((IAutomator)control).ContainerSelector = $"#{id} ";  
                ((IAutomator)control).Automate(sc);
            }
            ds.FismaFormSave();

            var actual = ds.GetFieldValue(By.XPath("//*[contains(@id, 'Error')]")) ?? "";
            Assert.Contains(expected, actual);
             
            ds.Driver.Quit();
        }
        [Theory]
        [CsvData(@"C:\temp\EINSTEIN_Validate.csv")]
        public void EINSTEIN_Validate(string Section, string metricXpath, string attempt, string expected)
        {
            // string ToTab
            // string Section
            // MetricAnswerProvider
            // string metricXpath
            // string attempt
            // string expected
            var ds = new DriverService(_logger);
            ds.CsConnect(UserContext.Agency)
                .ToTab("EINSTEIN")
                .ToSection((g => g.SectionText.Contains($"{Section}")));

            var metrics = new MetricAnswerProvider();
            metrics.Populate(ds);
            attempt = metrics.Eval<string>(attempt);

            var Defaults = new DefaultInputProvider(ds.Driver).DefaultValues;
            Defaults.Add(metricXpath, attempt);

            var sc = new SessionContext(ds.Logger, ds.Driver, Defaults);

            ds.FismaFormEnable();
            var pcc = ds.PageControlCollection().EmptyIfNull();
            foreach (IAutomator control in pcc)
                ((IAutomator)control).Automate(sc);
            ds.FismaFormSave();

            var actual = ds.GetFieldValue(By.XPath("//*[contains(@id, 'Error')]")) ?? "";
            Assert.Contains(expected, actual);
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


}
