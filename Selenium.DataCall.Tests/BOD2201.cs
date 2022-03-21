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
using System.IO; 
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Diagnostics;

namespace CyberScope.Tests.Selenium.Datacall.Tests
{ 
    public class BOD2201 : BaseDataCallTest
    { 
        #region CTOR 
        public BOD2201(ITestOutputHelper output):base(output)
        {  
        }
        #endregion
  
        [Theory]
        [InlineData("22-01", ".*")] 
        public void Initialize(string TabText, string SectionPattern)
        { 
            var ds = new Selenium.DriverService(_logger);
            ds.CsConnect(UserContext.Agency).ToTab(TabText); 
            ds.InitSections(qg => Regex.IsMatch(qg.SectionText, $"{SectionPattern}"));
            
        } 
    } 
}
