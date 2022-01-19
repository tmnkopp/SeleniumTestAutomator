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
using CyberScope.Tests.Selenium.Providers;

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

                    this.defaultWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(1)); 
                }
                return _driver;
            }
        }

        #endregion

        #region CTOR 
        WebDriverWait defaultWait;
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
                var eles = (from e in driver.FindElements(By.XPath($"{controlLocator.Locator}"))
                           where e.Displayed==true && e.Enabled==true select e).ToList();
                if (eles.Count > 0)
                {
                    var type = Assm.GetTypes().Where(t => t.Name == controlLocator.Type).FirstOrDefault();
                    IAutomator obj = (IAutomator)Activator.CreateInstance(Type.GetType($"{type.FullName}")); 
                    obj.ContainerSelector = $" #{GetElementID(controlLocator.Selector)} ";
                    obj.ValueSetters = (from vs in obj.ValueSetters
                                        where Regex.IsMatch(vs.GetType().Name, $"{controlLocator.ValueSetterTypes}")
                                        select vs).ToList(); 
                    automators.Add(obj);
                    //this.Logger.Information($"ControlLocator: {type.Name}");
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
            
        public DriverService ToTab(string TabText, bool Launch = true)
        { 
            var driver = this.Driver;
            IWebElement ele;
            WebDriverWait wait;
            if (!driver.Url.Contains("ReporterHome.aspx"))
            {
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
                ele = wait.Until(drv => drv.FindElement(By.XPath($"//*[contains(@id, 'ctl00_ImageButton1')]")));
                ele?.Click();
            }
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
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
        public IWebElement GetElement(By By)
        {
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(1));
            return (from e in wait.Until(dvr => dvr.FindElements(By))
                    where e.Enabled && e.Displayed
                    select e).FirstOrDefault();
        } 
        public string GetElementValue(By By)
        {
            IWebElement element = GetElement(By);
            return element?.Text;
        } 
        public DriverService SetFieldValue(By By, string value)
        {
            IWebElement element = GetElement(By);
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

        public DriverService ApplyValidationAttempt(ValidationAttempt va, Action Assertion) {
            var ds = this;
            Type answerProvider = typeof(ElementValueProvider);
            var ElementValueProviders = (from assm in AppDomain.CurrentDomain.GetAssemblies()
                    where assm.FullName.Contains(AppDomain.CurrentDomain.FriendlyName)
                    from t in assm.GetTypes()
                    where typeof(IElementValueProvider).IsAssignableFrom(t) && t.IsClass
                    select t).ToList();
            
            ElementValueProviders.ForEach(t => {
                var attr = t.GetCustomAttribute<ElementValueProviderMeta>(false);
                if (!string.IsNullOrEmpty(attr?.XpathMatch ?? ""))
                {
                    var e = this.GetElement(By.XPath(attr.XpathMatch)); 
                    if (e != null) answerProvider = t;
                } 
            }); 
            
            IElementValueProvider obj = (IElementValueProvider)Activator.CreateInstance(answerProvider);
            ((IElementValueProvider)obj).Populate(ds);
            string attempt = ((IElementValueProvider)obj).Eval<string>(va.ErrorAttemptExpression);

            var Defaults = new DefaultInputProvider(ds.Driver).DefaultValues;
            Defaults.Add(va.MetricXpath, attempt);

            var sc = new SessionContext(ds.Logger, ds.Driver, Defaults);
            var pcc = ds.PageControlCollection().EmptyIfNull();

            ds.FismaFormEnable();
            string id = Utils.ExtractContainerId(ds.Driver, va.MetricXpath);
            foreach (IAutomator control in pcc)
            {
                if (!string.IsNullOrEmpty(id))
                    ((IAutomator)control).ContainerSelector = $"#{id} ";
                ((IAutomator)control).Automate(sc);
            }
            ds.FismaFormSave(); 
            Assertion();
            return this;
        }

        public DriverService InitSections(Func<DataCallSection, bool> SectionGroupPredicate )
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
                this.FismaFormEnable();
                 
                foreach (IAutomator control in this.PageControlCollection().EmptyIfNull())
                    ((IAutomator)control).Automate(sc);
         
                if (this.Driver.PageSource.Contains("Server Error in '/' Application")) 
                    ApplicationError(appargs);

                this.FismaFormSave();

                SectionComplete(appargs);
            }
 
            return this;
        }

        public DriverService OpenTab() { 
            string url = this.Driver.Url;
            ((IJavaScriptExecutor)this.Driver).ExecuteScript("window.open();");
            var handles = this.Driver.WindowHandles;
            this.Driver.SwitchTo().Window(handles[this.Driver.WindowHandles.Count - 1]);
            this.Driver.Navigate().GoToUrl($"{url}"); 
            return this;
        }
        
        public DriverService CloseTab()
        {
            var handles = this.Driver.WindowHandles;
            ((IJavaScriptExecutor)this.Driver).ExecuteScript("window.close();");
            this.Driver.SwitchTo().Window(handles[this.Driver.WindowHandles.Count - 1]);
            return this;
        }
        #endregion

        #region METHODS: FismaForm ACCESSORS 

        public DriverService FismaFormEnable()
        {
            string btntext = "_btnEdit";
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(.15));
            var eles = wait.Until(drv =>
                drv.FindElements(By.XPath($"//td[contains(@class, 'ButtonDiv')]//*[contains(@id, '{btntext}')]")));
            (from el in eles where el.Displayed && el.Enabled select el).FirstOrDefault()?.Click();
            return this;
        }
            
        public DriverService FismaFormCancel()
        {
            return this.FismaFormEnable();
        }
            
        public DriverService FismaFormSave()
        {
            string btntext = "_btnSave";
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(.15));
            var eles = wait.Until(drv =>
                drv.FindElements(By.XPath($"//td[contains(@class, 'ButtonDiv')]//*[contains(@id, '{btntext}')]")));
            (from el in eles where el.Displayed && el.Enabled select el).FirstOrDefault()?.Click();  
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

        public string GetElementID(string XPathSelector) {
            string id = "";
            if (this.Driver.FindElements(By.XPath(XPathSelector)).Count > 0)
                id = this.Driver.FindElement(By.XPath(XPathSelector)).GetAttribute("id"); 
            return id;
        }

        #endregion

        #endregion
    }
}
