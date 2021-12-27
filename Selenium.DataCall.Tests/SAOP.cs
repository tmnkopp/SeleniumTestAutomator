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

namespace CyberScope.Tests.Selenium.Datacall.Tests
{
    public class SAOP
    {
        ILogger _logger;
        private readonly ITestOutputHelper output;
        public SAOP(ITestOutputHelper output)
        { 
            this.output = output;
            _logger = new LoggerConfiguration()
            .WriteTo.TestOutput(output, LogEventLevel.Verbose)
            .CreateLogger();
        }
        
        #region UNITTESTS 
        [Theory]
        [InlineData("2021 Annual Report - SAOP", "8")]
        public void TestSections(string _PK_FORM, string SectionPattern)
        {
            var ds = new Selenium.DriverService(_logger);
            var driver = ds.CsConnect(UserContext.Agency).ToTab(_PK_FORM).Driver; 
            ds.TestSections(qg => Regex.IsMatch(qg.SectionText, $"{SectionPattern}"));  
        }
        [Theory]
        [InlineData("qid_2c", "9999", "2c cannot exceed the value of 2b")]  
        [InlineData("qid_2d", "9999", "2d cannot exceed the value of 2c")]  
        [InlineData("qid_2e", "9999", "2e cannot exceed the value of 2c")]  
        [InlineData("qid_2f", "9999", "2f cannot exceed the value of 2c")]  
        [InlineData("qid_2g", "9999", "2g cannot exceed the value of 2c")]  
        public void S6A_Conditional(string qid, string attempt, string expected)
        {
            DriverService ds = new DriverService(_logger);
            ds.CsConnect(UserContext.Agency).ToTab("2021 Annual Report - SAOP").ToSection((g => g.SectionText.Contains("2B")));
            var Q2B = ds.GetFieldValue(By.XPath("//table[contains(@class,'rgMasterTable')]//tbody//tr[last()]/td[7]/span"));
            var Q2C = ds.GetFieldValue(By.XPath("//table[contains(@class,'rgMasterTable')]//tbody//tr[last()]/td[10]/span"));

            ds.ToSection((g => g.SectionText.Contains("3B")));
            var Q3B = ds.GetFieldValue(By.XPath("//table[contains(@class,'rgMasterTable')]//tbody//tr[last()]/td[7]/span")); 
 

            ds.FismaFormEnable();
            ds.SetFieldValue(By.XPath($"//input[contains(concat(' ', @class, ' '), ' {qid} ')]"), attempt);
            ds.FismaFormSave();

            var actual = ds.GetFieldValue(By.XPath("//span[contains(@id, '_lblError')]")) ?? "";
            Assert.Contains(expected.Replace(" ", ""), actual.Replace(" ", ""));
            ds.FismaFormCancel();

            ds.FismaFormEnable();
            ds.SetFieldValue(By.XPath($"//input[contains(concat(' ', @class, ' '), ' {qid} ')]"), "0");
            ds.FismaFormSave();

            actual = ds.GetFieldValue(By.XPath("//span[contains(@id, '_lblError')]")) ?? "";
            Assert.Equal("", actual);

            ds.Driver.Quit();
        }
        #endregion
    } 
}
