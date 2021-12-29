﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace CyberScope.Tests.Selenium
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CsvDataAttribute : DataAttribute
    {
        private string _fileName;
        public CsvDataAttribute(string fileName)
        {
            _fileName = fileName; 
        }
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (string.IsNullOrEmpty(_fileName))
            {
                _fileName = $"{ConfigurationManager.AppSettings.Get($"TestDataDir")}{testMethod.DeclaringType.Name}_{testMethod.Name}.csv";
            }
            var pars = testMethod.GetParameters();
            var parameterTypes = pars.Select(par => par.ParameterType).ToArray();
            using (var csvFile = new StreamReader(_fileName))
            {
                // csvFile.ReadLine(); Delimiter Row: "sep=,". Comment out if not used
                // csvFile.ReadLine(); Headings Row. Comment out if not used
                string line;
                while ((line = csvFile.ReadLine()) != null)
                {
                    var row = line.Split(',');
                    yield return ConvertParameters((object[])row, parameterTypes);
                }
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
    }
}