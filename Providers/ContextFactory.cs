using Serilog;
using CyberScope.Automator;
using OpenQA.Selenium.Chrome;

namespace CyberScope.Automator
{
    public class ContextFactory{
        private ChromeDriver Driver;
        private ILogger Logger;
        public ContextFactory(ChromeDriver driver, ILogger logger)
        {
            this.Driver = driver;
            this.Logger = logger; 
        }
        public SessionContext Create()
        {
            return new SessionContext()
            {
                Driver = this.Driver ,
                Logger = this.Logger ,
                Defaults = new DefaultInputProvider(this.Driver).DefaultValues
            }; 
         } 
    } 
}

