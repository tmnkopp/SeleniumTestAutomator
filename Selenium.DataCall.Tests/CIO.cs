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
using Serilog;
using Xunit.Abstractions;
using Serilog.Events;
using NCalc;
using System.Collections;
using CyberScope.Tests.Selenium.Providers;

namespace CyberScope.Tests.Selenium.Datacall.Tests
{
    public class CIO
    {
        ILogger _logger;
        private readonly ITestOutputHelper output;
        public CIO(ITestOutputHelper output)
        {
            this.output = output;
            _logger = new LoggerConfiguration()
            .WriteTo.TestOutput(output, LogEventLevel.Verbose)
            .CreateLogger();
        }

        #region UNITTESTS 

        [Theory]
        [InlineData("CIO 2022 Q1", ".*")]
        public void Initialize(string _PK_FORM, string SectionPattern)
        {
            var ds = new Selenium.DriverService(_logger);
            var driver = ds.CsConnect(UserContext.Agency).ToTab(_PK_FORM).Driver;
            ds.TestSections(qg => Regex.IsMatch(qg.SectionText, $"{SectionPattern}"));

            //Assert.True(ds.FismaFormValidates());
            //ds.Driver.Quit(); 
        }
        [Theory]
        [CsvData(@"C:\temp\CIO_Validate.csv")]
        public void Validate(string Section, string metricXpath, string attempt, string expected)
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
            foreach (IAutomator control in pcc)
            {
                if (!string.IsNullOrEmpty(id)) ((IAutomator)control).ContainerSelector = $"#{id} ";
                ((IAutomator)control).Automate(sc);
            }
            ds.FismaFormSave();

            var actual = ds.GetElementValue(By.XPath("//*[contains(@id, 'Error')]")) ?? "";
            Assert.Contains(expected, actual);

            ds.Driver.Quit();
        }
        #endregion
    }
}
