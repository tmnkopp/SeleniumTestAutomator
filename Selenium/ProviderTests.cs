using Dapper;
using Dapper.Mapper;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
namespace CyberScope.Tests.Selenium
{
    public class ProviderTests
    {
        [Theory] 
        [InlineData("CSTargerUrl")]
        [InlineData("AgencyUser")]
        [InlineData("AgencyPass")]
        [InlineData("AdminUser")]
        [InlineData("AdminPass")]
        public void UserSettingsProvider_Provides(string setting) { 
            Assert.NotNull(ConfigurationManager.AppSettings.Get(setting));
        } 
        [Fact]
        public void ConfigProvider_Provides()
        { 
            Assert.NotNull(SettingsProvider.InputDefaults);
            string dkey = "CIO";
            Dictionary<string, string> InputDefaults = SettingsProvider.InputDefaults[$"Global"];
            if (SettingsProvider.InputDefaults.ContainsKey($"{dkey}"))
            {
                foreach (var item in SettingsProvider.InputDefaults[$"{dkey}"])
                {
                    if (InputDefaults.ContainsKey(item.Key))
                        InputDefaults[item.Key] = item.Value;
                    else
                        InputDefaults.Add(item.Key, item.Value);
                }
            }
            Assert.NotNull(InputDefaults);
        }
        [Fact]
        public void ControlLocatorsProvider_Provides()
        {
            var set = SettingsProvider.ControlLocators[0];
            Assert.IsType<ControlLocator>(set);
        }
        [Fact]
        public void OrgSubmissionProvider_Provides()
        {
            var orgsub = OrgSubmissionProvider.GetBySession("Bill-D-Robertson", "2021-A-SAOP").FirstOrDefault();
            Assert.NotNull(orgsub);
        }
        [Fact]
        public void Repository_Provides()
        {
            string dcall = "2020-A-HVA";
            var orgsub = OrgSubmissionProvider.GetBySession("Bill-D-Robertson", $"{dcall}").FirstOrDefault().PK_OrgSubmission;

            var questiongroups = Repository<QuestionGroup>.Get(qg=> qg.PK_Form == $"{dcall}");
            var questions = Repository<Question>.GetAll();
            var answers = Repository<FsmaAnswer>.Get(a=>a.FK_OrgSubmission==orgsub);

            var result = (from q in questions 
                          join qg in questiongroups on q.FK_QuestionGroup equals qg.PK_QuestionGroup
                          join ans in answers on q.PK_Question equals ans.FK_Question into q_ans
                          from q_a in q_ans.DefaultIfEmpty(new FsmaAnswer() { Answer="" })
                          where qg.PK_Form == $"{dcall}" && q.identifier_text != ""
                          select new { 
                                PK_Question = q.PK_Question
                              , identifier_text = q.identifier_text
                              , selector = q.selector
                              , Answer=q_a.Answer 
                          }).ToList();

            Assert.NotNull(result);
        }
    }
}
