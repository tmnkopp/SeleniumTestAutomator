using OpenQA.Selenium.Chrome;
using System;
namespace CyberScope.Automator
{
    public class AutomatorEventArgs : EventArgs
    {
        public AutomatorEventArgs(SessionContext context)
        {
            this.Context = context;
        }
        public ChromeDriver Driver => Context.Driver; 
        public SessionContext Context { get; set; }
        public string CurrentWindowHandle { get; set; }
    }
}