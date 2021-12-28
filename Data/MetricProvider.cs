using CyberScope.Tests;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace CyberScope.Data.Tests 
{
    internal static class MetricProvider
    {
        #region PROPS

        #endregion
        
        #region Methods

        public static IEnumerable<Metric> GetItems(OrgSubmission orgsub)
        {
            var questiongroups = Repository<QuestionGroup>.Get(qg => qg.PK_Form == orgsub.PK_Form);
            var questions = Repository<Question>.GetAll();
            var answers = Repository<FsmaAnswer>.Get(a => a.FK_OrgSubmission == orgsub.PK_OrgSubmission);

            var metrics = (from q in questions
                           join qg in questiongroups on q.FK_QuestionGroup equals qg.PK_QuestionGroup
                           join ans in answers on q.PK_Question equals ans.FK_Question into q_ans
                           from q_a in q_ans.DefaultIfEmpty(new FsmaAnswer() { Answer = null })
                           where qg.PK_Form == orgsub.PK_Form && q.identifier_text != ""
                           select new Metric
                           {
                               FsmaAnswer = q_a,
                               Question = q,
                               OrgSubmission = orgsub
                           }).ToList();
            return metrics;
        }
        #endregion
    }
}
