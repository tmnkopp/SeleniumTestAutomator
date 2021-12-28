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
using Xunit.Sdk;
using System.Reflection;
using System.IO;

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
        [InlineData("CIO 2022 Q1", "[89]")] 
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
        [InlineData("qid_2_1", "sum111_112 + 1", "cannot exceed the value", "0")]
        [InlineData("qid_2_2", "sum111_112 + 1", "cannot exceed the value", "0")]
        [InlineData("qid_2_3", "sum111_112 + 1", "cannot exceed the value", "1")]
        [InlineData("qid_2_4", "( sum111_112 - 1 ) + 2", "cannot exceed the value", "1")]
        [InlineData("qid_2_5", "( sum111_112 - 2 ) + 4", "cannot exceed the value", "1")]
        [InlineData("qid_2_6", "sum111_112 + 1", "cannot exceed the value", "1")]
        [InlineData("qid_2_6_1", "2", "cannot exceed the value", "1")]
        public void S2_Validations(string qid, string attempt, string expected, string finalValue)
        {
            DriverService ds = new DriverService(_logger);
            ds.CsConnect(UserContext.Agency).ToTab("CIO 2022 Q1").ToSection((g => g.SectionText.Contains("S2")));
            Populate_111_112(ds);
            int sum111_112 = Convert.ToInt32(GetAnswer("qid_111_111")) + Convert.ToInt32(GetAnswer("qid_111_112"));
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
        private string GetAnswer(string key)
        { 
            return (_Answers.ContainsKey(key)) ? _Answers[key] : "0" ;
        }
        private void Populate_111_112(DriverService ds)
        { 
            var url = ds.Driver.Url;
            ((IJavaScriptExecutor)ds.Driver).ExecuteScript("window.open();");
            var handles = ds.Driver.WindowHandles;
            ds.Driver.SwitchTo().Window(handles[ds.Driver.WindowHandles.Count - 1]);
            ds.Driver.Navigate().GoToUrl($"{url}");
            ds.ToSection((g => g.SectionText.Contains("S1")));
            string m111 = ds.GetFieldValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblfirst_Total')]")) ?? "0";
            string m112 = ds.GetFieldValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblSecond_Total')]")) ?? "0";
            SetMetric("qid_111_111", m111);
            SetMetric("qid_111_112", m112);
            int sum111_112 = Convert.ToInt32(GetAnswer("qid_111_111")) + Convert.ToInt32(GetAnswer("qid_111_112"));
            SetMetric("sum_111_112", sum111_112.ToString());
            ((IJavaScriptExecutor)ds.Driver).ExecuteScript("window.close();");
            ds.Driver.SwitchTo().Window(handles[ds.Driver.WindowHandles.Count - 1]); 
        }
        [Theory]
        [CsvData(@"C:\temp\test.csv")]
        public void TestWithCSVData(string a, string b, string c, string d)
        {
            var a1 = a;

        }
       

        #endregion 
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CsvDataAttribute : DataAttribute
    {
        private readonly string _fileName;
        public CsvDataAttribute(string fileName)
        {
            _fileName = fileName;
        } 
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var pars = testMethod.GetParameters();
            var parameterTypes = pars.Select(par => par.ParameterType).ToArray();
            using (var csvFile = new StreamReader(_fileName))
            {
                // csvFile.ReadLine(); Delimiter Row: "sep=,". Comment out if not used
                // csvFile.ReadLine(); Headings Row. Comment out if not used
                string line;
                while ((line = csvFile.ReadLine()) != null)
                {
                    var row = line.Split(',');
                    yield return ConvertParameters((object[])row, parameterTypes);
                }
            }
        } 
        private static object[] ConvertParameters(IReadOnlyList<object> values, IReadOnlyList<Type> parameterTypes)
        {
            var result = new object[parameterTypes.Count];
            for (var idx = 0; idx < parameterTypes.Count; idx++)
            {
                result[idx] = ConvertParameter(values[idx], parameterTypes[idx]);
            } 
            return result;
        } 
        private static object ConvertParameter(object parameter, Type parameterType)
        {
            return parameterType == typeof(int) ? Convert.ToInt32(parameter) : parameter;
        }
    }
}
