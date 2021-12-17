using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CyberScope.Tests.Selenium
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    internal class AutomatorMeta : Attribute {
        public Dictionary<string, string> Locators { get; set;  }
        public AutomatorMeta( )
        { 
        }
    }
    internal interface IAutomator
    { 
        string ContainerSelector { get; set; }
        List<IValueSetter> ValueSetters { get; set; } 
        void Automate(SessionContext context); 
    }
    internal class SessionContext 
    {
        public ILogger Logger { get ; set; }
        public ChromeDriver Driver { get; set; }
        public OrgSubmission OrgSubmission { get; set; }
    }
    public class AutomatorEventArgs : EventArgs
    {
        public AutomatorEventArgs(ChromeDriver driver)
        {
            this.Driver = driver;
        }
        public ChromeDriver Driver { get; set; }
        public string CurrentWindowHandle { get; set; }
    }
    internal abstract class BaseAutomator
    {
        #region CTOR

        public BaseAutomator()
        {
            //TODO INJECT THIS
            valueSetters = new List<IValueSetter>();
            var setters = (from assm in AppDomain.CurrentDomain.GetAssemblies()
                           where assm.FullName.Contains(AppDomain.CurrentDomain.FriendlyName)
                           from t in assm.GetTypes()
                           where typeof(IValueSetter).IsAssignableFrom(t)
                           && t.IsClass
                           select t).ToList();
            foreach (var type in setters)
                valueSetters.Add((IValueSetter)Activator.CreateInstance(Type.GetType($"{type.FullName}")));
        }
        public BaseAutomator(List<IValueSetter> valueSetters)
        {
            this.valueSetters = valueSetters;
        }
        #endregion

        #region PROPS 
        protected ChromeDriver driver;
        protected WebDriverWait wait;
        protected IList<IWebElement> inputs;
        protected IWebElement input; 
        //private string _PK_FORM; 
        //public string PK_FORM { get => _PK_FORM; set => _PK_FORM = value;  }
        protected List<IValueSetter> valueSetters;
        public List<IValueSetter> ValueSetters { get => valueSetters; set => valueSetters = value;   } 
        protected string container = "div[id*='ctl00_ContentPlaceHolder1_Panel'] .table";
        public string ContainerSelector
        {
            get { return container;  }
            set { container = value; }
        }
        #endregion

        #region Events 
        public event EventHandler<AutomatorEventArgs> OnPreAutomate;
        protected virtual void PreAutomate(AutomatorEventArgs e)
        {
            OnPreAutomate?.Invoke(this, e);
        }
        public event EventHandler<AutomatorEventArgs> OnPostAutomate;
        protected virtual void PostAutomate(AutomatorEventArgs e)
        {
            OnPostAutomate?.Invoke(this, e);
        }
        public event EventHandler<AutomatorEventArgs> OnStaleElement;
        protected virtual void StaleElement(AutomatorEventArgs e)
        {
            OnStaleElement?.Invoke(this, e);
        }
        #endregion
    } 
    
}
