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
using CyberScope.Tests.Selenium.DataCall.Tests;

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

        public CIO()
        {
            TestInitializer.InitIIS();
        }
        [Theory]
        [InlineData("2022 Q2", ".*")]
        public void Initialize(string _PK_FORM, string SectionPattern)
        {
            var ds = new Selenium.DriverService(_logger);
            var driver = ds.CsConnect(UserContext.Agency).ToTab(_PK_FORM).Driver;
            ds.InitSections(qg => Regex.IsMatch(qg.SectionText, $"{SectionPattern}")); 
            Assert.True(ds.FismaFormValidates());
            //ds.Driver.Quit(); 
        }
        [Theory]
        [CsvData()]
        public void Validate(string Section, string metricXpath, string ErrorAttemptExpression, string ExpectedError)
        {
            var va = new ValidationAttempt(metricXpath, ErrorAttemptExpression);
            var ds = new DriverService(_logger);
            ds.CsConnect(UserContext.Agency)
                .ToTab("CIO 2022 Q2")
                .ToSection((s => s.SectionText.Contains($"{Section}")))
                .ApplyValidationAttempt(va, () => {
                    var actualError = ds.GetElementValue(By.XPath("//*[contains(@id, 'Error')]")) ?? "";
                    Assert.Contains(ExpectedError, actualError);
                });
            ds.DisposeDriverService();
        }
        #endregion
    }
}
