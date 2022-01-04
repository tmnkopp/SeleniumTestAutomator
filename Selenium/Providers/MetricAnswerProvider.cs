using NCalc;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CyberScope.Tests.Selenium.Providers 
{
    public class CIOMetricProvider : MetricAnswerProvider
    {
        public override void Populate(DriverService ds)
        {
            ds.OpenTab();
            ds.ToSection((g => g.SectionText.Contains("S1")));

            string m111 = ds.GetFieldValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblfirst_Total')]")) ?? "0";
            string m112 = ds.GetFieldValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblSecond_Total')]")) ?? "0";
            string m115 = ds.GetFieldValue(By.XPath("//td/span[contains(text(), '1.1.5')]/../..//*[contains(@class, 'CustomControlValue')]")) ?? "0";
            SetMetric("1.1.1", m111);
            SetMetric("1.1.2", m112);
            SetMetric("1.1.5", m115);

            ds.CloseTab(); 

            ds.OpenTab(); 
            ds.ToSection((g => g.SectionText.Contains("S1B")));

            string m12 = ds.GetFieldValue(By.XPath("//td/span[contains(@id, '_Section1Sum')]")) ?? "0";
            SetMetric("1.2", m12);
            ds.CloseTab();

            base.Populate(ds);
        }
    }
    public interface IPopulateMetricAnswers
    { 
        void Populate(DriverService ds);
    }
    public class MetricAnswerProvider: IPopulateMetricAnswers
    { 
        private KeyLengthSortedDecendingDictionary _Answers = new KeyLengthSortedDecendingDictionary();
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
        public T Eval<T>(string EvalExpression)
        {
            foreach (var kv in _Answers) {
                EvalExpression = EvalExpression.Replace($"{kv.Key}", $" {_Answers[kv.Key]} ");
            }  
            object Result;
            Utils.TryEval(EvalExpression, out Result); 
            return (T)Convert.ChangeType(Result, typeof(T));
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
                string m = Regex.Match(cls, $@"qid_([\._\w]+)")?.Groups[1]?.Value ?? "0";
                SetMetric(m, item.Text);
            }
        }
        public void SetMetric(string key, string val)
        {
            key = Regex.Replace(key, $@"qid_|\s", "");
            key = Regex.Replace(key, $@"_", ".");  
            if (!_Answers.ContainsKey(key))
                _Answers.Add(key, val);
        } 
    }  
}
