
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace CyberScope.Automator.Providers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ElementValueProviderMeta : Attribute
    { 
        public string XpathMatch { get; set; }
        public ElementValueProviderMeta(string XpathMatch)
        {
            this.XpathMatch = XpathMatch;
        }
    }
    public interface IElementValueProvider
    {
        void Populate(SessionContext sessionContext); 
        T Eval<T>(string EvalExpression);
    }

    public class ElementValueProvider: IElementValueProvider
    {
        public ElementValueProvider()
        { 
        }
        public Dictionary<string, string> Answers => _answers.ToDictionary(x => x.Key, x => x.Value); 
        protected KeyLengthSortedDecendingDictionary _answers = new KeyLengthSortedDecendingDictionary();
 
        public T Eval<T>(string EvalExpression)
        {
            foreach (var kv in _answers) {
                EvalExpression = EvalExpression.Replace($"{kv.Key}", $" {_answers[kv.Key]} ");
            }
            EvalExpression = Regex.Replace(EvalExpression, $@"(Numeric)", "0");  
            object Result;
            Utils.TryEval(EvalExpression, out Result); 
            return (T)Convert.ChangeType(Result, typeof(T));
        } 
        public T GetMetric<T>(string key)
        {
            object value = (_answers.ContainsKey(key)) ? _answers[key] : null;
            return (T)Convert.ChangeType(value, typeof(T)); 
        }
        public virtual void Populate(SessionContext sessionContext) {
            try
            {
                var rows = sessionContext.Driver.FindElementsByXPath("//td/span[contains(@class,'CustomControlValue')]/../..");
                foreach (IWebElement row in rows)
                {
                    var idt = (
                        from col in row.FindElements(By.XPath(".//td"))
                        where Regex.IsMatch(col.Text.Trim(), @"^[\d\.a-zA-Z]{1,15}$")
                        select col).FirstOrDefault();
                    var ans = row.FindElement(By.XPath(".//td/span[contains(@class, 'CustomControlValue')]"));
                    if (!string.IsNullOrEmpty(idt?.Text) && !string.IsNullOrEmpty(ans?.Text)) 
                        SetMetric(idt.Text, ans.Text); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            } 
            var mEles = sessionContext.Driver.FindElementsByXPath("//span[contains(@class, 'qid_')]"); 
            foreach (var item in mEles)
            {
                string cls = item.GetAttribute("class");
                string m = Regex.Match(cls, $@"qid_([\._\w]+)")?.Groups[1]?.Value ?? "0";
                SetMetric(m, item.Text);
            }
        }
        public void SetMetric(string key, string val)
        {
            if (key.Contains("qid_"))
            {
                key = Regex.Replace(key, $@"qid_|\s", "");
                key = Regex.Replace(key, $@"_", ".");
            }
            if (!key.Contains("{"))
                key = "{"+ key + "}";
            if (!_answers.ContainsKey(key))
                _answers.Add(key, val);
        } 
    }  
}