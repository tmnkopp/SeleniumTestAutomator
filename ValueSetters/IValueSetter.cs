using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace CyberScope.Automator
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
            var target = $"type:{input.GetAttribute("type")} " +  
                $" {input.GetAttribute("id")}" +
                $" {input.GetAttribute("class").Trim()}";
            target = Regex.Replace(target, @"\s{2,}", " ");
            return target; 
        }
        protected string GetIdText(IWebElement input)
        {
            string ret = $" {input.GetAttribute("class")} ";
            var m = Regex.Match(ret, @"qid_([\._\w]+)");
            if (m.Success) 
                return m.Value; 
            return null;
        }
        protected void Log(SessionContext sessionContext, string ElementId)
        {
            string xpath = string.Format("//*[@id='{0}']", ElementId);
            string section = "";
            wait = new WebDriverWait(sessionContext.Driver, TimeSpan.FromSeconds(.5)); 
            try
            {
                IWebElement elm = wait.Until(drv => drv.FindElement(By.XPath(xpath)));
                string avalue = elm.GetAttribute("value") ?? "";
                string @class = elm.GetAttribute("class") ?? "";
                var sectionElement = wait.Until(drv => drv.FindElements(By.XPath("//select/option[@selected='selected']")));
                if (sectionElement.Count > 0)
                    section = sectionElement[0]?.Text;
                var m = Regex.Match(@class, $@"qid_([\._\w]+)");
                if (m.Success)  
                    xpath = string.Format("{2},//*[@id='{0}' and contains(@class,'{3}')],{1},\"\"", ElementId, avalue, section, m.Value);
                else 
                    xpath = string.Format("{2},//*[@id='{0}'],{1},\"\"", ElementId, avalue, section);
                sessionContext.Logger.Verbose(xpath);
            }
            catch (Exception)
            {
                sessionContext.Logger.Warning("ValueSetter //select/option not found");
            } 

        }
    } 
}