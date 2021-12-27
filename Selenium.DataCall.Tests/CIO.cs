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
        [InlineData("CIO 2022 Q1", "1A")]
        public void TestSections(string _PK_FORM, string SectionPattern)
        {
            var ds = new Selenium.DriverService(_logger);
            var driver = ds.CsConnect(UserContext.Agency).ToTab(_PK_FORM).Driver; 
            ds.TestSections(qg => Regex.IsMatch(qg.SectionText, $"{SectionPattern}"));
            
            //Assert.True(ds.FismaFormValidates());
            //ds.Driver.Quit(); 
        } 
        [Theory] 
        [InlineData("qid_6_1", "9999", "6.1 Should not exceed the total of H, M")] 
        [InlineData("qid_6_2", "9999", "6.2 Should not exceed the total of H, M")] 
        [InlineData("qid_6_3", "9999", "6.3 Should not exceed the total of H, M")] 
        [InlineData("qid_6_5", "9999", "6.5 Should not exceed the total of H, M")] 
        [InlineData("qid_6_6", "9999", "6.6 Should not exceed the total of H, M")] 
        [InlineData("qid_6_7", "9999", "6.7 Should not exceed the total of H, M")] 
        [InlineData("qid_6_7_1", "9999", "6.7.1 should not exceed 6.7")] 
        [InlineData("qid_6_8", "9999", "6.8 Should not exceed the total of H, M")] 
        [InlineData("qid_6_9", "9999", "6.9 Should not exceed the total of H, M")] 
        [InlineData("qid_6_10_1", "9999", "6.10.1 should not exceed 6.10")] 
        [InlineData("qid_6_10_2", "9999", "6.10.2 should not exceed (6.10-6.10.1)")] 
        [InlineData("qid_6_10_3", "9999", "6.10.3 should not exceed 6.10")] 
        [InlineData("qid_6_10_4", "9999", "6.10.4 should not exceed 6.10")] 
        [InlineData("qid_6_11", "9999", "6.11 should not exceed 6.9")] 
        public void S6A_Conditional(string qid, string attempt, string expected)
        { 
            DriverService ds = new DriverService(_logger);
            ds.CsConnect(UserContext.Agency).ToTab("CIO 2022 Q1").ToSection((g => g.SectionText.Contains("S6A")));
       
            ds.FismaFormEnable(); 
            ds.SetFieldValue(  By.XPath($"//input[contains(concat(' ', @class, ' '), ' {qid} ')]"), attempt);  
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
