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
        public void InputDefaultProvider_Provides()
        {
            Assert.NotNull(SettingsProvider.InputDefaults);
            Assert.NotNull(SettingsProvider.InputDefaults["~unittest~"]);
        }  
        [Fact]
        public void Dapper_RepCycRepo_Provides()
        {
            List<ReportingCycle> result;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CAClientConnectionString"].ConnectionString))
            {
                const string sql = @" 
                    SELECT RC.PK_ReportingCycle , RC.Description , 
                    FM.FK_ReportingCycle, FM.PK_FORM, FM.Report_Year, FM.IntervalCode, FM.Form_Description,  
                    QG.PK_QuestionGroup, QG.PK_Form, QG.GroupName GroupName, QG.sortpos, QG.Text Text,
                    (	SELECT TOP 1 FP.TEXT FROM fsma_FormPages FP 
	                    WHERE FM.PK_Form=FP.FK_Form
	                    AND QG.sortpos=FP.sortpos
                    )   SectionText ,
                    Q.PK_Question, Q.FK_QuestionGroup, Q.QuestionText,  Q.FK_QuestionType, Q.identifier_text,
                    QT.PK_QuestionTypeId, QT.CODE
                    FROM fsma_ReportingCycles RC
                    INNER JOIN fsma_FormMaster FM ON RC.PK_ReportingCycle=FM.FK_ReportingCycle
                    INNER JOIN fsma_QuestionGroups QG ON FM.PK_Form=QG.PK_Form
                    INNER JOIN fsma_Questions Q ON QG.PK_QuestionGroup=Q.FK_QuestionGroup
                    INNER JOIN fsma_QuestionTypes QT ON QT.PK_QuestionTypeId=Q.FK_QuestionType
                    WHERE RC.Year > 2019
                    ORDER BY PK_ReportingCycle DESC, PK_QuestionGroup DESC
                ";
                var lookup = new Dictionary<int, ReportingCycle>();
                var lookup2 = new Dictionary<int, QuestionGroup>();
                conn.Open();
                conn.Query<ReportingCycle, FormMaster, QuestionGroup, Question, QuestionType, ReportingCycle>(
                        sql,
                        (rc, fm, qg, q, qt) =>
                        {
                            ReportingCycle reportingCycle;
                            if (!lookup.TryGetValue(rc.PK_ReportingCycle, out reportingCycle))
                            {
                                reportingCycle = rc;
                                reportingCycle.FormMaster = fm;
                                lookup.Add(reportingCycle.PK_ReportingCycle, reportingCycle);
                            }
                            QuestionGroup questionGroup;
                            if (!lookup2.TryGetValue(qg.PK_QuestionGroup, out questionGroup))
                            {
                                questionGroup = qg;
                                reportingCycle.FormMaster.QuestionGroups.Add(questionGroup);
                                lookup2.Add(questionGroup.PK_QuestionGroup, questionGroup);
                            }
                            q.QuestionType = qt;
                            reportingCycle.FormMaster.QuestionGroups.Last().Questions.Add(q);

                            return reportingCycle;
                        }, splitOn: "PK_ReportingCycle,FK_ReportingCycle,PK_Form,PK_QuestionGroup,PK_Question,PK_QuestionTypeId").Distinct().ToList();
                result = lookup.Values.ToList();
            } 
            Assert.NotNull(result);
        }   
        [Fact]
        public void CompositeControlDetector_Provides()
        {
            var automators = new List<IAutomator>(); 
            var ds = new DriverService();
            ds.CsConnect(UserContext.Agency)
                .ToTab("2021-Q3-CIO")
                .ToSection((s => s.SectionText.Contains("S1A")));
            var driver = ds.Driver;

            foreach (IAutomator automator in ds.DetectedCompositeControls()) 
                Assert.NotNull(automator);  
        }     
    } 
}
