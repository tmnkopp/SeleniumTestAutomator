using NCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CyberScope.Tests.Selenium.Providers 
{
    public abstract class MetricProvider
    {
 
        protected Dictionary<string, string> _Answers = new Dictionary<string, string>();
        public void SetMetric(string key, string val)
        {
            if (!_Answers.ContainsKey(key))
                _Answers.Add(key, val);
        }
        public T Eval<T>(string Expression)
        {
            foreach (var kv in _Answers)
                Expression = Expression.Replace($"{kv.Key} ", $"{_Answers[kv.Key]} ");
            var evaled = new Expression(Expression).Evaluate(); 
            return (T)Convert.ChangeType(evaled, typeof(T));
        }
        public T GetMetric<T>(string key)
        {
            object value = (_Answers.ContainsKey(key)) ? _Answers[key] : null;
            return (T)Convert.ChangeType(value, typeof(T)); 
        }
        public virtual void Populate(DriverService ds) {
            var mEles = ds.Driver.FindElementsByXPath("//span[contains(@class, 'qid_')]");
            foreach (var item in mEles)
            {
                string cls = item.GetAttribute("class");
                string m = Regex.Match(cls, $"qid_[\\d_]+")?.Groups[0]?.Value ?? "0";
                SetMetric(m, item.Text);
            }
        }
    }
}
