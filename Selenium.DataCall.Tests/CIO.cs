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
        [InlineData("qid_2_1", "9999", "cannot exceed the value of 1.1.1+1.1.2", "0")]  
        [InlineData("qid_2_2", "9999", "cannot exceed the value of 1.1.1+1.1.2", "0")]
        [InlineData("qid_2_3", "9999", "cannot exceed the value of 1.1.1+1.1.2", "1")]
        public void S2_Conditional(string qid, string attempt, string expected, string finalValue)
        { 
            DriverService ds = new DriverService(_logger);
            ds.CsConnect(UserContext.Agency).ToTab("CIO 2022 Q1").ToSection((g => g.SectionText.Contains("S2")));
       
            ds.FismaFormEnable(); 
            ds.SetFieldValue(  By.XPath($"//input[contains(concat(' ', @class, ' '), ' {qid} ')]"), attempt);  
            ds.FismaFormSave(); 
              
            var actual = ds.GetFieldValue(By.XPath("//span[contains(@id, '_lblError')]")) ?? "";
            Assert.Contains(expected.Replace(" ", ""), actual.Replace(" ", "")); 
            ds.FismaFormCancel();

            ds.FismaFormEnable();
            ds.SetFieldValue(By.XPath($"//input[contains(concat(' ', @class, ' '), ' {qid} ')]"), finalValue);
            ds.FismaFormSave();
 
            actual = ds.GetFieldValue(By.XPath("//span[contains(@id, '_lblError')]")) ?? "";
            Assert.Equal("", actual);

            ds.Driver.Quit();
        }
        private int? _SUM_111_112; 
        private int GetSum_111_112(DriverService ds) { 
            ds.ToSection((g => g.SectionText.Contains("S1")));
            if (this._SUM_111_112 == null)
            {
                string m111 = ds.GetFieldValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblfirst_Total')]")) ?? "0";
                string m112 = ds.GetFieldValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblSecond_Total')]")) ?? "0";
                this._SUM_111_112 = Convert.ToInt32(m111) + Convert.ToInt32(m112);
            } 
            return this._SUM_111_112 ?? 0;
        }
        #endregion
    } 
}
