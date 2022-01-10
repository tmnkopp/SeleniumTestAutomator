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
    public class CsvDataAttribute : DataAttribute
    {
        #region CTOR

        private string _fileName;
        public CsvDataAttribute()
        {
        }
        public CsvDataAttribute(string fileName)
        {
            _fileName = fileName;
        }

        #endregion

        #region METHODS 
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (string.IsNullOrEmpty(_fileName))
            {
                _fileName = ConfigurationManager.AppSettings.Get($"TestDataDir")
                    .Replace("[Type]", testMethod.DeclaringType.Name)
                    .Replace("[TestMethod]", testMethod.Name);
                if (!_fileName.EndsWith(".csv"))
                    _fileName = $"{_fileName}.csv";
            }
            var csvMapper = new CsvValidationAttemptMapping();
            var csvParserOptions = new CsvParserOptions(true, ',');
            CsvParser<ValidationAttempt> csvParser = new CsvParser<ValidationAttempt>(csvParserOptions, csvMapper);
            var result = csvParser.ReadFromFile(_fileName, Encoding.ASCII).ToList();
            foreach (var item in result)
            {
                yield return item.Result.GetAsRow;
            }
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
         
        #endregion

    }
}
