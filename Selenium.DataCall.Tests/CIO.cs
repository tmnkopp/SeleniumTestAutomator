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
        [InlineData("qid_2_1", "sum111_112 + 1", "cannot exceed the value of", "0")]
        [InlineData("qid_2_2", "sum111_112 + 1", "cannot exceed the value of", "0")]
        [InlineData("qid_2_3", "sum111_112 + 1", "cannot exceed the value of", "1")]
        [InlineData("qid_2_4", "( sum111_112 - qid_2_3 ) + 1", "cannot exceed the value of", "0")]
        public void S2_Conditional(string qid, string attempt, string expected, string finalValue)
        {
            DriverService ds = new DriverService(_logger);
            ds.CsConnect(UserContext.Agency).ToTab("CIO 2022 Q1").ToSection((g => g.SectionText.Contains("S2")));

            int sum111_112 = GetSum_111_112(ds);
            SetMetric("sum111_112", sum111_112.ToString());
            attempt = EvalAnswer(attempt);


            ds.FismaFormEnable();
            ds.SetFieldValue(By.XPath($"//input[contains(concat(' ', @class, ' '), ' {qid} ')]"), attempt);
            ds.FismaFormSave();

            var actual = ds.GetFieldValue(By.XPath("//span[contains(@id, '_lblError')]")) ?? "";
            Assert.Contains(expected.Replace(" ", ""), actual.Replace(" ", ""));
            ds.FismaFormCancel();

            ds.FismaFormEnable();
            ds.SetFieldValue(By.XPath($"//input[contains(concat(' ', @class, ' '), ' {qid} ')]"), finalValue);
            ds.FismaFormSave();

            actual = ds.GetFieldValue(By.XPath("//span[contains(@id, '_lblError')]")) ?? "";
            Assert.Equal("", actual);

            SetMetric(qid, finalValue);

            ds.Driver.Quit();
        }
        [Fact]
        public void Ncalc_Calculates(){
            Expression e = new Expression("17");
            var actual = e.Evaluate();
            Assert.Equal(17, actual);
        }
         
        #endregion

        #region PRIVS

        private Dictionary<string, string> _Answers = new Dictionary<string, string>();
        private void SetMetric(string key, string val) { 
            if (!_Answers.ContainsKey(key)) 
                _Answers.Add(key, val); 
        }
        private string EvalAnswer(string source)
        { 
            foreach (var kv in _Answers) 
                source = source.Replace($"{kv.Key} ", $"{_Answers[kv.Key]} "); 
            var evaled = new Expression(source).Evaluate();
            return Convert.ToString(evaled); 
        }

        private int? _SUM_111_112;
        private int GetSum_111_112(DriverService ds)
        {
            if (this._SUM_111_112 == null)
            {
                var url = ds.Driver.Url;
                ((IJavaScriptExecutor)ds.Driver).ExecuteScript("window.open();");
                var handles = ds.Driver.WindowHandles;
                ds.Driver.SwitchTo().Window(handles[ds.Driver.WindowHandles.Count - 1]);
                ds.Driver.Navigate().GoToUrl($"{url}");
                ds.ToSection((g => g.SectionText.Contains("S1")));
                string m111 = ds.GetFieldValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblfirst_Total')]")) ?? "0";
                string m112 = ds.GetFieldValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblSecond_Total')]")) ?? "0";
                this._SUM_111_112 = Convert.ToInt32(m111) + Convert.ToInt32(m112);
                ((IJavaScriptExecutor)ds.Driver).ExecuteScript("window.close();");
                ds.Driver.SwitchTo().Window(handles[ds.Driver.WindowHandles.Count - 1]);
            }
            return this._SUM_111_112 ?? 0; 
        }
        #endregion
    }
}
