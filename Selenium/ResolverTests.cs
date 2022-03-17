using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using SeleniumExtras.WaitHelpers;
using Serilog;
using Serilog.Events;
using CyberScope.Data.Tests;

namespace CyberScope.Tests.Selenium
{
    public class ResolverTests
    {
        ILogger _logger;
        private readonly ITestOutputHelper output;
        public ResolverTests(ITestOutputHelper output)
        {
            this.output = output;
        }
  
        
        [Fact]
        public void TextInputValueSetter_Resolves( ) {
            var o = new TextInputValueSetter();
            var attr  =  (ValueSetterMeta)Attribute.GetCustomAttribute(o.GetType(), typeof(ValueSetterMeta));
       
            var setters = (from assm in AppDomain.CurrentDomain.GetAssemblies()
                         where assm.FullName.Contains(AppDomain.CurrentDomain.FriendlyName)
                         from t in  assm.GetTypes()
                         where typeof(IValueSetter).IsAssignableFrom(t) 
                         && t.IsClass
                         select t).ToList();

            List<IValueSetter> valueSetters = new List<IValueSetter>();
            foreach (var type in setters)
            { 
                valueSetters.Add((IValueSetter)Activator.CreateInstance(Type.GetType($"{type.FullName}"))); 
            }
            foreach (var item in valueSetters)
            {
                var meta = (ValueSetterMeta)Attribute.GetCustomAttribute(item.GetType(), typeof(ValueSetterMeta));
                var selector = string.Format(meta.Selector, "test");
            }


            var vs = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IValueSetter).IsAssignableFrom(p)).ToList();

            var vss = vs;
        } 
                
        [Fact]
        public void Logger_logs( ) {
            _logger = new LoggerConfiguration()   
                .WriteTo.TestOutput(output, LogEventLevel.Verbose)
                .CreateLogger();
            _logger.Information("Information");
            _logger.Error("Error");
            _logger.Fatal("Fatal");  
        }  
    } 
}
