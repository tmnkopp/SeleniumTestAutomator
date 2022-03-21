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
using Serilog;
using Xunit.Abstractions;
using Serilog.Events;
using System.IO;
using System.Diagnostics;

namespace CyberScope.Tests.Selenium
{ 
    public class DriverTest
    {
        ILogger _logger;
        private readonly ITestOutputHelper output;
        public DriverTest(ITestOutputHelper output)
        {
            this.output = output;
            _logger = new LoggerConfiguration()
            .WriteTo.TestOutput(output, LogEventLevel.Verbose)
            .CreateLogger();
        }

        [Fact]
        public void Connects()
        { 
            var ds = new DriverService(_logger);
            var driver = ds.CsConnect(UserContext.Agency).Driver;
            Assert.IsType<ChromeDriver>(driver);
            ds.DisposeDriverService(); 
        }
        [Fact]
        public void IIS_Connects()
        {
            StartIIS();
           // _iisProcess.Kill(); 
        }
        
        private void StartIIS()
        { 
            var solutionFolder = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)));
            var applicationPath = Path.Combine(solutionFolder, "CyberScope.Tests"); 
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            Process _iisProcess = new Process(); 
            _iisProcess.StartInfo.FileName = programFiles + @"\IIS Express\iisexpress.exe";
            _iisProcess.StartInfo.Arguments = string.Format("/path:{0} /port:{1}", applicationPath, "57236");
            _iisProcess.Start();
        } 
    } 
}
