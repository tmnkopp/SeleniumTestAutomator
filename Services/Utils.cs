
using NCalc;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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
        public static string GetBasePath()
        {
            //Path.GetDirectoryName(Environment.CurrentDirectory)
            //Directory.GetCurrentDirectory()
            string path = System.IO.Path.GetDirectoryName(Environment.CurrentDirectory);  // Environment.CurrentDirectory;
            while (path.Contains("\\bin")) path = Directory.GetParent(path).ToString(); 
            return path;
        }
        public static string GetDataPath( )
        { 
            string path = Utils.GetBasePath() + @"\Data\" ;
            return path;
        } 
        public static string GetDownloadsPath()
        {
            var guid = new Guid("374DE290-123F-4565-9164-39C4925E467B");
            return SHGetKnownFolderPath(guid, 0);
        }
        [DllImport("shell32", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        private static extern string SHGetKnownFolderPath(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, int hToken = 0);
    }
}