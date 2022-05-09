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
                $" {input.GetAttribute("class")} ";
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
            wait = new WebDriverWait(sessionContext.Driver, TimeSpan.FromSeconds(2));
            IWebElement elm = wait.Until(drv => drv.FindElement(By.XPath(xpath))); 
            string avalue = elm.GetAttribute("value") ?? "";
            string @class = elm.GetAttribute("class") ?? "";
            try
            {
                elm = wait.Until(drv => drv.FindElement(By.XPath("//select/option[@selected='selected']")));
                if (elm != null)
                    section = elm?.Text; 
            }
            catch (Exception)
            {
                sessionContext.Logger.Warning("ValueSetter //select/option not found");
            } 
            xpath = string.Format("{2},//*[@id='{0}' and @class='{3}'],{1},\"\"", ElementId,  avalue, section, @class);
            sessionContext.Logger.Information(xpath);
        }
    } 
}