using OpenQA.Selenium;
using OpenQA.Selenium.Chrome; 
using System; 
using System.Linq;
using System.Text; 
using Xunit; 
using System.Text.RegularExpressions; 
using static CyberScope.Tests.Selenium.DriverService;
using Serilog;
using Serilog.Events;
using Xunit.Abstractions; 
using System.Reflection; 
using TinyCsvParser; 
using System.Net.Http;
using System.Net.Http.Headers;
using System.Dynamic;
using Newtonsoft.Json;
using System.IO;
using System.Configuration;

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

        #region UNITTESTS  

        [Theory]
        [InlineData("CIO 2022 Q1", "S1C")]
        //[InlineData("CIO 2022 Q1", "S1A|S1C|4|8|9|10")] // S1A|S1C| 
        //[InlineData("BOD 18-02 Annual 2021", "S1A")] //  S1A|S1C| 
        public void Initialize(string TabText, string SectionPattern)
        {
            var ds = new Selenium.DriverService(_logger);
            ds.CsConnect(UserContext.Agency).ToTab(TabText);
            ds.OnSectionComplete += (s, e) =>
            {
                Screenshot ss = ((ITakesScreenshot)ds.Driver).GetScreenshot();
                ss.SaveAsFile($"C://temp//{Regex.Replace(e.Section.SectionText, @"[^\w\d]", "")}.png", ScreenshotImageFormat.Png);
            };
            ds.InitSections(qg => Regex.IsMatch(qg.SectionText, $"{SectionPattern}"));
            // ds.Driver.Quit();
        }
        [Theory]
        [CsvData()]
        public void Validate(string Section, string metricXpath, string ErrorAttemptExpression, string ExpectedError)
        {
            var va = new ValidationAttempt(metricXpath, ErrorAttemptExpression);
            var ds = new DriverService(_logger);
            ds.CsConnect(UserContext.Agency)
                .ToTab("CIO 2022 Q1")
                .ToSection((s => s.SectionText.Contains($"{Section}")))
                .ApplyValidationAttempt(va, () =>
                {
                    var actualError = ds.GetElementValue(By.XPath("//*[contains(@id, 'Error')]")) ?? "";
                    Assert.Contains(ExpectedError, actualError);
                });
            ds.DisposeDriverService();
        }
        [Theory]
        [CsvData(@"C:\temp\CIO_Validate.csv")]
        public void PerformValidation_Validates(string Section, string metricXpath, string ErrorAttemptExpression, string ExpectedError)
        {
            var va = new ValidationAttempt(metricXpath, ErrorAttemptExpression);
            var ds = new DriverService(_logger);
            ds.CsConnect(UserContext.Agency)
                .ToTab("CIO 2022 Q1")
                .ToSection((s => s.SectionText.Contains($"{Section}")))
                .ApplyValidationAttempt(va, () =>
                {
                    var actualError = ds.GetElementValue(By.XPath("//*[contains(@id, 'Error')]")) ?? "";
                    Assert.Contains(ExpectedError, actualError);
                });
            ds.DisposeDriverService();
        }
        [Fact]
        public void GetDownloads()
        { 
            var path = ConfigurationManager.AppSettings.Get($"DownloadsDirs");
            var file = new DirectoryInfo(path)
                .GetFiles()
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault()
                .FullName; 
        } 
        [Fact]
        public void CustomScript()
        {
            var ds = new DriverService(_logger);
            ds.CsConnect(UserContext.Agency);

            var processor = new CsvCommandProcessor(@"c:\temp\DataCall_CustomAutomate.csv");
            processor.OnProcessComplete += (s, a) =>
            {
                var actualError = ds.GetElementValue(By.XPath("//*[contains(@id, 'Error')]")) ?? "";
                var expected = "";
                Assert.Contains(expected, actualError);
            };
            processor.Process(ds);
        } 
        #endregion

        #region PRIVS  
        [Theory]
        [CsvData(@"C:\temp\DataCall_Validate.csv")]
        public void TestCSV(string a, string b, string c, string d)
        {
            var cda = new CsvDataAttribute(@"C:\temp\DataCall_Validate.csv");
            var a1 = a;
        }
        #endregion 
    }
    public class CsvCommandProcessor
    {
        #region CTOR

        private string _filename;
        private CsvParser<ProcessMap> parser;
        public CsvCommandProcessor(string Filename)
        {
            this._filename = Filename;
            this.parser = new CsvParser<ProcessMap>(
              new CsvParserOptions(true, ','), new CsvGenericMapping()
            );
        }

        #endregion

        #region Events 
        public class ProcessEventArgs : EventArgs
        {
            public DriverService DriverService { get; set; }
            public ProcessEventArgs(DriverService driverService)
            {
                this.DriverService = driverService;
            }
        }
        public event EventHandler<ProcessEventArgs> OnProcessComplete;
        protected virtual void ProcessComplete(ProcessEventArgs e)
        {
            OnProcessComplete?.Invoke(this, e);
        }
        #endregion

        #region METHODS

        public void Process(DriverService ds)
        {
            var rows = parser.ReadFromFile(this._filename, Encoding.ASCII).ToList();
            foreach (var row in rows)
            {
                By by = (row.Result.ElementLocator.StartsWith("//")) ? By.XPath(row.Result.ElementLocator) : By.CssSelector(row.Result.ElementLocator);
                object[] parametersArray = new object[] { by };

                MethodInfo mi = typeof(ChromeDriver).GetMethod("FindElement");
                ds.Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
                IWebElement e = mi.Invoke(ds.Driver, parametersArray) as IWebElement;

                mi = typeof(IWebElement).GetMethod(row.Result.Action);
                object[] parms = (mi.GetParameters().Length > 0) ? new object[] { row.Result.Param } : null;
                if (mi.Name == "SendKeys")
                    typeof(IWebElement).GetMethod("Clear").Invoke(e, null);
                object result = mi.Invoke(e, parms);
            }
            var args = new ProcessEventArgs(ds);
            ProcessComplete(args);
        }
        #endregion
    }
 
}

