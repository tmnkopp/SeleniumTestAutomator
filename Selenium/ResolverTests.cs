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
using SeleniumExtras.WaitHelpers;
namespace CyberScope.Tests.Selenium
{ 
    public class ResolverTests
    {
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
    } 
}
