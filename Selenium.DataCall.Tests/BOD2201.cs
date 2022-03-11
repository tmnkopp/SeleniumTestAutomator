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
    public class BOD2201
    {
        #region FIELDS 
        ILogger _logger;
        private readonly ITestOutputHelper output;
        WebDriverWait wait;
        #endregion

        #region CTOR 
        public BOD2201(ITestOutputHelper output)
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
  
        [Theory]
        [InlineData("22-01", "S2")] 
        public void Initialize(string TabText, string SectionPattern)
        {
            var ds = new Selenium.DriverService(_logger);
            ds.CsConnect(UserContext.Agency).ToTab(TabText); 
            ds.InitSections(qg => Regex.IsMatch(qg.SectionText, $"{SectionPattern}"));
      
        }
    } 
}
