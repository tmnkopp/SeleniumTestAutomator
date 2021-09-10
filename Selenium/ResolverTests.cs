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
        [Theory]
        [InlineData("2021-Q4-CIO","3.9.1")]
        [InlineData("2021-Q4-CIO", "3.9.2")]
        [InlineData("2021-Q4-CIO", "3.10")]
        [InlineData("2021-Q4-CIO", "3.10.1")]
        public void TypeCodeResolver_Resolves(string PK_FORM, string identifier_text)
        {
            var query = RepCycRepo.Instance
                .Query(r=>r.FormMaster.PK_Form == PK_FORM)
                .SelectMany(r=>r.FormMaster.QuestionGroups)
                .SelectMany(q=>q.Questions)
                .Where(a=>a.identifier_text == identifier_text).FirstOrDefault();
 
            var QuestionTypeCode = query.QuestionType.code;
            Assert.NotNull(QuestionTypeCode);
        }
        [Theory]
        [InlineData("S2C: Protect - Identity and Access Management")]
        [InlineData("2E:")]
        [InlineData("S3")] 
        public void SectionResolver_Resolves(string section)
        {
            Func<QuestionGroup, bool> Predicate = q => q.Text.Contains(section); 
            string _PK_FORM = "2021-Q3-CIO";
            var datacall =
             (from rc in RepCycRepo.Instance.Query(r => r.FormMaster.PK_Form == _PK_FORM)
              select new
              {
                  Tab = rc.Description,
                  Section = (
                    from qg in rc.FormMaster.QuestionGroups.Where(Predicate)
                    select new { qg.Text, qg.GroupName }).FirstOrDefault()
              }).FirstOrDefault();
            Assert.NotNull(datacall.Section.GroupName);
          
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
