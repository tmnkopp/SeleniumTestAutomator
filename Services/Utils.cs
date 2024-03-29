
using NCalc;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CyberScope.Automator
{
    public static class Utils
    { 
        public static string ExtractContainerId(ChromeDriver driver, string metricXpath) {
            string id = "";
            int ittr = 0; 
            while (string.IsNullOrEmpty(id)) {
                ittr++;
                metricXpath = $"{metricXpath}/.."; 
                var ele = driver.FindElementsByXPath($"{metricXpath}");
                if (ele.Count > 0) 
                    id = ele[0]?.GetAttribute("id") ?? ""; 
                if (ittr > 3) 
                    break; 
            }
            return id; 
        }
        public static void TryEval(string EvalExpression, out object Result)
        {
            try
            {
                Result = new Expression(EvalExpression).Evaluate();
            }
            catch (Exception ex)
            {
                Result = EvalExpression;
                Console.WriteLine(ex.Message);
            }
        } 
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string ReverseRegex(string value)
        {  
            while (value.Contains(@"\d"))
                value = value.Replace(@"\d", new Random().Next(10).ToString());
            while (value.Contains(@"\w")) 
                value = value.Replace(@"\w", Utils.RandomString(1)); 
            return value;
        }
    }
}