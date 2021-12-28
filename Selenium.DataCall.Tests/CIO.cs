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
        public void TestSections(string _PK_FORM, string SectionPattern)
        {
            var ds = new Selenium.DriverService(_logger);
            var driver = ds.CsConnect(UserContext.Agency).ToTab(_PK_FORM).Driver;
            ds.TestSections(qg => Regex.IsMatch(qg.SectionText, $"{SectionPattern}"));

            //Assert.True(ds.FismaFormValidates());
            //ds.Driver.Quit(); 
        }

        #endregion
    }
}
