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
using System.Diagnostics;
using NCalc;

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

        [Theory]
        [InlineData("qid_2_1", "sum1_1_1__1_1_2 + 1", "cannot exceed the value of", "0")]
        [InlineData("qid_2_2", "sum1_1_1__1_1_2 + 1", "cannot exceed the value of", "0")]
        [InlineData("qid_2_3", "sum1_1_1__1_1_2 + 1", "cannot exceed the value of", "1")]
        [InlineData("qid_2_4", "( sum1_1_1__1_1_2 - 1 ) + 2", "cannot exceed the value of", "1")]
        [InlineData("qid_2_5", "( sum1_1_1__1_1_2 - 2 ) + 4", "cannot exceed the value of", "1")]
        [InlineData("qid_2_6", "sum1_1_1__1_1_2 + 1", "cannot exceed the value of", "1")]
        [InlineData("qid_2_6_1", "2", "cannot exceed the value of", "1")]
        public void S2_Validations(string qid, string attempt, string expected, string finalValue)
        {
            DriverService ds = new DriverService(_logger);
            ds.CsConnect(UserContext.Agency).ToTab("CIO 2022 Q1").ToSection((g => g.SectionText.Contains("S2")));

            int sum1_1_1__1_1_2 = GetSum_111_112(ds);
            SetMetric("sum1_1_1__1_1_2", sum1_1_1__1_1_2.ToString());
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
        public void Ncalc_Calculates()
        {
            Expression e = new Expression("17");
            var actual = e.Evaluate();
            Assert.Equal(17, actual);
        }

        #endregion

        #region PRIVS 
        private Dictionary<string, string> _Answers = new Dictionary<string, string>();
        private void SetMetric(string key, string val)
        {
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
