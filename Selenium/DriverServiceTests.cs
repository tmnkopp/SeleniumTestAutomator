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
namespace CyberScope.Tests.Selenium
{ 
    public class DriverTest
    {
        [Fact]
        public void Connects()
        { 
            var driver = new DriverService().Driver;
            Assert.IsType<ChromeDriver>(driver);
            driver.Quit();
            var ds = new DriverService();
            driver = ds.CsConnect(UserContext.Agency).Driver;
            Assert.IsType<ChromeDriver>(driver);
            ds.DisposeDriverService(); 
        }  
    } 
}
