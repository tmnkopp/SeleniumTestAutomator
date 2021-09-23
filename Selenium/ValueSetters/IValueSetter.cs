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
        public ValueSetterMeta()
        { 
        }
    } 
    public interface IValueSetter {
        void SetValue(ChromeDriver driver, string ElementId); 
        Dictionary<string, string> Defaults { set; } 
        bool Overwrite { get; set; }
    }
    public abstract class BaseValueSetter
    {
        protected Dictionary<string, string> defaults;
        public Dictionary<string, string> Defaults { set => defaults = value; protected get => defaults; } 
        public bool Overwrite { get; set; } = true;
        protected IList<IWebElement> inputs;
        
        protected WebDriverWait wait; 
        protected string GetMatchAttribute(IWebElement input) { 
            var target = $"{input.GetAttribute("name")} " +
                $"{input.GetAttribute("id")} " +
                $"{input.GetAttribute("class")} " +
                $"type:{input.GetAttribute("type")} ";
            return target; 
        }
    } 
}
