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
        private KeyLengthSortedDecendingDictionary _Answers = new KeyLengthSortedDecendingDictionary();
        public void SetMetric(string key, string val)
        {
            key = key.Replace(".", "_");
            if (!Regex.IsMatch(key, "^\\w+_"))
                key = $"qid_{key}";

            if (!_Answers.ContainsKey(key))
                _Answers.Add(key, val);
        }
        public T Eval<T>(string EvalExpression)
        {
            foreach (var kv in _Answers) {
                EvalExpression = EvalExpression.Replace($"{kv.Key}", $" {_Answers[kv.Key]} ");
            } 
            if (Regex.IsMatch(EvalExpression,$@"\w+" ) || string.IsNullOrEmpty(EvalExpression))
            {
                return (T)Convert.ChangeType(EvalExpression, typeof(string));
            }
            object evaled = EvalExpression;
            try
            {
                evaled = new Expression(EvalExpression).Evaluate();
            }
            catch (Exception ex)
            { 
                throw ex;
            } 
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
                string m = Regex.Match(cls, $"qid_[\\d_\\w]+")?.Groups[0]?.Value ?? "0";
                SetMetric(m, item.Text);
            }
        }
        private class KeyLengthSortedDecendingDictionary : SortedDictionary<string, string>
        {
            private class StringLengthComparer : IComparer<string>
            {
                public int Compare(string x, string y)
                {
                    if (x == null) throw new ArgumentNullException(nameof(x));
                    if (y == null) throw new ArgumentNullException(nameof(y));
                    var lengthComparison = x.Length.CompareTo(y.Length) * -1;
                    return lengthComparison == 0 ? string.Compare(x, y, StringComparison.Ordinal) : lengthComparison;
                }
            }
            public KeyLengthSortedDecendingDictionary() : base(new StringLengthComparer()) { }
        }
    }  
}
