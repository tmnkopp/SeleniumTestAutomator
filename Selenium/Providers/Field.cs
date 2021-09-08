using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberScope.Tests.Selenium
{ 
    public class Field
    {
        #region PROPS
         
        protected WebDriverWait wait; 
        private By _by;
        public By BySelector { get => _by; set => _by = value; }
        public IWebElement WebElement { get => GetElement();   }
        public string ElementId { get => WebElement.GetAttribute("id"); }
        public string ElementClass { get => WebElement.GetAttribute("class"); }

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
        #endregion 

        #region CTOR

        protected ChromeDriver driver;  
        public Field(ChromeDriver driver)
        { 
            this.driver = driver; 
        }
        public Field(ChromeDriver driver, By Selector)
        { 
            this.driver = driver;
            this.BySelector = Selector; 
        }

        #endregion

        #region METHODS

        public Predicate<IWebElement> ElementPredicate { get; set; } = (i) => (i.Enabled && i.Displayed);
        private IWebElement GetElement()
        {
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            return (from i in wait.Until(dvr => dvr.FindElements(_by))
                         where ElementPredicate(i)
                         select i).ToList().FirstOrDefault(); 
        }

        public virtual string GetValue()
        {
            if (this.driver == null) throw new ArgumentNullException("Null Driver Exception");
            var automatorEventArgs = new AutomatorEventArgs(driver);
            automatorEventArgs.CurrentWindowHandle = driver.CurrentWindowHandle;
            PreAutomate(automatorEventArgs);
            IWebElement input;
            try
            {
                input = GetElement();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                PostAutomate(automatorEventArgs);
            } 
            return input?.Text;
        }
        public virtual void SetValue(string Value)
        {
            if (this.driver == null) throw new ArgumentNullException("Null Driver Exception");
            var automatorEventArgs = new AutomatorEventArgs(driver);
            automatorEventArgs.CurrentWindowHandle = driver.CurrentWindowHandle;
            PreAutomate(automatorEventArgs); 
            try
            {
                var input = GetElement();
                input.Clear();
                input.SendKeys($"{ Value }"); 
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {  
                PostAutomate(automatorEventArgs);
            }
        } 
        #endregion 
    }
}
