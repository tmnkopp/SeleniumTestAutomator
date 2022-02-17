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

namespace CyberScope.Tests.Selenium.Datacall.Tests
{ 
    public class EINSTEIN
    {
        #region FIELDS 
        ILogger _logger;
        private readonly ITestOutputHelper output;
        #endregion

        #region CTOR 
        public EINSTEIN(ITestOutputHelper output)
        {
            this.output = output;
            _logger = new LoggerConfiguration()
            .WriteTo.TestOutput(output, LogEventLevel.Verbose)
            .CreateLogger();
        }
        #endregion

        #region UNITTESTS 
   
        [Theory]
        [CsvInitData()]
        public void Initialize(string Section)
        { 
            var ds = new Selenium.DriverService(_logger);
            ds.CsConnect(UserContext.Agency).ToTab("EINSTEIN"); 
            ds.InitSections(qg => Regex.IsMatch(qg.SectionText, $"{Section}"));

        }
        [Theory]
        [CsvData()]
        public void Validate(string Section, string metricXpath, string ErrorAttemptExpression, string ExpectedError)
        {
            var s = Section;
            var mxp = metricXpath;
            var eae = ErrorAttemptExpression;
            var ee = ExpectedError;

        }

        #endregion 
    }
}
