using CyberScope.Automator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TinyCsvParser;
using TinyCsvParser.Mapping;
using TinyCsvParser.Model;
using Xunit.Sdk;

namespace CyberScope.Automator
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CsvInitDataAttribute : BaseDataAttribute
    { 
        #region METHODS 
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            string _fileName = GetFileName(testMethod);
            var csvParser = new CsvParser<InitializeAttempt>(
                   this.parseOptions, new InitializeAttemptMapper());
            var result = csvParser.ReadFromFile(_fileName, Encoding.ASCII).ToList(); 
            foreach (var item in result) 
                yield return item.Result.GetAsRow; 
        }
        #endregion 
    }  
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CsvValidationDataAttribute : BaseDataAttribute
    { 
        #region METHODS 
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            string _fileName = GetFileName(testMethod);
            var csvParser = new CsvParser<ValidationAttempt>(
                  this.parseOptions, new ValidationAttemptMapper());
            var result = csvParser.ReadFromFile(_fileName, Encoding.ASCII).ToList();
            foreach (var item in result)
                yield return item.Result.GetAsRow;
        } 
        #endregion 
    } 
    public abstract class BaseDataAttribute : DataAttribute
    {
        protected CsvParserOptions parseOptions { get; set; } = new CsvParserOptions(true, ',');
        protected string _filename = ""; 
        public string GetFileName (MethodInfo mi){
            var atts = mi.CustomAttributes;
            if (string.IsNullOrEmpty(_filename))
            {
                _filename =   Utils.GetDataPath()
                    .Replace("{Type}", mi.DeclaringType.Name)
                    .Replace("{TestMethod}", mi.Name);
                if (!_filename.EndsWith(".csv"))
                    _filename = $"{_filename}.csv"; 
            }
            return _filename;
        } 
    }
}
