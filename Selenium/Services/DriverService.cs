using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Configuration; 
using System.Linq; 
using System.Reflection;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System.Text.RegularExpressions;
using System.IO;
using Serilog; 

namespace CyberScope.Tests.Selenium
{
    #region ENUMS 
    public enum UserContext
    {
        Admin,
        Agency
    }
    public enum TestResult
    {
        Pass,
        Fail 
    }

    #endregion

    public class DriverService
    {
        #region PROPS 
        public UserContext UserContext { get; set; } = UserContext.Agency;
        public TestResult TestResult { get; set; }
        public ILogger Logger;  
        private ChromeDriver _driver;  

        public void DisposeDriverService() { 
            Driver.Quit();
        }
        public ChromeDriver Driver
        {
            get
            {
                if (_driver == null)
                {
                    var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
                    var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
                    var dirPath = Path.GetDirectoryName(codeBasePath);
              
                    var chromedriverpath = ConfigurationManager.AppSettings.Get($"chromedriverpath");
                    if (string.IsNullOrEmpty(chromedriverpath)) 
                        chromedriverpath = $"{dirPath}\\Selenium\\";
                
                    ChromeOptions options = new ChromeOptions();
                    var chromeDriverService = ChromeDriverService.CreateDefaultService(chromedriverpath);
                    chromeDriverService.HideCommandPromptWindow = true;
                    chromeDriverService.SuppressInitialDiagnosticInformation = true;
                    var args = SettingsProvider.ChromeOptions;
                    foreach (var item in args.EmptyIfNull()) 
                        options.AddArgument(item);
                 
                    _driver = new ChromeDriver(chromeDriverService, options); 
                }
                return _driver;
            }
        }

        #endregion

        #region CTOR 
        public DriverService(ILogger Logger)
        {
            this.Logger = Logger;
        } 
        #endregion

        #region Events 
        public class DriverServiceEventArgs : EventArgs
        {
            public DriverService DriverService { get; set; }
            public DataCallSection Section { get; set; }
            public ChromeDriver Driver { get; set; }
            public DriverServiceEventArgs(DriverService driverService)
            {
                this.DriverService = driverService;
                this.Driver = driverService.Driver;
            } 
        } 
        public event EventHandler<DriverServiceEventArgs> OnApplicationError;
        protected virtual void ApplicationError(DriverServiceEventArgs e)
        { 
            OnApplicationError?.Invoke(this, e);
        }
        public event EventHandler<DriverServiceEventArgs> OnSectionComplete;
        protected virtual void SectionComplete(DriverServiceEventArgs e)
        {
            OnSectionComplete?.Invoke(this, e);
        }
        #endregion

        #region METHODS

        #region METHODS: Control ACCESSORS 

        internal IEnumerable<IAutomator> PageControlCollection() {
            var automators = new List<IAutomator>();
            var driver = this.Driver;
            var controlLocators = SettingsProvider.ControlLocators.EmptyIfNull();
            foreach (ControlLocator controlLocator in controlLocators)
            { 
                if (driver.FindElements(By.XPath($"{controlLocator.Locator}")).Count > 0)
                {
                    var type = Assm.GetTypes().Where(t => t.Name == controlLocator.Type).FirstOrDefault();
                    IAutomator obj = (IAutomator)Activator.CreateInstance(Type.GetType($"{type.FullName}")); 
                    obj.ContainerSelector = $" #{GetElementID(controlLocator.Selector)} ";
                    obj.ValueSetters = (from vs in obj.ValueSetters
                                        where Regex.IsMatch(vs.GetType().Name, $"{controlLocator.ValueSetterTypes}")
                                        select vs).ToList(); 
                    automators.Add(obj);
                }
            }
            return automators;
        }
            
        public bool ElementExists(By Selector) {
            return (this.Driver.FindElements(Selector).Count() > 0);
        }
        #endregion

        #region METHODS: NAV
        public DriverService CsConnect(UserContext userContext)
        { 
            var driver = this.Driver;
            var user = $"{ConfigurationManager.AppSettings.Get($"{userContext.ToString()}User")}";
            var pass = $"{ConfigurationManager.AppSettings.Get($"{userContext.ToString()}Pass")}";
            driver.Url = ConfigurationManager.AppSettings.Get("CSTargerUrl");  
            driver.FindElementByCssSelector("input[id$=Login1_UserName]").SendKeys(user);
            driver.FindElementByCssSelector("input[id$=Login1_Password]").SendKeys(pass);
            driver.FindElementByCssSelector("input[id$=Login1_LoginButton]").Click(); 
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            IWebElement ele = wait.Until(drv => drv.FindElement(By.CssSelector($"input[id$=ctl00_ContentPlaceHolder1_btn_Accept]")));
            ele.Click(); 
            return this;
        }
            
        public DriverService ToTab(string TabText)
        { 
            var driver = this.Driver;
            IWebElement ele; 
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            var eles = wait.Until(drv => drv.FindElements(By.XPath($"//*[contains(@id, '_Surveys')]//*[contains(@class, 'rtsTxt')]")))?.Reverse();
   
            ele = (from e in eles where Regex.IsMatch(e.Text, TabText) || e.Text.Contains(TabText) select e).FirstOrDefault();
            ele?.Click(); 
            ele = wait.Until(drv => drv.FindElement(By.XPath($"//a[contains(@id, '_ctl04_hl_Launch')]")));
            ele?.Click();
             
            return this;
        }
           
        public DriverService ToSection(DataCallSection Section)  { 
            SelectElement se = new SelectElement(this.Driver.FindElementByCssSelector("*[id*='_ddl_Sections']"));
            se?.Options.Where(o => o.Text.Contains(Section?.SectionText)).FirstOrDefault()?.Click();
            return this;
        }
        public DriverService ToSection(Func<DataCallSection, bool> Predicate) { 
            var section = this.Sections().Where(Predicate).FirstOrDefault();
            this.ToSection(section);
            return this; 
        }
           
        public DriverService ToSection(int Index)
        {
            var driver = this.Driver; 
            SelectElement se = new SelectElement(driver.FindElementByCssSelector("*[id*='_ddl_Sections']"));
            if (Index < 0) 
                Index = se.Options.Count() - 1; 
            se.SelectByIndex(Index);
            return this;
        }
        #endregion

        #region METHODS: FIELD ACCESSORS
        public IWebElement GetField(By By)
        {
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(2));
            return (from e in wait.Until(dvr => dvr.FindElements(By))
                    where e.Enabled && e.Displayed
                    select e).FirstOrDefault();
        }
        
        public string GetFieldValue(By By)
        {
            IWebElement element = GetField(By);
            return element?.Text;
        }
        
        public DriverService SetFieldValue(By By, string value)
        {
            IWebElement element = GetField(By);
            element?.Clear();
            element?.SendKeys(value);
            return this;
        }
        #endregion

        #region METHODS: Section ACCESSORS

        public IEnumerable<DataCallSection> Sections() { 
            var driver = this.Driver;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            IReadOnlyCollection<IWebElement> ele;
            ele = wait.Until(drv => drv.FindElements(By.XPath($"//*[contains(@id, '_ddl_Sections')]/option")));
            var groups = (from e in ele
                            select new DataCallSection
                            {
                                URL = e.GetAttribute("value"),
                                SectionText = e.Text
                            }).ToList(); 
            return groups;  
        }
         
        public DriverService TestSections(Func<DataCallSection, bool> SectionGroupPredicate )
        { 
            SessionContext sc = new SessionContext() { 
                Driver = this.Driver
                , Logger = this.Logger
                , Defaults = new DefaultInputProvider(this.Driver).DefaultValues 
            };
             
            foreach (DataCallSection section in this.Sections().Where(SectionGroupPredicate))
            {
                var appargs = new DriverServiceEventArgs(this);
                appargs.Section = section;
                 
                this.ToSection(section);
                ((IJavaScriptExecutor)this.Driver).ExecuteScript("document.getElementsByClassName('navbar')[0].style.display = 'none';");
                    
                foreach (IAutomator control in this.PageControlCollection().EmptyIfNull())
                    ((IAutomator)control).Automate(sc);
         
                if (this.Driver.PageSource.Contains("Server Error in '/' Application")) 
                    ApplicationError(appargs);   

                SectionComplete(appargs);
            }
 
            return this;
        }

        #endregion

        #region METHODS: FismaForm ACCESSORS 

        public DriverService FismaFormEnable()
        { 
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(2));
            IWebElement input = wait.Until(drv => drv.FindElement(By.CssSelector($"*[id$='_btnEdit']")));
            input.Click(); 
            return this;
        }
            
        public DriverService FismaFormCancel()
        { 
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(2));
            IWebElement input = wait.Until(drv => drv.FindElement(By.CssSelector($"*[id$='_btnEdit']")));
            input.Click();
            return this;
        }
            
        public DriverService FismaFormSave()
        {
            var driver = this.Driver;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            IWebElement input = wait.Until(drv => drv.FindElement(By.CssSelector($"*[id$='_btnSave']")));
            input.Click();
            return this;
        }

        public bool FismaFormValidates() {
            this.ToSection(-1);
            var success = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(5))
            .Until(dvr => dvr.FindElements(By.CssSelector("#ctl00_ContentPlaceHolder1_lblSuccessInfo")));
            if (success.Count() > 0)
                return success[0].Text.Contains("Your form has been validated and contains no errors.");
            return false;
        }

        #endregion

        #region METHODS: PRIV

        private string GetElementID(string XPathSelector) {
            string id = "";
            if (this.Driver.FindElements(By.XPath(XPathSelector)).Count > 0)
                id = this.Driver.FindElement(By.XPath(XPathSelector)).GetAttribute("id"); 
            return id;
        }

        #endregion

        #endregion
    }
}
