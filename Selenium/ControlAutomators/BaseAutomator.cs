using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
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
        event EventHandler<AutomatorEventArgs> OnFormSubmitted; 
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
        public event EventHandler<AutomatorEventArgs> OnFormSubmitted;
        protected virtual void FormSubmitted(AutomatorEventArgs e)
        {
            OnFormSubmitted?.Invoke(this, e);
        } 
        #endregion
    } 
    
}
