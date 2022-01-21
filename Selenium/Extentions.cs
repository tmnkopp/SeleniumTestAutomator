using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberScope.Tests.Selenium
{
    public static class Extentions
    {
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }
    }
    public static class Assm
    {
        public static IEnumerable<Type> GetTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(assm => assm.GetTypes()).Where(t => t.IsClass);
        }
    } 
}
