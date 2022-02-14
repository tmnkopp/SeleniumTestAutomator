using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TinyCsvParser;
using Xunit.Sdk;

namespace CyberScope.Tests.Selenium
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CsvInitDataAttribute : CsDataAttribute
    {
        #region CTOR 
        public CsvInitDataAttribute()
        {
        }
        public CsvInitDataAttribute(string FileName) : base(FileName)
        {
        }
        #endregion

        #region METHODS 
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            string _fileName = GetFileName(testMethod);
            var csvParser = new CsvParser<InitializeAttempt>(
                  new CsvParserOptions(true, ',')
                , new CsvInitializeAttemptMapping()
                );
            var result = csvParser.ReadFromFile(_fileName, Encoding.ASCII).ToList();
            foreach (var item in result)
            {
                yield return item.Result.GetAsRow;
            }
        }
        #endregion

    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CsvDataAttribute : CsDataAttribute
    {
        #region CTOR 
        public CsvDataAttribute()
        {
        }
        public CsvDataAttribute(string FileName) : base(FileName)
        { 
        }
        #endregion

        #region METHODS 
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            string _fileName = GetFileName(testMethod); 
            var csvParser = new CsvParser<ValidationAttempt>(
                  new CsvParserOptions(true, ',')
                , new CsvValidationAttemptMapping()
                );
            var result = csvParser.ReadFromFile(_fileName, Encoding.ASCII).ToList();
            foreach (var item in result)
            {
                yield return item.Result.GetAsRow;
            }
        } 
        #endregion

    } 
    public abstract class CsDataAttribute : DataAttribute
    {
        private string _filename = ""; 
        public string GetFileName (MethodInfo mi){
            if (string.IsNullOrEmpty(_filename))
            {
                _filename = ConfigurationManager.AppSettings.Get($"TestDataDir")
                    .Replace("{Type}", mi.DeclaringType.Name)
                    .Replace("{TestMethod}", mi.Name);
                if (!_filename.EndsWith(".csv"))
                    _filename = $"{_filename}.csv"; 
            }
            return _filename;
        }
        public CsDataAttribute()
        {

        }
        public CsDataAttribute(string fileName)
        {
            _filename = fileName;
        } 
        private static object[] ConvertParameters(IReadOnlyList<object> values, IReadOnlyList<Type> parameterTypes)
        {
            var result = new object[parameterTypes.Count];
            for (var idx = 0; idx < parameterTypes.Count; idx++)
            {
                result[idx] = ConvertParameter(values[idx], parameterTypes[idx]);
            }
            return result;
        }
        private static object ConvertParameter(object parameter, Type parameterType)
        {
            return parameterType == typeof(int) ? Convert.ToInt32(parameter) : parameter;
        }

    }
}
