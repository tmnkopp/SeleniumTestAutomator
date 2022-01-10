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
using CyberScope.Tests.Selenium.Providers;
using TinyCsvParser;
using System.Collections.ObjectModel;

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
        [InlineData("CIO 2022 Q1", "S1A")]  
        //[InlineData("CIO 2022 Q1", "S1A|S1C|4|8|9|10")] // S1A|S1C| 
        //[InlineData("BOD 18-02 Annual 2021", "S1A")] //  S1A|S1C| 
        public void Initialize(string TabText, string SectionPattern)
        {
            var ds = new Selenium.DriverService(_logger);
            ds.CsConnect(UserContext.Agency).ToTab(TabText);
            ds.OnSectionComplete += (s, e) =>
            {
                var url = e.Section.URL;
            };
            ds.InitSections(qg => Regex.IsMatch(qg.SectionText, $"{SectionPattern}"));
            ds.Driver.Quit();
        }
        [Theory] 
        [CsvData()]
        public void Validate(string Section, string metricXpath, string ErrorAttemptExpression, string ExpectedError)
        {
            var va = new ValidationAttempt( metricXpath, ErrorAttemptExpression );
            var ds = new DriverService(_logger);
            ds.CsConnect(UserContext.Agency)
                .ToTab("CIO 2022 Q1")
                .ToSection((s => s.SectionText.Contains($"{Section}")))
                .ApplyValidationAttempt(va, () => {
                    var actualError = ds.GetElementValue(By.XPath("//*[contains(@id, 'Error')]")) ?? "";
                    Assert.Contains(ExpectedError, actualError);
                });
            ds.DisposeDriverService();
        }
        [Theory]
        [CsvData(@"C:\temp\CIO_Validate.csv")]
        public void PerformValidation_Validates(string Section, string metricXpath, string ErrorAttemptExpression, string ExpectedError)
        { 
            var va = new ValidationAttempt( metricXpath, ErrorAttemptExpression ); 
            var ds = new DriverService(_logger); 
            ds.CsConnect(UserContext.Agency)
                .ToTab("CIO 2022 Q1")
                .ToSection((s => s.SectionText.Contains($"{Section}"))) 
                .ApplyValidationAttempt(va, () => {
                    var actualError = ds.GetElementValue(By.XPath("//*[contains(@id, 'Error')]")) ?? "";
                    Assert.Contains(ExpectedError, actualError);
                });
            ds.DisposeDriverService();
        }

        [Theory]
        [InlineData(@"CIO 2022 Q1", "S2" )]
        public void Custom_Automation(string Tab, string Section )
        { 
            var ds = new DriverService(_logger);
            ds.CsConnect(UserContext.Agency);
            ds.ToTab(Tab);
            ds.ToSection((s => s.SectionText.Contains($"{Section}"))) ;
             
            CsvParser<GenericMap> csvParser = new CsvParser<GenericMap>(
              new CsvParserOptions(true, ',')
             ,new CsvGenericMapping()
            );

            var csv = csvParser.ReadFromFile(@"c:\temp\automate.csv", Encoding.ASCII).ToList();
            foreach (var row in csv)
            {
                MethodInfo methodInfo = typeof(ChromeDriver).GetMethod("FindElementsByXPath");
                object[] parametersArray = new object[] { row.Result.ColC };
                ReadOnlyCollection<IWebElement> elements = methodInfo.Invoke(ds.Driver, parametersArray) as ReadOnlyCollection<IWebElement>;
                elements?.ToList()?.ForEach( e => {
                    object result = null;
                    object[] parms = null;
                    methodInfo = typeof(IWebElement).GetMethod(row.Result.ColD);  
                    if (methodInfo.GetParameters().Length > 0)
                    {
                        parms = new object[] { row.Result.ColE }; 
                    } 
                    result = methodInfo.Invoke(e, parms);
                }); 
            }
            ds.DisposeDriverService();
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
}
