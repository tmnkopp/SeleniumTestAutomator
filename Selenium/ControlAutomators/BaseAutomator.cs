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
    public class AutomatorMeta : Attribute {
        public Dictionary<string, string> Locators { get; set;  }
        public AutomatorMeta( )
        { 
        }
    }
    public interface IAutomator
    {
        string PK_FORM { get; set; }
        string ContainerSelector { get; set; }
        void Automate(ChromeDriver driver);
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
    public abstract class BaseAutomator
    { 
        #region PROPS 
        protected ChromeDriver driver;
        protected WebDriverWait wait;
        protected IList<IWebElement> inputs;
        protected IWebElement input; 
        private string _PK_FORM; 
        public string PK_FORM { get => _PK_FORM; set => _PK_FORM = value;  }
        public string DataCall {
            get {
                var lst = PK_FORM?.Split('-');
                return (lst==null) ? "" : lst[lst.Count() - 1]; 
            }   
        }
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
