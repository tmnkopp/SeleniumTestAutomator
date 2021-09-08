using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberScope.Tests.Selenium
{
    public interface IValueSetter {
        void SetValue(ChromeDriver driver, string ElementId);
        IWebElement Element { get; set; }
        Dictionary<string, string> Defaults { set; } 
        bool Overwrite { get; set; }
    }
    public abstract class BaseValueSetter
    {
        protected Dictionary<string, string> defaults;
        public Dictionary<string, string> Defaults { set => defaults = value; protected get => defaults; }
        public IWebElement Element { get; set; }
        public bool Overwrite { get; set; } = true;
        protected IList<IWebElement> inputs;
        
        protected WebDriverWait wait; 
        protected string GetMatchAttribute() {
            IWebElement input = Element;
            var target = $"{input.GetAttribute("name")} " +
                $"{input.GetAttribute("id")} " +
                $"{input.GetAttribute("class")} " +
                $"type:{input.GetAttribute("type")} ";
            return target; 
        }
    } 
}
