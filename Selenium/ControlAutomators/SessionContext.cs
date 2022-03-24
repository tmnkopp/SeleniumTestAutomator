using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;

namespace CyberScope.Tests.Selenium
{
    public class SessionContext 
    {
        public ILogger Logger { get ; set; }
        public ChromeDriver Driver { get; set; }
        public Dictionary<string,string> Defaults { get; set; }
        public UserContext userContext { get; set; } 
        public SessionContext()
        { 
        }
        public SessionContext(ILogger Logger, ChromeDriver Driver)
        {
            this.Logger = Logger;
            this.Driver = Driver;
        }
        public SessionContext(
            ILogger Logger
            , ChromeDriver Driver
            , Dictionary<string, string> Defaults)
        {
            this.Logger = Logger;
            this.Driver = Driver;
            this.Defaults = Defaults;
        }

    } 
}
