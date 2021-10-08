using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberScope.Tests
{
    public static class Repository<T> where T : class, new()
    {
        public static IEnumerable<T> Get(Func<T, bool> predicate)
        { 
            return GetAll().Where(predicate).AsEnumerable<T>();
        }
        public static IEnumerable<T> GetAll()
        {
            IEnumerable<T> results;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CAClientConnectionString"].ConnectionString))
            {
                var meta = typeof(T).GetCustomAttributes(typeof(EntityMeta), true).FirstOrDefault() as EntityMeta;
                results = conn.Query<T>($"SELECT * FROM {meta.TableName}").AsEnumerable<T>();
            }
            return results;
        }
    } 
    public class RepCycRepo 
    {
        private RepCycRepo() {
            if (ReportingCycles == null)
                ReportingCycles = Load();
        }
        public readonly IEnumerable<ReportingCycle> ReportingCycles;
        public IEnumerable<ReportingCycle> Query(Func<ReportingCycle, bool> predicate) {
            return ReportingCycles.Where(predicate).AsEnumerable();
        }
        private static readonly Lazy<RepCycRepo> _instance = new Lazy<RepCycRepo>(() => new RepCycRepo()); 
        public static RepCycRepo Instance
        {
            get
            {
                return _instance.Value; 
            }
        } 
        private IEnumerable<ReportingCycle> Load()
        {
            IEnumerable<ReportingCycle> result; 
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
                    WHERE RC.Year > 2018
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
            return result.AsEnumerable();
        }
    }  
    public class FismaSections
    {
        public static IEnumerable<QuestionGroup> GetAll(string PKFORM)
        {
            var rc = RepCycRepo.Instance.Query(q => q.FormMaster.PK_Form == PKFORM);
            var sections = (from r in rc
                            select r.FormMaster.QuestionGroups)
                            .SelectMany(qg => qg).OrderBy(s => s.sortpos).ToList();
            return sections;
        }
    }
}
