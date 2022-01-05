using OpenQA.Selenium.Chrome;
using System;

namespace CyberScope.Tests.Selenium
{
    public class AutomatorEventArgs : EventArgs
    {
        public AutomatorEventArgs(ChromeDriver driver)
        {
            this.Driver = driver;
        }
        public ChromeDriver Driver { get; set; }
        public string CurrentWindowHandle { get; set; }
    }
    
}
