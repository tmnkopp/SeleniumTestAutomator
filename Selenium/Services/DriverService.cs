using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System.Text.RegularExpressions;
using System.IO;
using Serilog;
using Serilog.Core;

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
        private string _PK_FORM;
        private ChromeDriver _driver;  
        public DriverService(ILogger Logger) {
            this.Logger = Logger;
        }
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
                    options.AddArgument("log-level=3");
                    _driver = new ChromeDriver(chromeDriverService, options);
                    _driver.Manage().Window.Maximize();
                }
                return _driver;
            }
        }

        #endregion

        #region Events 
        public class DriverServiceEventArgs : EventArgs
        {
            public DriverService DriverService { get; set; }
            public QuestionGroup QuestionGroup { get; set; }
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
        public DriverService ToTab(string PK_FORM)
        { 
            var driver = this.Driver;
            var datacall = (from rc in RepCycRepo.Instance.Query(r => r.FormMaster.PK_Form == PK_FORM)
                            select new { Tab = rc.Description }).FirstOrDefault();

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            IWebElement ele = wait.Until(drv => drv.FindElement(By.XPath($"//ul[@class='rtsUL']//li//span[contains(text(), '{datacall.Tab}')]")));
            ele.Click();

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            ele = wait.Until(drv => drv.FindElement(By.XPath($"//a[contains(@id, '_ctl04_hl_Launch')]")));
            ele.Click();
            
            this._PK_FORM = PK_FORM;
            return this;
        }
        public DriverService ToSection(QuestionGroup QuestionGroup)
        {
            var driver = this.Driver; 
            SelectElement se = new SelectElement(driver.FindElementByCssSelector("*[id*='_ddl_Sections']"));
            se?.Options.Where(o => o.Text.Contains(QuestionGroup.SectionText)).FirstOrDefault()?.Click();
            return this;
        }
        public DriverService ToSection(Func<QuestionGroup, bool> Predicate) {
            var driver = this.Driver;
            var datacall =
             (from rc in RepCycRepo.Instance.Query(r => r.FormMaster.PK_Form == _PK_FORM)
              select new
              {
                  Tab = rc.Description,
                  Section = (
                    from qg in rc.FormMaster.QuestionGroups.Where(Predicate) 
                    select new { qg.Text, qg.GroupName, qg.SectionText }).FirstOrDefault()
              }).FirstOrDefault();
            SelectElement se = new SelectElement(driver.FindElementByCssSelector("*[id*='_ddl_Sections']"));
            se?.Options.Where(o => o.Text.Contains(datacall.Section.SectionText)).FirstOrDefault()?.Click();
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

        public IEnumerable<QuestionGroup> Sections() {
            return FismaSections.GetAll(_PK_FORM);
        }
         
        public DriverService SectionTest(Func<QuestionGroup, bool> SectionGroupPredicate )
        {
            SessionContext sc = new SessionContext() { Driver = this.Driver, Logger = this.Logger };
            foreach (var section in this.Sections().Where(SectionGroupPredicate))
            {
                var appargs = new DriverServiceEventArgs(this);
                appargs.QuestionGroup = section;

                this.ToSection(section);
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
           var driver = this.Driver; 
            WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(2));
            IWebElement input = wait.Until(drv => drv.FindElement(By.CssSelector($"*[id$='_btnEdit']")));
            input.Click(); 
            return this;
        }
        public DriverService FismaFormCancel()
        {
            var driver = this.Driver;
            WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(2));
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

        #region METHODS: Control ACCESSORS 

        internal IEnumerable<IAutomator> PageControlCollection() {
            var automators = new List<IAutomator>();
            var driver = this.Driver;
            foreach (var c in SettingsProvider.ControlLocators.EmptyIfNull())
            {
                if (driver.FindElements(By.XPath($"{c.Locator}")).Count > 0)
                {
                    var type = Assm.GetTypes().Where(t => t.Name.Contains(c.Type)).FirstOrDefault();
                    IAutomator obj = (IAutomator)Activator.CreateInstance(Type.GetType($"{type.FullName}"));
                    obj.PK_FORM = _PK_FORM;
                    obj.ContainerSelector = $" #{GetElementID(c.Selector)} ";
                    automators.Add(obj);
                }
            }
            return automators;
        }
        public bool ElementExists(By Selector) {
            return (this.Driver.FindElements(Selector).Count() > 0);
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
