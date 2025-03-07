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
using CyberScope.Automator.Providers;
using System.Threading;
using Newtonsoft.Json;
using Xunit;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;
using DocumentFormat.OpenXml.Bibliography;

namespace CyberScope.Automator
{
    #region ENUMS 
    public enum UserContext
    {
        Admin,
        Agency,
        CB
    }
    public enum TestResult
    {
        Pass,
        Fail 
    }
    #endregion
    
    public class CsDriverService
    {
        #region PROPS 
        public UserContext UserContext { get; set; } = UserContext.Agency; 
        public ILogger Logger;  
        private ChromeDriver _driver;  
        public void DisposeDriverService() {
            if (SettingsProvider.appSettings["DisposeOnComplete"] == "true")
            {
                Driver.Quit();
            } 
        }
        public string ChromeDriverPath { get; private set; }
        public string AgencyName{
            get{
                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(2));
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete"); 
                if (!Regex.IsMatch(this.Driver.PageSource, @"lbl_AgencyName")) return null;
                var elm = wait.Until(d => d.FindElement(By.CssSelector("*[id$=lbl_AgencyName]")));
                return (elm == null) ? null : elm.Text;  
            }
        }
        public string PK_FORM
        {
            get
            {
                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(2)); 
                var action = wait.Until(d => d.FindElement(By.XPath("//form"))).GetAttribute("action");
                var match = Regex.Match(action, @"(\d{4}_[A-Z0-9]{1,3}_[A-Z0-9]{1,8}).*aspx"); 
                if (!match.Success) return null;
                if (match.Groups.Count < 1) return null;
                var pk_form = match.Groups[1].Value;
                return pk_form.Replace("_","-");
            }
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
                   
                    if (SettingsProvider.appSettings.ContainsKey("chromedriverpath")) {
                        ChromeDriverPath = SettingsProvider.appSettings[$"chromedriverpath"];
                    } else{
                        ChromeDriverPath = new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser);
                        ChromeDriverPath = ChromeDriverPath.Replace("chromedriver.exe", "");
                    } 
                    ChromeOptions options = new ChromeOptions();
                    var chromeDriverService = ChromeDriverService.CreateDefaultService(ChromeDriverPath);
                    chromeDriverService.HideCommandPromptWindow = true;
                    chromeDriverService.SuppressInitialDiagnosticInformation = true;
                    var args = SettingsProvider.ChromeOptions;
                    foreach (var item in args.EmptyIfNull()) 
                        options.AddArgument(item);
                    Logger.Information("{o}", new { driverpath = ChromeDriverPath });
                    //Logger.Debug("{o}", new { ChromeOptions = options }); 
                    _driver = new ChromeDriver(chromeDriverService, options);
                    if (JsonConvert.SerializeObject(options).Contains("minimize")) 
                        _driver.Manage().Window.Minimize(); 
                    this.defaultWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(1)); 
                }
                return _driver;
            }
        }
        #endregion
       
        #region CTOR 
        WebDriverWait defaultWait;
        public CsDriverService(ILogger Logger)
        { 
            this.Logger = Logger;
        } 
        #endregion
        
        #region Events 
        public class DriverServiceEventArgs : EventArgs
        {
            public CsDriverService DriverService { get; set; }
            public DataCallSection Section { get; set; }
            public ChromeDriver Driver { get; set; }
            public DriverServiceEventArgs(CsDriverService driverService)
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
                bool isExcluded = false;
                foreach (var exclude in controlLocator.Exclude)
                {
                    isExcluded = (from e in driver.FindElements(By.XPath($"{exclude}")) select e).FirstOrDefault() != null;
                    if (isExcluded ) continue;
                }
                if (isExcluded) continue;

                var eles = (from e in driver.FindElements(By.XPath($"{controlLocator.Locator}"))
                           where e.Displayed==true && e.Enabled==true select e).ToList();
                if (eles.Count > 0)
                {
                    var type = Assm.GetTypes().Where(t => t.Name == controlLocator.Type).FirstOrDefault(); 
                    var exists = (from a in automators where a.GetType().Name == type.Name select a).ToList().Count > 0;
                    if (exists) continue;

                    IAutomator obj = (IAutomator)Activator.CreateInstance(Type.GetType($"{type.FullName}")); 
                    string ValueSetterType = (!string.IsNullOrWhiteSpace(controlLocator.ValueSetterTypes)) ? controlLocator.ValueSetterTypes : ".*"; 
                    obj.ContainerSelector = $" #{GetElementID(controlLocator.Selector)} ";
                    obj.ValueSetters = (from vs in obj.ValueSetters
                                        where Regex.IsMatch(vs.GetType().Name, ValueSetterType)
                                        select vs).ToList();
                    obj.Overwrite = controlLocator.Overwrite; 
                    automators.Add(obj); 
                    this.Logger.Information("{@controlLocator}", new { controlLocator });
                }
            }
            return automators;
        }
        public bool ElementExists(By Selector) { 
            var elms = this.Driver.FindElements(Selector);
             return (elms.Count() > 0);
        }
        #endregion
        #region METHODS: NAV
        public CsDriverService ToUrl(string url)
        {
            var @base = SettingsProvider.appSettings[$"CSTargerUrl"]; 
            url = url.Replace("~", @base);
            this.Driver.Navigate().GoToUrl(url);
            return this;
        }
        public CsDriverService CsConnect(UserContext userContext)
        {
            var url = SettingsProvider.appSettings[$"CSTargerUrl"];
            var sc = new SessionContext()
            {
                Driver = this.Driver ,
                Logger = this.Logger ,
                userContext = userContext
            }; 
            var IConnectType = (from t in Assm.GetTypes()
                           where typeof(IConnect).IsAssignableFrom(t) 
                           && Regex.IsMatch(url, t.GetCustomAttribute<ConnectorMeta>()?.Selector ?? "^$")
                           select t).FirstOrDefault() ?? typeof(DefaultCsConnector);
            IConnect obj = (IConnect)Activator.CreateInstance(Type.GetType($"{IConnectType.FullName}"));
            obj.Connect(sc);
             
            this.Driver.Manage().Cookies.DeleteCookieNamed("_selenium");
            var cookie = new Cookie("_selenium", DateTime.Today.ToString());
            this.Driver.Manage().Cookies.AddCookie(cookie); 
            return this;
        }
        public CsDriverService ToTab(string TabText, bool Launch = true, string FormName= "")
        { 
            var driver = this.Driver;
            IWebElement ele;
            WebDriverWait wait;
            if (!driver.Url.Contains("ReporterHome.aspx"))
            {
                this.ToUrl("~ReporterHome.aspx");
            }
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            var eles = wait.Until(drv => drv.FindElements(By.XPath($"//*[contains(@id, '_Surveys')]//*[contains(@class, 'rtsTxt')]")))?.Reverse();
            ele = (from e in eles where Regex.IsMatch(e.Text, TabText) || e.Text.Contains(TabText) select e).FirstOrDefault();
            ele?.Click();
            Thread.Sleep(1000);

            string FormLauncherXPATH = SettingsProvider.appSettings["FormLauncherXPATH"];
            if(SettingsProvider.TestSettings["DataCall"].ContainsKey("FormLauncherXPATH"))
            {
                FormLauncherXPATH = SettingsProvider.TestSettings["DataCall"]["FormLauncherXPATH"];
            }
            else if(this.ElementExists(By.XPath("//li[contains(@class, 'rtsSelected')]//span[contains(text(), '18-02')]")))
            {
                FormLauncherXPATH = SettingsProvider.appSettings["HVAFormLauncherXPATH"];
            }
           
            if (this.ElementExists(By.XPath(FormLauncherXPATH))){
                ele = wait.Until(d => {
                    return d.FindElement(By.XPath(FormLauncherXPATH)); 
                });
                ele = wait.Until(drv => drv.FindElement(By.XPath(FormLauncherXPATH)));
                ele?.Click();
                return this;
            }
            var FormExpanderXPATH = SettingsProvider.appSettings[$"FormExpanderXPATH"]; 
            if (this.ElementExists(By.XPath(FormExpanderXPATH)))
            {
                var elm = driver.FindElements(By.XPath(FormExpanderXPATH));
                elm[0].Click(); 
                try
                {
                    if (string.IsNullOrEmpty(FormName)) {
                        FormName = Automator.SettingsProvider.TestSettings["DataCall"]["Form"];
                    } 
                }
                catch (Exception ex)
                {
                    this.Logger.Error("{@Exception}", new { ex.Message });
                } 
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
                var element = driver.FindElement(By.XPath($"//tr[contains(@id, 'formsgrid')]//tr/td[contains(text(), '"+ FormName + "')]/../td/a"));
                element.Click();
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.0001);
                return this;
            }
             
            return this;

        }
        public CsDriverService ToSection(DataCallSection Section)  { 
            SelectElement se = new SelectElement(this.Driver.FindElementByCssSelector("*[id*='_ddl_Sections']")); 
            se?.Options.Where(o => o.Text.Contains(Section?.SectionText)).FirstOrDefault()?.Click(); 
            return this;
        }
        public CsDriverService ToSection(Func<DataCallSection, bool> Predicate) { 
            var section = this.Sections().Where(Predicate).FirstOrDefault();
            this.ToSection(section);
            return this; 
        }
        public CsDriverService ToSection(int Index)
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
        public CsDriverService SetFieldValue(By By, string value)
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
        public CsDriverService ApplyValidationAttempt(ValidationAttempt va, Action Assertion) {
            var ds = this; 
            var assms = AppDomain.CurrentDomain.GetAssemblies();
            var ElementValueProviders = (from assm in assms
                                         from t in assm.GetTypes()
                    where typeof(IElementValueProvider).IsAssignableFrom(t) && t.IsClass
                    select t).ToList();

            Type answerProvider = typeof(ElementValueProvider);
            ElementValueProviders.ForEach(t => {
                var attr = t.GetCustomAttribute<ElementValueProviderMeta>(false);
                if (!string.IsNullOrEmpty(attr?.XpathMatch))
                {
                    var e = this.GetElement(By.XPath(attr.XpathMatch)); 
                    if (e != null) answerProvider = t;
                } 
            });

            var sc = new SessionContext(ds.Logger, ds.Driver);

            IElementValueProvider obj = (IElementValueProvider)Activator.CreateInstance(answerProvider);
            ((IElementValueProvider)obj).Populate(sc);
            
            string attempt = ((IElementValueProvider)obj).Eval<string>(va.ErrorAttemptExpression);
            var Defaults = new DefaultInputProvider(ds.Driver).DefaultValues;
            Defaults.Add(va.MetricXpath, attempt); 
            sc.Defaults = Defaults;

            var pcc = ds.PageControlCollection().EmptyIfNull();
            ds.FismaFormEnable();

            string id = Utils.ExtractContainerId(ds.Driver, va.MetricXpath); 
            foreach (IAutomator control in pcc)
            { 
                ((IAutomator)control).ContainerSelector = !string.IsNullOrEmpty(id)? $"#{id} ": "";
                ((IAutomator)control).OnFormSubmitted += (s, e) =>
                {
                    ((IAutomator)s).AutomatorState = AutomatorState.AutomationComplete;
                };
                ((IAutomator)control).Automate(sc); 
            }
            ds.FismaFormSave();  
            Assertion();
            return this;
        }
        public CsDriverService InitSections(Func<DataCallSection, bool> SectionGroupPredicate )
        { 
            SessionContext sessionContext = new SessionContext() { 
                Driver = this.Driver
                , Logger = this.Logger 
                , Defaults = new DefaultInputProvider(this.Driver).DefaultValues
            };
      
            IElementValueProvider oElementValueProvider = (IElementValueProvider)Activator.CreateInstance(_getElementValueProviderType());
            
            foreach (DataCallSection section in this.Sections().Where(SectionGroupPredicate))
            {
                var appargs = new DriverServiceEventArgs(this);
                appargs.Section = section;
                this.ToSection(section);
                 
                foreach (IAutomator control in this.PageControlCollection().EmptyIfNull()){
                    var na = ((IAutomator)control).GetType().FullName.Contains("NaiveAutomator");
                    if (na) {
                        this.FismaFormEnable(); 
                    }   
                    ((IAutomator)control).Automate(sessionContext);
                    if (na) this.FismaFormSave();
                    ((IElementValueProvider)oElementValueProvider).Populate(sessionContext);
                     
                } 
                this.LogScreenshot(section.SectionText);
                SectionComplete(appargs);
            } 
            return this;
        }
        public void LogScreenshot(string log)
        {
            if(!SettingsProvider.appSettings.ContainsKey("ScreenshotLogDir")) 
                return;
          
            var ssdir = SettingsProvider.appSettings[$"ScreenshotLogDir"];
            var ssPre = SettingsProvider.appSettings[$"ScreenshotPre"];
            var ssPost = SettingsProvider.appSettings[$"ScreenshotPost"];
            if (!string.IsNullOrWhiteSpace(ssdir))
            { 
                ((IJavaScriptExecutor)this.Driver).ExecuteScript("scroll(0, -250)");  
                var uri = (this.Driver.Url.Contains("?")) ? this.Driver.Url.Substring(1, this.Driver.Url.IndexOf("?")) : this.Driver.Url;
                uri = (from s in uri.Split('/') where s.Contains(".") select s).FirstOrDefault();
                uri = Regex.Replace(uri, @"[^\w\d_]", "");
                uri = (uri.Length >= 50) ? uri.Substring(1, 50) : uri;
                log = Regex.Replace(log, @"[^\w\d_]", "");
                log = (log.Length >= 50) ? log.Substring(1, 49) : log;
                ssdir = ssdir.Replace("{log}", log);
                ssdir = ssdir.Replace("{uri}", uri);
                ssdir = ssdir.Replace("{date}", DateTime.Now.ToString("yyyy_MM_dd"));
                var dir = string.Join($"\\", (from d in ssdir.Split('\\') where !d.Contains(".") select d));
                var di = new DirectoryInfo(dir);
                if (!di.Exists)
                {
                    return;
                }

                if (!string.IsNullOrWhiteSpace(ssPre))
                    ((IJavaScriptExecutor)this.Driver).ExecuteScript(ssPre);
                 
                Screenshot ss = ((ITakesScreenshot)this.Driver).GetScreenshot(); 
                ss.SaveAsFile($"{ssdir}", ScreenshotImageFormat.Png);
               
                if (!string.IsNullOrWhiteSpace(ssPost))
                    ((IJavaScriptExecutor)this.Driver).ExecuteScript(ssPost);

            }
        }
        public CsDriverService OpenTab() { 
            string url = this.Driver.Url;
            ((IJavaScriptExecutor)this.Driver).ExecuteScript("window.open();");
            var handles = this.Driver.WindowHandles;
            this.Driver.SwitchTo().Window(handles[this.Driver.WindowHandles.Count - 1]);
            this.Driver.Navigate().GoToUrl($"{url}"); 
            return this;
        }
        public CsDriverService CloseTab()
        {
            var handles = this.Driver.WindowHandles;
            ((IJavaScriptExecutor)this.Driver).ExecuteScript("window.close();");
            this.Driver.SwitchTo().Window(handles[this.Driver.WindowHandles.Count - 1]);
            return this;
        }
        #endregion
        #region METHODS: FismaForm ACCESSORS 
        public CsDriverService FismaFormEnable()
        {
            string btntext = "CBButtPanel1_btnEdit";
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(1)); 
            var ele = wait.Until(drv =>  drv.FindElement(By.XPath($"//*[contains(@id, '{btntext}')]")));
            ((IJavaScriptExecutor)this.Driver).ExecuteScript("arguments[0].click();", ele); 
            return this;
        }
        public CsDriverService FismaFormCancel()
        {
            return this.FismaFormEnable();
        }
        public CsDriverService FismaFormSave()
        {
            string btntext = "_btnSave";
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(1));
            var ele = wait.Until(drv => drv.FindElement(By.XPath($"//*[contains(@id, '{btntext}')]")));
            ((IJavaScriptExecutor)this.Driver).ExecuteScript("arguments[0].click();", ele);
            return this; 
        }
        public bool FismaFormValidates() {
            this.ToSection(-1);
            LogScreenshot("FismaFormValidates");
            var success = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(5))
            .Until(dvr => dvr.FindElements(By.CssSelector("#ctl00_ContentPlaceHolder1_lblSuccessInfo")));
            if (success.Count() > 0){
                return success[0].Text.Contains("Your form has been validated and contains no errors.");
            }    
            this.Logger.Warning($"FismaForm InValid");
            return false;
        }
        #endregion
        #region METHODS: PRIV
        private Type _getElementValueProviderType(){
            var assms = AppDomain.CurrentDomain.GetAssemblies();
            var ElementValueProviders = (from assm in assms
                                         from t in assm.GetTypes()
                                         where typeof(IElementValueProvider).IsAssignableFrom(t) && t.IsClass
                                         select t).ToList();

            Type answerProvider = typeof(ElementValueProvider);
            ElementValueProviders.ForEach(t => {
                var attr = t.GetCustomAttribute<ElementValueProviderMeta>(false);
                if (!string.IsNullOrEmpty(attr?.XpathMatch))
                {
                    var e = this.GetElement(By.XPath(attr.XpathMatch));
                    if (e != null) answerProvider = t;
                }
            });
            return answerProvider;
        }
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