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
    public class SAOP : BaseDataCallTest
    {
        #region CTOR
        public SAOP(ITestOutputHelper output) : base(output)
        {
        }
        #endregion


        #region UNITTESTS 
        [Theory]
        [InlineData("2021 Annual Report - SAOP", "8")]
        public void Initialize(string _PK_FORM, string SectionPattern)
        {
            var ds = new Selenium.DriverService(_logger);
            var driver = ds.CsConnect(UserContext.Agency).ToTab(_PK_FORM).Driver; 
            ds.InitSections(qg => Regex.IsMatch(qg.SectionText, $"{SectionPattern}"));  
        } 
        #endregion
    } 
}
