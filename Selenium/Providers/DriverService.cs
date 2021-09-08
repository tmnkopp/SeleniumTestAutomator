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
        private string _PK_FORM;
        private ChromeDriver _driver;
         
        public DriverService() { }
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
                    {
                        chromedriverpath = $"{dirPath}\\Selenium\\";
                    }
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
        public class ApplicationErrorEventArgs : EventArgs
        {
            public DriverService DriverService { get; set; }
            public QuestionGroup QuestionGroup { get; set; }
            public ChromeDriver Driver { get; set; }
            public ApplicationErrorEventArgs(DriverService driverService)
            {
                this.DriverService = driverService;
                this.Driver = driverService.Driver;
            } 
        }

        public event EventHandler<ApplicationErrorEventArgs> OnApplicationError;
        protected virtual void ApplicationError(ApplicationErrorEventArgs e)
        { 
            OnApplicationError?.Invoke(this, e);
        }

        #endregion
        #region METHODS

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
        public IEnumerable<QuestionGroup> Sections() {
            return FismaSections.GetAll(_PK_FORM);
        }


        public DriverService SectionTest(Func<QuestionGroup, bool> SectionGroupPredicate )
        {
            foreach (var section in this.Sections().Where(SectionGroupPredicate))
            {
                this.ToSection(section);
                foreach (IAutomator control in this.DetectedCompositeControls().EmptyIfNull())
                    ((IAutomator)control).Automate(this.Driver);

                bool IsYellow = this.Driver.PageSource.Contains("Server Error in '/' Application");
                if (IsYellow)
                {
                    var appargs = new ApplicationErrorEventArgs(this); 
                    appargs.QuestionGroup = section;
                    OnApplicationError(this, appargs);
                    this.TestResult = TestResult.Fail;
                    return this;
                } 
            }
            this.TestResult = TestResult.Pass;
            return this;
        }
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
        public IEnumerable<IAutomator> DetectedCompositeControls() {
            var automators = new List<IAutomator>();
            var driver = this.Driver;
            foreach (var c in SettingsProvider.CompositeControls.EmptyIfNull())
            {
                if (driver.FindElements(By.XPath($"{c.Key}")).Count > 0)
                {
                    var type = Assm.GetTypes().Where(t => t.Name.Contains(c.Value)).FirstOrDefault();
                    IAutomator obj = (IAutomator)Activator.CreateInstance(Type.GetType($"{type.FullName}"));
                    obj.PK_FORM = _PK_FORM; 
                    automators.Add(obj);
                }
            }
            return automators;
        }
        public bool ElementExists(By Selector) {
            return (this.Driver.FindElements(Selector).Count() > 0);
        }
        #endregion
    }
}
