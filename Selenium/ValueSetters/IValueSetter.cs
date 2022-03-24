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
    public class ValueSetterMeta: Attribute
    {
        public string Selector { get; set; }
        public string XPATH { get; set; }
        public ValueSetterMeta()
        { 
        }
    } 
    public interface IValueSetter {
        void SetValue(SessionContext sessionContext, string ElementId);  
        bool Overwrite { get; set; }
    }
    public abstract class BaseValueSetter
    { 
        public bool Overwrite { get; set; } = true;
        protected IList<IWebElement> inputs;
        
        protected WebDriverWait wait; 
        protected string GetMatchAttribute(IWebElement input) { 
            var target = $"type:{input.GetAttribute("type")}  " + 
                $" {input.GetAttribute("id")} " +
                $" {input.GetAttribute("class")} ";
            return target; 
        }
    } 
}
