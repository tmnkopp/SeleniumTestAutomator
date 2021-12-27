﻿using OpenQA.Selenium;
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

        #region PRIV 

        private void AppErrorHandler(DriverServiceEventArgs e)
        {
            string fnam = Regex.Replace(e.Section.SectionText + e.Driver.Title, $"[^A-Za-z0-9]", "");
            if (fnam.Length > 245) fnam = fnam.Substring(0, 245);
            var path = Environment.GetEnvironmentVariable("TMP", EnvironmentVariableTarget.User);
            e.Driver.GetScreenshot().SaveAsFile($"{path}/{fnam}.png", ScreenshotImageFormat.Png);
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
        [InlineData("CIO 2022 Q1", "1A")] 
        public void DataCall_Resolves(string TabText, string SectionPattern)
        { 
            var ds = new Selenium.DriverService(_logger);
            ds.CsConnect(UserContext.Agency).ToTab(TabText);

            string m111 = ds.GetFieldValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblfirst_Total')]")) ?? "0";
            string m112 = ds.GetFieldValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblSecond_Total')]")) ?? "0";
            int SUM_111_112 = Convert.ToInt32(m111) + Convert.ToInt32(m112);

            ds.OnSectionComplete += (s, dsvc) => { 
                dsvc.DriverService.Logger.Information($"Section Complete {dsvc.Section.SectionText}"); 
            };
            ds.TestSections(qg => Regex.IsMatch(qg.SectionText, $"{SectionPattern}")); 

        }
        #endregion 
    }
}